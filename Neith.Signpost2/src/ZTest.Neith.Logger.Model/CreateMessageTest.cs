using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace ZTest.Neith.Logger.Model
{
    [TestClass]
    public class CreateMessageTest
    {
        [TestMethod]
        public void CreateRegester1()
        {
            var sender = new GrowlConnector(null, "CreateMessageTest", 12345);
            var note = new Notification("AppName","NoteName","id12345","title","text");
            bool waitForCallback;
            var mb = sender.CreateNotify(note, null, null, out waitForCallback);
            var message = Encoding.UTF8.GetString(mb.GetBytes());
            Assert.IsTrue(message.StartsWith(CreateRegester1Text));
        }

        private const string CreateRegester1Text = @"GNTP/1.0 NOTIFY NONE 
Application-Name: AppName
Notification-ID: id12345
Notification-Title: title
Notification-Text: text
Notification-Sticky: No
Notification-Priority: 0
Notification-Coalescing-ID: 
Notification-Name: NoteName";


        [TestMethod]
        public void CreateRegester2()
        {
            var pass = "abc";
            var sender = new GrowlConnector(pass, "CreateMessageTest", 12345);
            var note = new Notification("AppName", "NoteName", "id12345", "title", "text");
            bool waitForCallback;
            var mb = sender.CreateNotify(note, null, null, out waitForCallback);
            var message = Encoding.UTF8.GetString(mb.GetBytes());
            var match = Regex.Match(message, @"SHA1:([0-9A-F]+).([0-9A-F]+)");
            Assert.IsTrue(match.Success);
            var hash = match.Groups[1].Value;
            var salt = match.Groups[2].Value;
            Key matchKey;
            Assert.IsTrue(Key.Compare(pass, hash, salt, Cryptography.HashAlgorithmType.SHA1, Cryptography.SymmetricAlgorithmType.PlainText, out matchKey));

        }

        [TestMethod]
        public void CreateRegester3()
        {
            var pass = "abcdefgh";
            var sender = new GrowlConnector(pass, "CreateMessageTest", 12345);
            sender.KeyHashAlgorithm = Cryptography.HashAlgorithmType.SHA256;
            sender.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;
            var note = new Notification("AppName", "NoteName", "id12345", "title", "text");
            bool waitForCallback;
            var mb = sender.CreateNotify(note, null, null, out waitForCallback);
            var message = Encoding.UTF8.GetString(mb.GetBytes());
            var matchHash = regExMessageHeader_Remote.Match(message);
            Assert.IsTrue(matchHash.Success);
            Assert.AreEqual("AES", matchHash.Groups["EncryptionAlgorithm"].Value);
            Assert.AreEqual("SHA256", matchHash.Groups["KeyHashAlgorithm"].Value);
            var iv = matchHash.Groups["IV"].Value;
            var hash = matchHash.Groups["KeyHash"].Value;
            var salt = matchHash.Groups["Salt"].Value;
            Key matchKey;
            Assert.IsTrue(Key.Compare(pass, hash, salt, Cryptography.HashAlgorithmType.SHA256, Cryptography.SymmetricAlgorithmType.AES, out matchKey));

        }

        /// <summary>
        /// Regex used to parse GNTP headers for non-local requests (password required)
        /// </summary>
        private static Regex regExMessageHeader_Remote = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))\s+|((?<EncryptionAlgorithm>\S+)\s+))(?<KeyHashAlgorithm>(\S+)):(?<KeyHash>(\S+))\.(?<Salt>(\S+))\s*[\r\n]");

    }
}
