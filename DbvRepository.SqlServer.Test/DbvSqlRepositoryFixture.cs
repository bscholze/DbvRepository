using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using NUnit.Framework;

namespace DbvRepository.SqlServer.Test
{

    class ConnectionFactory : IConnectionFactory
    {
        private readonly IDbConnection _cnx;
        private readonly string _connectionString;

        public ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
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
            var cnx = new SqlConnection(_connectionString);
            return cnx;
        }
    }


    class ConnectionTransactionFactory : IConnectionFactory
    {
        private readonly IDbConnection _cnx;
        private readonly IDbTransaction _tsn;
        private readonly string _connectionString;

        public ConnectionTransactionFactory(string connectionString)
        {
            _connectionString = connectionString;
            _cnx = GetSqlConnection();
            _cnx.Open();
            _tsn = _cnx.BeginTransaction();
        }

        public IDbConnection GetConnection()
        {
            return _cnx;
        }

        public IDbTransaction GetTransaction()
        {
            return _tsn;
        }

        public SqlConnection GetSqlConnection()
        {
            var cnx = new SqlConnection(_connectionString);
            return cnx;
        }
    }




    [TestFixture]
    public class DbvSqlRepositoryFixture
    {

        private IConnectionFactory _connectionFactory;
        private IDbConnection _connection;
        private DbvItemRepository _repo;

        [TestFixtureSetUp]
        public void VerifyTestDatabaseExists()
        {
            _connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["DBV_TEST"].ConnectionString);
            _connection = _connectionFactory.GetConnection();
            _connection.Open();
            Assert.AreEqual(ConnectionState.Open, _connection.State);
            // verify table exists
            _connection.Close();

            _repo = new DbvItemRepository(new DbvItemSqlServerStore(_connectionFactory, "DBV_REVISION_STORE"));

        }

        [SetUp]
        public void SetupOpenDbConnection()
        {
            _connection.Open();

            // Empty table 
            _connection.Execute("TRUNCATE TABLE DBV_REVISION_STORE");


        }

        [TearDown]
        public void TearDownCloseDbConnection()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }



        [Test]
        public void A_VerifyThatTableExistsAndIsEmpty()
        {
            var results = _connection.Query<DbvItem>("select TOP 10 * from DBV_REVISION_STORE");
            Assert.IsEmpty(results);
        }

        [Test]
        public void CheckinOneElementInDatabase()
        {

            var rev = _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Checkin comment", "john doe");
            Assert.AreEqual(1, rev);

            var results = _connection.Query<DbvItem>("select * from DBV_REVISION_STORE").ToList();
            Assert.IsNotEmpty(results);
            var r = results.First();
            Assert.AreEqual("/test/dummy.csv", r.Name);

        }

        [Test]
        public void VerifyCheckoutRetreivesLatestOnly()
        {


            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "First", "john doe");
            var rev = _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Second", "jane doe");

            Assert.AreEqual(2, rev);

            var item = _repo.Checkout("/test/dummy.csv");
            Assert.IsNotNull(item);
            Assert.AreEqual(2, item.Revision);
            Assert.AreEqual("jane doe", item.Author);

        }

        [Test]
        public void VerifyCheckoutRetreivesLatestVersionIgnoreCase()
        {


            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "First", "john doe");
            var rev = _repo.Checkin("/Test/DUMMY.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Second", "jane doe");

            Assert.AreEqual(2, rev);

            var item = _repo.Checkout("/test/dummy.csv");
            Assert.IsNotNull(item);
            Assert.AreEqual(2, item.Revision);
            Assert.AreEqual("jane doe", item.Author);
            Console.Out.WriteLine(item);

        }

        [Test]
        public void VerifyCheckoutSpecificVersion()
        {


            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Y", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Z", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "1", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "2", "john doe");
            var rev = _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "Second", "jane doe");

            Assert.AreEqual(6, rev);

            var item = _repo.Checkout("/test/dummy.csv", 3);
            Assert.IsNotNull(item);
            Assert.AreEqual(3, item.Revision);
            Assert.AreEqual("Z", item.Comments);
            Console.Out.WriteLine(item);

        }

        [Test]
        public void VerifyListSpecificPath()
        {
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/second/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/test/notshown.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");

            var expected = new string[] {"/second/dummy.csv", "/test/dummy.csv"};


            var results = _repo.List("/**/dummy.csv");

            Assert.AreEqual(expected.OrderBy(s => s).ToList(), results.Select(s => s.Name).OrderBy(s => s).ToList());

        }


        [Test]
        public void VerifyListSpecificPathWithRevision()
        {
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/test/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/second/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            _repo.Checkin("/test/notshown.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");
            var rev = _repo.Checkin("/third/dummy.csv", Encoding.ASCII.GetBytes("ABCDEFG"), "X", "john doe");

            var expected = new string[] { "/second/dummy.csv", "/test/dummy.csv" };
            var results = _repo.List("/**/dummy.csv", rev-1);

            Assert.AreEqual(expected.OrderBy(s => s).ToList(), results.Select(s => s.Name).OrderBy(s => s).ToList());

        }

        [Test]
        public void Z_CheckinAFewLargeFiles()
        {

            var largeContent = File.ReadAllBytes("LargeTextForUnitTest.txt");

            Console.Out.WriteLine("largeContent.Length = {0}", largeContent.Length);

            for (int i = 0; i < 100; i++)
            {
                _repo.Checkin("/large/textfile.txt", largeContent, string.Format("Checked in {0}", i), "john doe");
            }

            var readItBack = _repo.Checkout("/large/TEXTFILE.TXT");

            Assert.AreEqual(largeContent, readItBack.Content);
            Assert.AreEqual(100, readItBack.Revision);

            // delete it and verify it is not availabe anymore

            _repo.Remove("/large/TextFile.txt", "Deleted", "jane doe");

            var item = _repo.Checkout("/large/textFile.txt");
            Assert.IsNull(item);

            _repo.Checkin("/test/Dummy.CSV", largeContent, "X", "john doe");
            _repo.Checkin("/test/dUMMY.csv", largeContent, "X", "john doe");
            _repo.Checkin("/SECOND/dummy.csv", largeContent, "X", "john doe");
            _repo.Checkin("/test/NotShown.csv", largeContent, "X", "john doe");


        }


        [Test]
        public void VerifyHistoryOfCheckin()
        {
            _repo.Checkin("/historyCheck/test.pdf", Encoding.ASCII.GetBytes("First"), "first checkin comment",
                "John Doe");
            _repo.Checkin("/historyCheck/test.pdf", Encoding.ASCII.GetBytes("Second"), "second checkin comment",
                "Jane Doe");

            _repo.Remove("/historyCheck/test.pdf", "Deleted", "jack doe");

            var history = _repo.History("/historyCheck/**").ToList();

            history.ForEach(Console.WriteLine);

            Assert.AreEqual(3, history.Count());

            // ensure it is really gone

            var noresult = _repo.Checkout("/historyCheck/test.pdf");
            Assert.IsNull(noresult);


        }



    }
}