using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Concurrency;
using System.Diagnostics;
using System.Threading;
using Neith.Util.IO;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    //[TestFixture]
    public class FileWatcherTest
    {
        [Test]
        public void RxWatchTest()
        {
            var dir = Path.GetFullPath("RxWatchTest");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            try {
                using (var watcher = new FileSystemWatcher(dir))
                using (var rxWatch = RxWatchTask(watcher))
                using (var pushd = new Pushd(dir)) {
                    watcher.EnableRaisingEvents = true;
                    Debug.WriteLine(" DIR: " + dir);
                    for (var i = 0; i < 10; i++) {
                        var name = Path.GetFullPath("file" + i.ToString());
                        var name2 = Path.GetFullPath("f2_" + i.ToString());
                        Debug.WriteLine("make: " + name);
                        File.WriteAllText(name, "作った");
                        File.AppendAllText(name, "追加した");
                        File.Move(name, name2);
                        File.Delete(name2);
                        Thread.Sleep(10);
                    }
                }
            }
            finally {
                Directory.Delete(dir, true);
            }
        }

        private static IDisposable RxWatchTask(FileSystemWatcher watcher)
        {
            return watcher
                .RxAllWatch()
                .ObserveOn(Scheduler.ThreadPool)
                .Select(a=>a.EventArgs)
                .Select(a =>string.Format("  watch=> Action:{0} FullPath:{1} Name:{2}",a.ChangeType,a.FullPath,a.Name))
                .Subscribe(a => { Debug.WriteLine(a); })
                ;
        }


    }
}
