using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Neith.Logger.Test
{
    using NUnit.Framework;

    //[TestFixture]
    public class LocalTestTest
    {
        static readonly XName LOCALTEST = XName.Get("localtest");
        static readonly XName mail_smtp_server = XName.Get("mail.smtp.server");
        static readonly XName mail_smtp_port = XName.Get("mail.smtp.port");
        static readonly XName mail_pop3_server = XName.Get("mail.pop3.server");
        static readonly XName mail_pop3_port = XName.Get("mail.pop3.port");
        static readonly XName mail_pop3_user = XName.Get("mail.pop3.user");
        static readonly XName mail_pop3_pass = XName.Get("mail.pop3.pass");


        [Test]
        public void ReadTest()
        {
            using (var st = File.OpenRead(@"..\..\..\..\..\..\local-neith\local.xml")) {
                var doc = XDocument.Load(st, LoadOptions.None);
                var el = doc.Elements(LOCALTEST).First();
                var smtp_server = el.Attribute(mail_smtp_server).Value;
                if (string.IsNullOrEmpty(smtp_server)) Assert.Fail();
            }
            if (string.IsNullOrEmpty(LocalTestData.Mail.Pop3.Server)) Assert.Fail();
        }
    }
}
