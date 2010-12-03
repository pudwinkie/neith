using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Community.CsharpSqlite.SQLiteClient;



namespace Neith.Logger.Test
{
    using NUnit.Framework;

    [Table(Name = "TEST_TABLE")]
    public class TestTable
    {
        [Column]
        public int COLA { get; set; }
        [Column]
        public string COLB { get; set; }
        [Column]
        public DateTime COLC { get; set; }

        public override string ToString()
        {
            return string.Format("A:{0} B:{1} C:{2}", COLA, COLB, COLC);
        }
    }


    [TestFixture]
    public class SQLiteTest
    {
        [Test]
        public void Test1()
        {
            using (var conn = new SqliteConnection("uri=file::memory:")) {
                conn.Open();
                var cmd = conn.CreateCommand();

                // CREATE
                cmd.CommandText = "CREATE TABLE TEST_TABLE ( COLA INTEGER, COLB TEXT, COLC DATETIME )";
                cmd.ExecuteNonQuery();

                // INSERT
                cmd.CommandText = "INSERT INTO TEST_TABLE ( COLA, COLB, COLC ) VALUES (123,'ABC','2008-12-31 18:19:20' )";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO TEST_TABLE ( COLA, COLB, COLC ) VALUES (124,'DEF', '2009-11-16 13:35:36.12345678' )";
                cmd.ExecuteNonQuery();

                // SELECT
                //cmd.CommandText = "SELECT t0.COLA, t0.COLB, t0.COLC FROM TEST_TABLE AS t0 WHERE t0.COLB = 'DEF'";
                cmd.CommandText = "SELECT [t0].[COLA], [t0].[COLB], [t0].[COLC] FROM [TEST_TABLE] AS [t0] WHERE [t0].[COLB] = 'DEF'";
                var reader = cmd.ExecuteReader();
                var items = from a in reader.AsEnumerable()
                            let A = a.GetInt32(a.GetOrdinal("COLA"))
                            let B = a.GetString(a.GetOrdinal("COLB"))
                            let C = a.GetDateTime(a.GetOrdinal("COLC"))
                            select new { A, B, C };
                Debug.WriteLine("ITEM= " + items.First().ToString());


                // LINQ to SQL
                var db = new DataContext(conn);
                var table = db.GetTable<TestTable>();
                var q1 = from a in table
                         where a.COLB == "DEF"
                         select a;
                Debug.WriteLine("SQL= " + q1.ToString());
                var item = q1.AsEnumerable().First();
                Debug.WriteLine("ITEM= " + item.ToString());
            }
        }

    }
}
