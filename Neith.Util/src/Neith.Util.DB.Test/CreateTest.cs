using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EffiProz;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neith.Util.DB.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class CreateTest
    {
        [Test]
        public void Connect1()
        {
            //var filePath = "Test/Data/SampleDB";
            //var connString = "Connection Type=File ; Initial Catalog=" + filePath + "; User=sa; Password=;";  
            var connString = "Connection Type=Memory ; Initial Catalog=TestDB; User=sa; Password=;";
            using (var conn = new EfzConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE Test(ID INT PRIMARY KEY, Name VARCHAR(100));";
                cmd.ExecuteNonQuery();


                cmd.CommandText = "INSERT INTO Test(ID , Name) VALUES(@ID , @Name);";
                var id = cmd.CreateParameter();
                id.ParameterName = "@ID";
                id.Value = 1;
                cmd.Parameters.Add(id);

                var name = cmd.CreateParameter();
                name.ParameterName = "@NAME";
                name.Value = "Van";
                cmd.Parameters.Add(name);
                cmd.ExecuteNonQuery();

                id.Value = 2;
                name.Value = "Car";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT * FROM TEST;";
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Debug.WriteLine(String.Format("ID= {0} , Name= {1}",
                         reader.GetInt32(0), reader.GetString(1)));
                }





            }
        }
    }
}
