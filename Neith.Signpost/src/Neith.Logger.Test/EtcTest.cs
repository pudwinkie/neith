using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class EtcTest
    {
        [Test]
        public void Is64()
        {
            // このソフトは64bitで確実に動作することを確認するために
            // ユニットテストに64試験を組み込みます。
            Assert.IsTrue(Environment.Is64BitProcess);
        }

        [Test]
        public void AllProcessWindows()
        {
            var q1 = from p in Process.GetProcesses()
                     let Name = p.ProcessName
                     let IsWindow = (p.MainWindowHandle != IntPtr.Zero)
                     let WinName = p.MainWindowTitle
                     select new { Process = p, Name, IsWindow, WinName };
            var q2 = q1.ToArray();

            foreach (var a in q2) {
                var mes = string.Format("[{0}]", a.Name);
                if (a.IsWindow == true) mes += string.Format("  WIN[{0}]", a.WinName);
                Debug.WriteLine(mes);
            }

            var q3 = from p in q2.Where(a => a.IsWindow)
                     let Rect = p.Process.GetWindowRect()
                     select new { p.WinName, Rect };

            foreach (var a in q3) {
                Debug.WriteLine(a);
            }


        }
    }
}
