using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.EffiProz;



namespace Neith.Logger.Test
{
    using NUnit.Framework;

    //[TestFixture]
    public class SQLTest
    {
        [Test]
        public void Test1()
        {
            string connString = "Connection Type=Memory ; Initial Catalog=TestDB; User=sa; Password=; auto shutdown=True;";
            using (var conn = new EfzConnection(connString)) {
                conn.Open();
                var cmd = conn.CreateCommand();

                // CREATE
                cmd.CommandText = "CREATE TABLE TEST_TABLE ( COLA INTEGER, COLB VARCHAR(16), COLC TIMESTAMP )";
                cmd.ExecuteNonQuery();

                // INSERT
                cmd.CommandText = "INSERT INTO TEST_TABLE ( COLA, COLB, COLC ) VALUES (123,'ABC','2008-12-31 18:19:20' )";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO TEST_TABLE ( COLA, COLB, COLC ) VALUES (124,'DEF', '2009-11-16 13:35:36.12345678' )";
                cmd.ExecuteNonQuery();

                // SELECT
                //cmd.CommandText = "SELECT t0.COLA, t0.COLB, t0.COLC FROM TEST_TABLE AS t0 WHERE t0.COLB = 'DEF'";
                //cmd.CommandText = "SELECT [t0].[COLA], [t0].[COLB], [t0].[COLC] FROM [TEST_TABLE] AS [t0] WHERE [t0].[COLB] = 'DEF'";
                cmd.CommandText = @"SELECT ""t0"".""COLA"", ""t0"".""COLB"", ""t0"".""COLC"" FROM ""TEST_TABLE"" AS ""t0"" WHERE ""t0"".""COLB"" = 'DEF'";
                var reader = cmd.ExecuteReader();
                var items = from a in reader.AsEnumerable()
                            let A = a.GetInt32(a.GetOrdinal("COLA"))
                            let B = a.GetString(a.GetOrdinal("COLB"))
                            let C = a.GetDateTime(a.GetOrdinal("COLC"))
                            select new { A, B, C };
                Debug.WriteLine("ITEM= " + items.First().ToString());




            }
        }

    }
}
