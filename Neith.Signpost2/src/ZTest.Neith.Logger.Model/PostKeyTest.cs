using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neith.Interop.Win32;

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
