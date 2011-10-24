using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FFXIVRuby
{
    public class LogStatusSearcher
    {
        // Fields
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private FFXIVLogReader _stat;
        private FFXIVProcess ffxiv;
        private Regex reLogEntry = new Regex(@"[0-9A-F]{4}::\w+|^[0-9A-F]{4}:[()\w\s\0]{32}:");
        private List<Thread> threadList = new List<Thread>();
        private const long READ_BLOCK_SIZE = 0x10000;
        private static readonly Encoding TextEncoding = Encoding.UTF8;

        // Events
        /// <summary>
        /// 検索完了イベント。
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// ステータス発見イベント。
        /// </summary>
        public event LogStatusFoundEventHandler LogStatusFound;

        private void OnLogStatusFound(FFXIVLogReader stat)
        {
            logger.Trace("OnLogStatusFound({0})", stat);

            _stat = stat;
            if (LogStatusFound != null) {
                LogStatusFound(this, new LogStatusFoundEventArgs(stat));
            }
            if (Completed != null) {
                Completed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="_ffxiv"></param>
        public LogStatusSearcher(FFXIVProcess _ffxiv)
        {
            this.ffxiv = _ffxiv;
        }


        public FFXIVLogReader Search()
        {
            _stat = null;
            threadList.Clear();
            StartSearch();
        Label_0018:
            if (this._stat != null) return _stat;

            foreach (Thread thread in threadList) {
                if (thread.IsAlive) {
                    Thread.Sleep(100);
                    goto Label_0018;
                }
            }
            return null;
        }

        public void StartSearch()
        {
            new Memory();
            foreach (var info in this.ffxiv.GetMemoryBasicInfos()) {
                if (this._stat != null) return;

                if (info.Protect == Memory.MEMORY_ALLOCATION_PROTECT.PAGE_READWRITE) {
                    for (int i = (int)((uint)info.BaseAddress); i < (((uint)info.BaseAddress) + ((int)info.RegionSize)); i += 0x10000) {
                        int num2 = 0x10000;
                        if ((i + num2) > (((uint)info.BaseAddress) + ((int)info.RegionSize))) {
                            num2 = (((int)((uint)info.BaseAddress)) + ((int)info.RegionSize)) - i;
                        }
                        // スレッドが１０個を超えたら、終了スレッドを管理外に
                        while (this.threadList.Count > 10) {
                            for (int j = this.threadList.Count - 1; j >= 0; j--) {
                                if (!this.threadList[j].IsAlive) {
                                    this.threadList.RemoveAt(j);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        Thread thread = new Thread(() => { SearchTask(i, num2); });
                        thread.Priority = ThreadPriority.BelowNormal;
                        thread.Start();
                        this.threadList.Insert(0, thread);
                    }
                }
            }
        }

        private void SearchTask(int ent, int size)
        {
            var buffer = this.ffxiv.ReadBytes((IntPtr)ent, size);
            var input = new MemoryStream();
            input.Write(buffer, 0, buffer.Length);
            input.Position = 0L;
            var reader = new BinaryReader(input);
            for (int i = 0; i < (size - 4); i += 4) {
                if (this._stat != null) {
                    return;
                }
                var address = reader.ReadInt32() + 0x40;
                var num3 = ffxiv.ReadInt32((IntPtr)address);
                if ((num3 >= 0x10000) && (num3 <= 0x7ffff000)) {
                    var bytes = this.ffxiv.ReadBytes((IntPtr)num3, 5);
                    if (((bytes[4] == 0x3a) && (bytes[0] == 0x30)) && (bytes[1] == 0x30)) {
                        bytes = this.ffxiv.ReadBytes((IntPtr)num3, 50);
                        string str = TextEncoding.GetString(bytes);
                        if (this.reLogEntry.IsMatch(str)) {
                            var status = new FFXIVLogReader(this.ffxiv, (IntPtr)address);
                            OnLogStatusFound(status);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// PLINQ版メモリサーチ。
        /// </summary>
        /// <returns></returns>
        public FFXIVLogReader SearchPLINQ(CancellationToken token)
        {
            _stat = null;
            var st = EnRangePair(token)
                .AsParallel()
                .SelectMany(a => EnSearch(a.EntryAddress, a.Size, 0x40, token))
                .FirstOrDefault();
            if (st != null) OnLogStatusFound(st);
            _stat = st;
            return st;
        }

        private struct SearchRange
        {
            public IntPtr EntryAddress;
            public int Size;
            public SearchRange(long ent, long size) { EntryAddress = new IntPtr(ent); Size = (int)size; }
        }

        /// <summary>
        /// 検索範囲の列挙。64KB単位で切り出す。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SearchRange> EnRangePair(CancellationToken token)
        {
            foreach (var info in ffxiv.GetMemoryBasicInfos()) {
                // ワーク領域以外はスキャン対象外
                if (info.Protect != Memory.MEMORY_ALLOCATION_PROTECT.PAGE_READWRITE) continue;

                // 64K単位で区切る
                var start = (long)info.BaseAddress;
                if (!IsValidAddress(start)) continue;
                var size = (long)info.RegionSize;
                var end = start + size;
                for (var i = start; i < end; i += READ_BLOCK_SIZE) {
                    if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                    var num2 = READ_BLOCK_SIZE;
                    if ((i + num2) > end) num2 = end - i;
                    yield return new SearchRange(i, num2);
                }
            }
        }

        /// <summary>
        /// 検索処理本体
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="size"></param>
        private IEnumerable<FFXIVLogReader> EnSearch(IntPtr ent, int size, int offset, CancellationToken token)
        {
#if true
            var tid = Thread.CurrentThread.ManagedThreadId;
            logger.Trace("EnSearch[{0,2}](0x{1,8:X}, 0x{2,6:X})",
                tid, ent, size);
#endif
            foreach (var ptr in ffxiv.ReadBytesOrNull(ent, size).EnReadInt32()) {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                // ptr値が妥当なアドレスか？
                if (!IsValidAddress(ptr)) continue;

                // ptr+offsetの場所に格納されているアドレスが、
                // 有効なアドレス値範囲にいなければ除外
                var logEntry = ptr + offset;
                var logAddr = ffxiv.ReadInt32OrZero((IntPtr)logEntry);
                if (!IsValidAddress(logAddr)) continue;

                // 見つけたアドレスから５バイトをチェック
                // '00??:' 以外の場合に除外
                var c1 = this.ffxiv.ReadBytesOrNull((IntPtr)logAddr, 5);
                if (c1 == null) continue;
                if (c1[4] != 0x3a) continue;
                if (c1[0] != 0x30) continue;
                if (c1[1] != 0x30) continue;

                // 見つけたアドレスから50バイトをチェック
                // 正規表現のパターンマッチで不一致なら除外
                var c2 = this.ffxiv.ReadBytesOrNull((IntPtr)logAddr, 50);
                if (c2 == null) continue;
                var str = Encoding.UTF8.GetString(c2);
                if (!reLogEntry.IsMatch(str)) continue;

                // 最終チェック
                var reader = new FFXIVLogReader(this.ffxiv, (IntPtr)logEntry);


                // 確定
                yield return reader;
                yield break;
            }
        }

        /// <summary>
        /// 値が有効なアドレスならtrue
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private static bool IsValidAddress(long addr)
        {
            if (addr < 0x00010000) return false;
            if (addr > 0x7ffff000) return false;
            return true;
        }


        /// <summary>
        /// PLINQ版テキストサーチ。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<IntPtr, int>> SearchPLINQ(string text, int minOffset, int maxOffset, int nest, CancellationToken token)
        {
            return EnRangePair(token)
                .AsParallel()
                .SelectMany(a => EnSearchText(a.EntryAddress, a.Size, text, minOffset, maxOffset, nest, token))
                .FirstOrDefault();
        }


        /// <summary>
        /// 特定のtext(utf-8)が指定のoffset範囲でポインタ参照されているかどうかを検索
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="size"></param>
        private IEnumerable<IEnumerable<Tuple<IntPtr, int>>> EnSearchText(IntPtr ent, int size, string text, int minOffset, int maxOffset, int nest, CancellationToken token)
        {
#if true
            var tid = Thread.CurrentThread.ManagedThreadId;
            logger.Trace("EnSearchText[{0,2}](0x{1,8:X}, 0x{2,6:X})",
                tid, ent, size);
#endif
            var data = TextEncoding.GetBytes(text).TakeWhile(a => a != 0).ToArray();
            return EnSearchBytes(ent, size, data, minOffset, maxOffset, nest, token);
        }

        /// <summary>
        /// 特定のbyte[]列が指定のoffset範囲でポインタ参照されているかどうかを検索
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="size"></param>
        private IEnumerable<IEnumerable<Tuple<IntPtr, int>>> EnSearchBytes(IntPtr ent, int size, byte[] data, int minOffset, int maxOffset, int nest, CancellationToken token)
        {
#if true
            var tid = Thread.CurrentThread.ManagedThreadId;
            logger.Trace("EnSearchBytes[{0,2}](0x{1,8:X}, 0x{2,6:X})",
                tid, ent, size);
#endif
            foreach (var ptr in ffxiv.ReadBytesOrNull(ent, size).EnReadInt32()) {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                // ptr値が妥当なアドレスか？
                if (!IsValidAddress(ptr)) continue;

                // 範囲検索
                var rc = EnSearchBytesImpl((IntPtr)ptr, data, minOffset, maxOffset, nest, token);
                if (rc != null) {
                    rc.Reverse();
                    yield return rc.ToArray();
                }
                yield break;
            }
        }

        private List<Tuple<IntPtr, int>> EnSearchBytesImpl(IntPtr ptr, byte[] data, int minOffset, int maxOffset, int nest, CancellationToken token)
        {
            // このレベルで検索
            var rc = EnSearchBytesImpl2(ptr, data, minOffset, maxOffset);
            if (rc != null) return rc;

            // 次のレベルを検索
            if (nest <= 0) return null;

            for (var offset = minOffset; offset <= maxOffset; offset += 4) {
                if (token.IsCancellationRequested) throw new OperationCanceledException(token);
                // ptr+offsetの場所に格納されているアドレスが、
                // 有効なアドレス値範囲にいなければ除外
                var entry = (IntPtr)((long)ptr + offset);
                var addr = ffxiv.ReadInt32OrZero(entry);
                if (!IsValidAddress(addr)) continue;

                var rc2 = EnSearchBytesImpl(entry, data, minOffset, maxOffset, nest - 1, token);
                if (rc2 == null) continue;
                rc2.Add(Tuple.Create(ptr, offset));
                return rc2;
            }
            return null;
        }


        private List<Tuple<IntPtr, int>> EnSearchBytesImpl2(IntPtr ptr, byte[] data, int minOffset, int maxOffset)
        {
            try {
                byte[] check = ffxiv.ReadBytesOrNull(ptr, data.Length + maxOffset);
                if (check == null) return null;
                for (var offset = 0; offset <= maxOffset; offset++) {
                    for (int i = minOffset; i < data.Length; i++) {
                        if (data[i] != check[offset + i]) goto NEXT_OFFSET;
                    }
                    var list = new List<Tuple<IntPtr, int>>();
                    list.Add(Tuple.Create(ptr, offset));
                    return list;

                NEXT_OFFSET: ;
                }
            }
            catch { }
            return null;
        }


        // Properties
        public FFXIVLogReader FFXIVLogStat { get { return this._stat; } }
    }

}