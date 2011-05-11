using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Common.Logging;

namespace FFXIVRuby
{
    public class LogStatusSearcher
    {
        // Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private FFXIVLogStatus _stat;
        private FFXIVProcess ffxiv;
        private Regex reLogEntry = new Regex(@"[0-9A-F]{4}::\w+|^[0-9A-F]{4}:[()\w\s\0]{32}:");
        private List<Thread> threadList = new List<Thread>();
        private const uint READ_BLOCK_SIZE = 0x10000;

        // Events
        /// <summary>
        /// 検索完了イベント。
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// ステータス発見イベント。
        /// </summary>
        public event LogStatusFoundEventHandler LogStatusFound;

        private void OnLogStatusFound(FFXIVLogStatus stat)
        {
            Log.TraceFormat("OnLogStatusFound({0})", stat);

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


        public FFXIVLogStatus Search()
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
            var buffer = this.ffxiv.ReadBytes(ent, size);
            var input = new MemoryStream();
            input.Write(buffer, 0, buffer.Length);
            input.Position = 0L;
            var reader = new BinaryReader(input);
            for (int i = 0; i < (size - 4); i += 4) {
                if (this._stat != null) {
                    return;
                }
                var address = reader.ReadInt32() + 0x40;
                var num3 = ffxiv.ReadInt32(address);
                if ((num3 >= 0x10000) && (num3 <= 0x7ffff000)) {
                    var bytes = this.ffxiv.ReadBytes(num3, 5);
                    if (((bytes[4] == 0x3a) && (bytes[0] == 0x30)) && (bytes[1] == 0x30)) {
                        bytes = this.ffxiv.ReadBytes(num3, 50);
                        string str = Encoding.GetEncoding("utf-8").GetString(bytes);
                        if (this.reLogEntry.IsMatch(str)) {
                            var status = new FFXIVLogStatus(this.ffxiv, address);
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
        public FFXIVLogStatus SearchPLINQ()
        {
            _stat = null;
            var st = EnRangePair()
                .AsParallel()
                .SelectMany(a => EnSearch(a.Entry, a.Size, 0x40))
                .FirstOrDefault();
            if (st != null) OnLogStatusFound(st);
            _stat = st;
            return st;
        }

        private struct SearchRange
        {
            public int Entry;
            public int Size;
            public SearchRange(int ent, int size) { Entry = ent; Size = size; }
        }

        /// <summary>
        /// 検索範囲の列挙。64KB単位で切り出す。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SearchRange> EnRangePair()
        {
            foreach (var info in ffxiv.GetMemoryBasicInfos()) {
                // ワーク領域以外はスキャン対象外
                if (info.Protect != Memory.MEMORY_ALLOCATION_PROTECT.PAGE_READWRITE) continue;

                // 64K単位で区切る
                uint start = (uint)info.BaseAddress;
                if (!IsValidAddress((int)start)) continue;
                uint size = (uint)info.RegionSize;
                uint end = start + size;
                for (uint i = start; i < end; i += READ_BLOCK_SIZE) {
                    uint num2 = READ_BLOCK_SIZE;
                    if ((i + num2) > end) num2 = end - i;
                    yield return new SearchRange((int)i, (int)num2);
                }
            }
        }

        /// <summary>
        /// 検索処理本体
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="size"></param>
        private IEnumerable<FFXIVLogStatus> EnSearch(int ent, int size, int offset)
        {
#if true
            var tid = Thread.CurrentThread.ManagedThreadId;
            Log.TraceFormat("EnSearch[{0,2}](0x{1,8:X}, 0x{2,6:X})",
                tid, ent, size);
#endif
            foreach (var ptr in ffxiv.ReadBytesOrNull(ent, size).EnReadInt32()) {
                // ptr値が妥当なアドレスか？
                if (!IsValidAddress(ptr)) continue;

                // ptr+offsetの場所に格納されているアドレスが、
                // 有効なアドレス値範囲にいなければ除外
                var logEntry = ptr + offset;
                var logAddr = ffxiv.ReadInt32OrZero(logEntry);
                if (!IsValidAddress(logAddr)) continue;

                // 見つけたアドレスから５バイトをチェック
                // '00??:' 以外の場合に除外
                var c1 = this.ffxiv.ReadBytesOrNull(logAddr, 5);
                if (c1 == null) continue;
                if (c1[4] != 0x3a) continue;
                if (c1[0] != 0x30) continue;
                if (c1[1] != 0x30) continue;

                // 見つけたアドレスから50バイトをチェック
                // 正規表現のパターンマッチで不一致なら除外
                var c2 = this.ffxiv.ReadBytesOrNull(logAddr, 50);
                if (c2 == null) continue;
                var str = Encoding.UTF8.GetString(c2);
                if (!reLogEntry.IsMatch(str)) continue;

                // 確定
                yield return new FFXIVLogStatus(this.ffxiv, logEntry);
                yield break;
            }
        }

        /// <summary>
        /// 値が有効なアドレスならtrue
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private static bool IsValidAddress(int addr)
        {
            if (addr < 0x00010000) return false;
            if (addr > 0x7ffff000) return false;
            return true;
        }

        // Properties
        public FFXIVLogStatus FFXIVLogStat { get { return this._stat; } }
    }

}