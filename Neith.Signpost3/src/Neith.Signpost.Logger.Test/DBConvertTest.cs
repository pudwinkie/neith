using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using Neith.Signpost.Logger.XIV;
using Wintellect.Sterling;

namespace Neith.Signpost.Logger.Test
{
    using NUnit.Framework;

    //[TestFixture]
    public class DBConvertTest
    {
        private CompositeDisposable Tasks;

        [TestFixtureTearDown]
        public void Dispose()
        {
            Tasks.Dispose();
            Tasks = null;
        }

        public SterlingEngine DBEngine { get; private set; }
        public LogDBFileInstance Database { get; private set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            Tasks = new CompositeDisposable();
            DBEngine = new SterlingEngine().Add(Tasks);
            DBEngine.Activate();
            // dbのroot
            var date = DateTime.Parse("2011/10/27");
            Database = new LogDBFileInstance(DBEngine, date,Const.DBPath);
        }

        [Test]
        public void Test1()
        {
            Database.AllLogsKV.Count.IsNot(0);
            Debug.WriteLine(string.Format("件数：{0}", Database.AllLogsKV.Count));
        }

        //[Test]
        public void TagListTest()
        {
            var tags = Database.AllLogsKV
                .Select(a => a.LazyValue.Value)
                .AsParallel()
                .Select(a => a.Body)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .SelectMany(a => reTag.Matches(a).Cast<Match>())
                .Select(m => m.Value);
            var q2 = from a in tags group a by a;
            var q3 = from a in q2 select new { Key = a.Key, Count = a.Count() };
            var tagDic = q3.ToDictionary(a => a.Key, a => a.Count);

            foreach (var pair in tagDic.OrderBy(a => a.Key)) {
                Debug.WriteLine(string.Format("Tag：{0}  count={1}", pair.Key, pair.Value));
            }
        }

        private readonly Regex reTag = new Regex(@"\{02([0-9A-F]{2})+\}", RegexOptions.Compiled);


        [Test]
        public void ToMicroDataTest()
        {
            var items = Database.AllLogsKV
                .Select(a => a.LazyValue.Value)
                .Select(a => a.Source)
                .OfType<FFXIVRuby.FFXIVLog>()
                .Where(a => !a.Message.Contains('\u0003'))
                .Select(a => a.ToMicroData());
            using (var writer = File.CreateText(Const.InputLogPath)) {
                writer.WriteLine(MICRO_DATA_HTML_HEADER);
                foreach (var el in items) {
                    writer.WriteLine(el.ToString());
                }
            }
        }

        private const string MICRO_DATA_HTML_HEADER = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
  <meta charset=""utf-8"" />
  <title>title</title>
  <link rel=""stylesheet"" href=""microdata.css"" />
</head>
<body>";

    }
}
