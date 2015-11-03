using System.Data;
using System.Data.SqlClient;
using RepositoryWrapper.Dapper;

namespace DbvCmd
{
    public class SimpleConnectionFactory : IConnectionFactory
    {
        private SqlConnection _openConnection;

        public SimpleConnectionFactory(SqlConnection openConnection)
        {
            _openConnection = openConnection;
        }

        public IDbConnection GetConnection()
        {
            return _openConnection;
        }

        public IDbTransaction GetTransaction()
        {
            return _openConnection.BeginTransaction();
        }
    }
}