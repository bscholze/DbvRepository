using RepositoryWrapper.Dapper;

namespace DbvCmd
{
    public class DbvWriterRepository : WriterRepositoryDapperBase<DbvEntry>
    {
        public DbvWriterRepository(IConnectionFactory connectionFactory, string tableName) : base(connectionFactory, tableName)
        {
        }
    }
}