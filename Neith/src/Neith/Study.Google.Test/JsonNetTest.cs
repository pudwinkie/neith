using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Study.Google.Test
{
    using NUnit.Framework;
    [TestFixture]
    public class JsonNetTest
    {
        private const string jsondoc =
@"{
	""J"": ""hello1"",
	""S"": ""hello2"",
	""O STRING"": ""hello3"",
	""N ARRAY"": [""J"",""S"",""O"",""N\nN""]
}
";

        [Test]
        public void JsonTest1()
        {





        }


    }
}
