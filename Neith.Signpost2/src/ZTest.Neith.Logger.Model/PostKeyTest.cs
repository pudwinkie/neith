using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neith.Interop.Win32;
using System.Windows.Input;

namespace ZTest.Neith.Logger.Model
{
    [TestClass]
    public class PostKeyTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            'F'.SendShift();
            //"ゆにこーどしけん".SendInput();
        }
    }
}
