using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Study.Google.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class EnvReport
    {
        [Test]
        public void PrintEnv()
        {
            Debug.WriteLine(string.Format("Path[ApplicationData]:{0}", 
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));

        }

    }
}
