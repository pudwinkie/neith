using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neith.Growl.Daemon;
using Neith.Growl.Connector;

namespace ZTest.Neith.Logger.Model
{
    [TestClass]
    public class ParseTest
    {
        private const string TestMessage = @"GNTP/1.0 REGISTER NONE
Application-Name: SurfWriter 
Application-Icon: http://www.site.org/image.jpg 
X-Creator: Apple Software 
X-Application-ID: 08d6c05a21512a79a1dfeb9d2a8f262f 
Notifications-Count: 2 

Notification-Name: Download Complete 
Notification-Display-Name: Download completed 
Notification-Icon: x-growl-resource://cb08ca4a7bb5f9683c19133a84872ca7 
Notification-Enabled: True 
X-Language: English 
X-Timezone: PST 

Notification-Name: Document Published 
Notification-Display-Name: Document successfully published 
Notification-Icon: http://fake.net/image.png 
Notification-Enabled: False 
X-Sound: http://fake.net/sound.wav 
X-Sound-Alt: x-growl-resource://f082d4e3bdfe15f8f5f2450bff69fb17 

Identifier: cb08ca4a7bb5f9683c19133a84872ca7 
Length: 4 

ABCD

Identifier: f082d4e3bdfe15f8f5f2450bff69fb17 
Length: 16

FGHIJKLMNOPQRSTU

";
        private static readonly byte[] TestBytes = Encoding.UTF8.GetBytes(TestMessage);


        [TestMethod]
        public void GNTPParserTest()
        {
            var pw = new PasswordManager();
            var info = new RequestInfo();
            var parser = new GNTPParser(pw, false, true, true, true, info);
            GNTPRequest reqData = null;
            parser.MessageParsed += (req) =>
            {
                reqData = req;
            };
            parser.Error += (error) =>
            {
                Assert.Fail("ErrorCode={0}, Description={1}", error.ErrorCode, error.ErrorDescription);
            };
            var remain = TestBytes.Length;
            var index = 0;
            var length = 0;
            while (true) {
                var data = GetBytes(index,length);
                var s = Encoding.UTF8.GetString(data);
                var next = parser.Parse(data);
                if (!next.ShouldContinue) break;
                index += length;
                length = GetLength(next, index);
            }
            Assert.IsNotNull(reqData);
        }

        private static byte[] GetBytes(int index, int length)
        {
            return TestBytes.Skip(index).Take(length).ToArray();
        }

        private static int GetLength(NextIndicator next,int index)
        {
            if (next.UseLength) return next.Length;
            var bytes = next.Bytes;
            var bytesLength = bytes.Length;
            var length = 0;
            while (true) {
                if ((index + length + bytesLength) > TestBytes.Length) return TestBytes.Length - index;
                var scan = TestBytes.Skip(index + length).Take(bytesLength);
                if (scan.SequenceEqual(bytes)) return length + bytesLength;
                length++;    
            }
        }

    }
}
