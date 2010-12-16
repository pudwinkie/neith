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
        public event EventHandler Completed;

        // Methods
        public LogStatusSearcher(FFXIVProcess _ffxiv)
        {
            this.ffxiv = _ffxiv;
        }

        private void _Search(object o)
        {
            object[] objArray = (object[])o;
            this._Search((int)objArray[0], (int)objArray[1]);
        }

        private void _Search(int ent, int size)
        {
            byte[] buffer = this.ffxiv.ReadBytes(ent, size);
            MemoryStream input = new MemoryStream();
            input.Write(buffer, 0, buffer.Length);
            input.Position = 0L;
            BinaryReader reader = new BinaryReader(input);
            for (int i = 0; i < (size - 4); i += 4) {
                if (this._stat != null) {
                    return;
                }
                int address = reader.ReadInt32() + 0x40;
                int num3 = this.ffxiv.ReadInt32(address);
                if ((num3 >= 0x10000) && (num3 <= 0x7ffff000)) {
                    byte[] bytes = this.ffxiv.ReadBytes(num3, 5);
                    if (((bytes[4] == 0x3a) && (bytes[0] == 0x30)) && (bytes[1] == 0x30)) {
                        bytes = this.ffxiv.ReadBytes(num3, 50);
                        string str = Encoding.GetEncoding("utf-8").GetString(bytes);
                        if (this.regex1.IsMatch(str)) {
                            FFXIVLogStatus status = new FFXIVLogStatus(this.ffxiv, address);
                            this._stat = status;
                            if (this.Completed != null) {
                                this.Completed(this, null);
                            }
                        }
                    }
                }
            }
        }

        public FFXIVLogStatus Search()
        {
            this._stat = null;
            this.threadList.Clear();
            this.StartSearch();
        Label_0018:
            if (this._stat != null) {
                return this._stat;
            }
            foreach (Thread thread in this.threadList) {
                if (thread.IsAlive) {
                    Thread.Sleep(100);
                    goto Label_0018;
                }
            }
            return null;
        }

        public void StartSearch()
        {
            ParameterizedThreadStart start = new ParameterizedThreadStart(this._Search);
            new Memory();
            foreach (Memory.MEMORY_BASIC_INFORMATION memory_basic_information in this.ffxiv.GetMemoryBasicInfos()) {
                if (this._stat != null) {
                    return;
                }
                if (memory_basic_information.Protect == Memory.MEMORY_ALLOCATION_PROTECT.PAGE_READWRITE) {
                    for (int i = (int)((uint)memory_basic_information.BaseAddress); i < (((uint)memory_basic_information.BaseAddress) + ((int)memory_basic_information.RegionSize)); i += 0x10000) {
                        int num2 = 0x10000;
                        if ((i + num2) > (((uint)memory_basic_information.BaseAddress) + ((int)memory_basic_information.RegionSize))) {
                            num2 = (((int)((uint)memory_basic_information.BaseAddress)) + ((int)memory_basic_information.RegionSize)) - i;
                        }
                        while (this.threadList.Count > 10) {
                            for (int j = this.threadList.Count - 1; j >= 0; j--) {
                                if (!this.threadList[j].IsAlive) {
                                    this.threadList.RemoveAt(j);
                                }
                            }
                            Thread.Sleep(100);
                        }
                        Thread item = new Thread(start);
                        item.Priority = ThreadPriority.BelowNormal;
                        item.Start(new object[] { i, num2 });
                        this.threadList.Insert(0, item);
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