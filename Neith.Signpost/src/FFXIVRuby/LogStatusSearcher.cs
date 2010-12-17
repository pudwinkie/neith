using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace FFXIVRuby
{
    public class LogStatusSearcher
    {
        // Fields
        private int _size = 0x70000000;
        private int _start = 0x10000;
        private FFXIVLogStatus _stat;
        private int _step = 0x1000;
        private FFXIVProcess ffxiv;
        private bool IsCompleted;
        private Regex regex1 = new Regex(@"[0-9A-F]{4}::\w+|^[0-9A-F]{4}:[()\w\s\0]{32}:");
        private List<Thread> threadList = new List<Thread>();

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
                        if (this.regex1.IsMatch(str)) {
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
        public FFXIVLogStatus Search2()
        {
            _stat = null;
            var st = EnRangePair()
                .AsParallel()
                .SelectMany(a => EnSearch(a.Item1, a.Item2))
                .FirstOrDefault();
            if (st != null) OnLogStatusFound(st);
            _stat = st;
            return st;
        }

        /// <summary>
        /// 検索範囲の列挙
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<int, int>> EnRangePair()
        {
            new Memory();
            foreach (var info in ffxiv.GetMemoryBasicInfos()) {
                if (info.Protect == Memory.MEMORY_ALLOCATION_PROTECT.PAGE_READWRITE) {
                    for (int i = (int)((uint)info.BaseAddress); i < (((uint)info.BaseAddress) + ((int)info.RegionSize)); i += 0x10000) {
                        int num2 = 0x10000;
                        if ((i + num2) > (((uint)info.BaseAddress) + ((int)info.RegionSize))) {
                            num2 = (((int)((uint)info.BaseAddress)) + ((int)info.RegionSize)) - i;
                        }
                        yield return new Tuple<int, int>(i, num2);
                    }
                }
            }
        }

        /// <summary>
        /// 検索処理本体
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="size"></param>
        private IEnumerable<FFXIVLogStatus> EnSearch(int ent, int size)
        {
            using (var input = new MemoryStream(ffxiv.ReadBytes(ent, size)))
            using (var reader = new BinaryReader(input)) {
                for (int i = 0; i < (size - 4); i += 4) {
                    var address = reader.ReadInt32() + 0x40;
                    var num3 = ffxiv.ReadInt32(address);
                    if (num3 < 0x10000) continue;
                    if (num3 > 0x7ffff000) continue;

                    var check1 = this.ffxiv.ReadBytes(num3, 5);
                    if (check1[4] != 0x3a) continue;
                    if (check1[0] != 0x30) continue;
                    if (check1[1] != 0x30) continue;

                    var check2 = this.ffxiv.ReadBytes(num3, 50);
                    string str = Encoding.GetEncoding("utf-8").GetString(check2);
                    if (this.regex1.IsMatch(str)) {
                        yield return new FFXIVLogStatus(this.ffxiv, address);
                        yield break;
                    }
                }
            }
        }

        // Properties
        public FFXIVLogStatus FFXIVLogStat
        {
            get { return this._stat; }
            set
            {
                if (this._stat != null) {
                    this._stat = value;
                }
            }
        }
    }

}