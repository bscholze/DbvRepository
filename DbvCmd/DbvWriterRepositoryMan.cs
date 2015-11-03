using System;
using System.Data;
using System.Data.SqlClient;
using RepositoryWrapper;
using RepositoryWrapper.Dapper;

namespace DbvCmd
{
    public class DbvWriterRepositoryMan : IWriteRepository<DbvEntry>
    {

        private string _updateSql = @"UPDATE {0} SET Name=@Name,Tags=@Tags,Description=@Description,[Data]=@ImageData,[UserName]=@P_User,Created_Tms=@CreatedTms WHERE Id=@Id";
        private IConnectionFactory _connectionFatory;
        private string _tableName;



        public DbvWriterRepositoryMan(IConnectionFactory connectionFactory, string tableName)
        {
            _connectionFatory = connectionFactory;
            _tableName = tableName;
        }


        public void Add(DbvEntry item)
        {
            throw new NotImplementedException();
        }

        public void Update(DbvEntry item)
        {

            var c = _connectionFatory.GetConnection();

            var cmd = new SqlCommand(string.Format(_updateSql, _tableName), (SqlConnection)c);

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "Name";
            p1.DbType = DbType.String;
            p1.Value = item.Name;

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "Tags";
            p2.DbType = DbType.String;
            p2.Value = item.Tags;

            var p3 = cmd.CreateParameter();
            p3.ParameterName = "Description";
            p3.DbType = DbType.String;
            p3.Value = item.Description;

            var p4 = cmd.CreateParameter();
            p4.ParameterName = "ImageData";
            p4.DbType = DbType.Binary;
            p4.Size = item.Data != null ? item.Data.Length : 0;
            p4.Value = item.Data;

            var p5 = cmd.CreateParameter();
            p5.ParameterName = "P_User";
            p5.DbType = DbType.String;
            p5.Value = item.UserName;

            var p6 = cmd.CreateParameter();
            p6.ParameterName = "CreatedTms";
            p6.DbType = DbType.DateTime;
            p6.Value = item.CreatedTms;



            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);
            cmd.Parameters.Add(p3);
            cmd.Parameters.Add(p4);
            cmd.Parameters.Add(p5);
            cmd.Parameters.Add(p6);

            var p7 = cmd.CreateParameter();
            p7.ParameterName = "Id";
            p7.DbType = DbType.Int64;
            p7.Value = item.Id;

            cmd.Parameters.Add(p7);

            Console.Out.WriteLine(cmd.CommandText);


            try
            {
                c.Open();
                cmd.ExecuteNonQuery();

            }
            finally
            {
                if (c.State == ConnectionState.Open)
                    c.Close();
            }
            


        }

        public void Delete(DbvEntry item)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}