using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DbvCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBV"].ConnectionString);

            try
            {
                connection.Open();

                var results = connection.Query<DbvEntry>("select * from Dbv_Repository");
                foreach (var r in results)
                {
                    Console.Out.WriteLine("r.N = {0} {1} {2}", r.Name, r.Id, r.Data != null ? r.Data.Length : 0);
                }


                var cf = new ConnectionFactory();

                var cft = new ConnectionTransactionFactory();



                var rep = new DbvFinderRepository(cf, "DBV_REPOSITORY");

                var e = rep.GetById(1);
                Console.Out.WriteLine("e.Name {0}, e.Id = {1}", e.Name, e.Id);

                var e1 = rep.GetById(1);
                Console.Out.WriteLine("e1.Name {0}, e1.Id = {1}", e1.Name, e1.Id);


                var wrep = new DbvWriterRepository(cf, "DBV_REPOSITORY");

                e.Data = Encoding.UTF8.GetBytes("JUST A TEST");
                e.UserName = "bscholze";
                e.CreatedTms = DateTime.Now;
                wrep.Update(e);


                var ne = new DbvEntry()
                {
                    Name = "/test/test.csv",
                    UserName = "michael",
                    CreatedTms = DateTime.Now,
                    Description = "",
                    Tags = "",
                    Data = Encoding.UTF8.GetBytes("ANOTHER TAGS"),
                };

                wrep.Add(ne);


                var sql = @"Select * from dbv_repository";

                var cmd = connection.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;
                cmd.Connection = connection;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.Out.WriteLine("reader[0] = {0} {1} {2} {3} {4}", reader[0],reader[1],reader[2], reader[3],reader[4]);
                    }

                }



                connection.Close();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("ex = {0}", ex);
            }

            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();


        }
    }
}
