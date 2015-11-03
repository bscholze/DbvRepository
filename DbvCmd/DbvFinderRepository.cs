using RepositoryWrapper.Dapper;

namespace DbvCmd
{
    public class DbvFinderRepository : FinderRepositoryDapperBase<DbvEntry>
    {
        public DbvFinderRepository(IConnectionFactory connectionFactory, string tableName) : base(connectionFactory, tableName)
        {
        }
    }
}