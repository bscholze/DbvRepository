using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using RepositoryWrapper.Dapper;

namespace DbvCmd
{
    class ConnectionFactory : IConnectionFactory
    {
        private readonly IDbConnection _cnx;

        public ConnectionFactory()
        {
            _cnx = GetSqlConnection();
        }

        public IDbConnection GetConnection()
        {
            return _cnx;
        }

        public IDbTransaction GetTransaction()
        {
            return null;
        }

        public SqlConnection GetSqlConnection()
        {
            string cnxStr = ConfigurationManager.ConnectionStrings["DBV"].ConnectionString;
            var cnx = new SqlConnection(cnxStr);
            return cnx;
        }
    }
}