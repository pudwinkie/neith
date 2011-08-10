using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Common
{
    public static class DbDataReaderExtensions
    {
        public static IEnumerable<IDataReader> AsEnumerable(this DbDataReader reader)
        {
            while (reader.Read()) yield return reader;
        }
    }
}
