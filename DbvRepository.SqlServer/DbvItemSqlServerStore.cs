using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DbvRepository.SqlServer
{

    public interface IConnectionFactory
    {
        IDbConnection GetConnection();

        IDbTransaction GetTransaction();
    }



    public class DbvItemSqlServerStore : IDbvItemStore
    {
        private string _tableName;
        private IConnectionFactory _connectionFactory;

        private string _insertQueryFmt = @"
INSERT INTO [{0}](NAME,NAME_LC,COMMENTS,AUTHOR,TIMESTAMP,DELETED,CONTENT) VALUES (@Name,LOWER(@Name),@Comments,@Author,@Timestamp,@Deleted,@Content);
SELECT CONVERT(int, SCOPE_IDENTITY());";

        private string _insertQuery;



        public DbvItemSqlServerStore(IConnectionFactory connectionFactory, string tableName )
        {
            _tableName = tableName;
            _connectionFactory = connectionFactory;
            _insertQuery = string.Format(_insertQueryFmt, tableName);

        }


        public DbvItem Checkout(string itemPath, int revision)
        {
            //var connection = _connectionFactory.GetConnection();

            try
            {
                // Question is, should the connection factory return a open connection? who opens and closes it?
                //connection.Open();
                var sql = string.Format("SELECT TOP 1 * FROM {0} WHERE NAME_LC=@itemPath {1} ORDER BY REVISION DESC",
                    _tableName, revision == 0 ? "" : "AND REVISION <= @revision");

                
                var items = SqlMapper.Query<DbvItem>(_connectionFactory.GetConnection(), sql, new { itemPath, revision }, _connectionFactory.GetTransaction());
                return items.FirstOrDefault();
            }
            finally
            {
                /*
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
                */
            }
            
        }

        public int Checkin(DbvItem item)
        {
            //var connection = _connectionFactory.GetConnection();

            try
            {
                //connection.Open();
                var revision = SqlMapper.ExecuteScalar(_connectionFactory.GetConnection(),_insertQuery, item, _connectionFactory.GetTransaction());
                return (int) revision;
            }
            finally
            {
                /*
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
                */
            }

        }

        public IEnumerable<DbvItemBase> List(int revision)
        {
           
            var sql = string.Format("SELECT REVISION,NAME,COMMENTS,AUTHOR,TIMESTAMP,DELETED FROM {0} {1} ORDER BY REVISION DESC",
                    _tableName, revision == 0 ? "" : "WHERE REVISION <= @revision");

            return SqlMapper.Query<DbvItemBase>(_connectionFactory.GetConnection(),
                sql, new {revision}, _connectionFactory.GetTransaction());

        }

        public IEnumerable<DbvItemBase> ListLatestOnly(int revision)
        {

            var sql = string.Format(@"
;WITH MAXREVBYNAME AS (
   SELECT NAME_LC,MAX(REVISION) AS LATEST_REVISION FROM {0} 
   {1}
   GROUP BY NAME_LC
)
SELECT 
   REVISION,NAME,COMMENTS,AUTHOR,TIMESTAMP,DELETED 
FROM 
   {0} 
   JOIN MAXREVBYNAME ON ({0}.REVISION = MAXREVBYNAME.LATEST_REVISION) 
ORDER BY REVISION DESC
", _tableName, revision == 0 ? "" : "WHERE REVISION <= @revision");

            return SqlMapper.Query<DbvItemBase>(_connectionFactory.GetConnection(),
                sql, new { revision }, _connectionFactory.GetTransaction());

        }
    }
}