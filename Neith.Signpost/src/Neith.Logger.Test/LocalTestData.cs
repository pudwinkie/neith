using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Neith.Logger.Test
{
    public static class LocalTestData
    {
        public static class Mail
        {
            public static class Smtp
            {
                static Smtp() { StaticInit(); }
                public static string Server { get; internal set; }
                public static int Port { get; internal set; }
            }
            public static class Pop3
            {
                static Pop3() { StaticInit(); }
                public static string Server { get; internal set; }
                public static int Port { get; internal set; }
                public static string User { get; internal set; }
                public static string Pass { get; internal set; }
            }
        }

        static LocalTestData()
        {
            using (var st = File.OpenRead(@"..\..\..\..\..\..\local-neith\local.xml")) {
                var doc = XDocument.Load(st, LoadOptions.None);
                var el = doc.Elements("localtest").First();
                Mail.Smtp.Server = el.Attribute("mail.smtp.server").Value;
                Mail.Smtp.Port = el.Attribute("mail.smtp.port").Value.ToInt();
                Mail.Pop3.Server = el.Attribute("mail.pop3.server").Value;
                Mail.Pop3.Port = el.Attribute("mail.pop3.port").Value.ToInt();
                Mail.Pop3.User = el.Attribute("mail.pop3.user").Value;
                Mail.Pop3.Pass = el.Attribute("mail.pop3.pass").Value;
            }
        }

        internal static void StaticInit() { }


        private static int ToInt(this string value)
        {
            int rc;
            int.TryParse(value, out rc);
            return rc;
        }

    }
}
