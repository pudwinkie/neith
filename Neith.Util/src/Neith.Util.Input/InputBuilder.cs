using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Neith.Util.Input
{
    public class InputBuilder
    {
        private struct KeyItem
        {
            public readonly VK vk;
            public readonly KEYEVENTF flags;
            public KeyItem(VK vk, KEYEVENTF flags)
            {
                this.vk = vk;
                this.flags = flags;
            }
        }
        private readonly Queue<KeyItem> EventQueue = new Queue<KeyItem>();

        /// <summary>送信イベント数</summary>
        public int SendCount { get; private set; }

        /// <summary>キューのイベント数</summary>
        public int QueueCount { get { return EventQueue.Count; } }

        /// <summary>
        /// キーイベントを追加します。
        /// </summary>
        /// <param name="vk"></param>
        /// <param name="flags"></param>
        public void Add(VK vk, KEYEVENTF flags)
        {
            EventQueue.Enqueue(new KeyItem(vk, flags));
        }

        /// <summary>
        /// キューを全て出力し、指定時間待ちます。
        /// </summary>
        /// <param name="dueTime"></param>
        /// <returns></returns>
        public async Task<int> Flush(TimeSpan dueTime)
        {
            var inputs = new API.Input[QueueCount];
            var layout = API.GetKeyboardLayout(0);
            //var exInfo = IntPtr.Zero;
            var exInfo = API.GetMessageExtraInfo();

            for (var i = 0; i < inputs.Length; i++) {
                var item = EventQueue.Dequeue();
                var vk = item.vk;
                var flags = item.flags;
                var scan = (ushort)API.MapVirtualKeyEx((int)vk, API.MAPVK_VK_TO_VSC, layout);

                inputs[i].type = API.INPUT_KEYBOARD;
                inputs[i].ki.wVk = vk;
                inputs[i].ki.wScan = scan;
                inputs[i].ki.dwFlags = flags;
                inputs[i].ki.dwExtraInfo = exInfo;
                inputs[i].ki.time = 0;
            }

            var rc = API.SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(API.Input)));
            SendCount += inputs.Length;

            if (dueTime > TimeSpan.Zero) await TaskEx.Delay(dueTime);
            else await TaskEx.Yield();
            return rc;
        }

        public Task<int> Flush() { return Flush(TimeSpan.Zero); }
        public Task<int> Flush(int dueTime) { return Flush(TimeSpan.FromMilliseconds(dueTime)); }
    }
}
