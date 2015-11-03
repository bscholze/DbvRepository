using System;
using System.Linq;
using System.Text;
using DbvRepository.Minimatch;
//using Minimatch;
using NUnit.Framework;

namespace DbvRepository.Test
{
    [TestFixture]
    public class DbvItemRepositoryFixture
    {
        public DbvItemRepository Repository;

        public DbvItemRepositoryFixture()
        {
            
        }

        [SetUp]
        public void Setup()
        {
            Repository = new DbvItemRepository(new DbvItemMemoryStore());
            FillTestRepository(Repository);
        }
 
        public void FillTestRepository(DbvItemRepository repository)
        {
            var content = Encoding.ASCII.GetBytes("Hello World");

            repository.Checkin("/test/dummy.csv", content, "CheckinComments", "John Doe");
            repository.Checkin("/second/dummy.csv", content, "Checkin Comments", "John Doe"); 
            repository.Checkin("/test/dummy.csv", content, "Checkin Comments", "John Doe"); 
            repository.Checkin("/test/dummy.csv", content, "Checkin Comments", "John Doe");
            repository.Checkin("/second/dummy.csv", content, "Checkin Comments", "John Doe"); 
            repository.Checkin("/test/dummy.csv", content, "Checkin Comments", "John Doe"); 

        }


        [Test]
        public void CheckinNewDbvItem()
        {

            var repository = new DbvItemRepository(new DbvItemMemoryStore());

            var latestRev = repository.Checkin("/test/dummy.csv", null, "comment", "jane doe");
            Assert.Greater(latestRev, 0);
            // checkin item again and verify that revision number increased

            var latestRev1 = repository.Checkin("/test/dummy.csv", null, "comment", "jane doe");
            Assert.AreEqual(latestRev + 1, latestRev1);

        }

        [Test]
        public void RetreiveLatestDbvItemByPath()
        {

            var item = Repository.Checkout("/test/dummy.csv");
            Assert.IsNotNull(item);
            Assert.AreEqual("/test/dummy.csv", item.Name);


        }

        [Test]
        public void RetreiveUnknownItemPath()
        {
            Assert.IsNull(Repository.Checkout("/for/sure/unknown.xx"));
        }


        [Test]
        public void RetreiveSpecificRevisionByPath()
        {

            // get the latest version of an item and try to get the previous one
            var latest = Repository.Checkout("/test/dummy.csv");
            Assert.IsNotNull(latest);


            var prev = Repository.Checkout("/test/dummy.csv", latest.Revision - 1);

            Assert.Less(prev.Revision, latest.Revision);

        }

        [Test]
        public void CheckinByNameAndParameters()
        {

            var currentRevision = Repository.Checkout("/test/dummy.csv");

            var content = Encoding.ASCII.GetBytes("abcdefg");
            var revision = Repository.Checkin("/test/dummy.csv", content, "checkin comment", "john doe");

            Assert.Greater(revision, currentRevision.Revision);

            // and get it back 
            var latest = Repository.Checkout("/test/dummy.csv");
            Assert.AreEqual("john doe", latest.Author);
            Assert.AreEqual(content, latest.Content);
            Assert.AreEqual("checkin comment", latest.Comments);

        }

        [Test]
        public void RetreiveListOfAllItemsInRepository()
        {
            var expected = new string[] {"/test/dummy.csv", "/second/dummy.csv"};

            var items = Repository.List("");

            Assert.AreEqual(expected.OrderBy(s => s).ToArray(), items.Select(s => s.Name).OrderBy(s => s).ToArray());

        }

        [Test]
        public void RetreiveListOfItemsInRepositoryWithMask()
        {

            var expected = new string[] { "/second/dummy.csv" };

            var items = Repository.List("/second/**");

            Assert.AreEqual(expected.OrderBy(s => s).ToArray(), items.Select(s => s.Name).OrderBy(s => s).ToArray());
        }

        [Test]
        public void RetreiveListOfItemsInRepositoryWithMaskCaseInsensitive()
        {

            Repository.Checkin("/TEST/DUMMY.CSV", Encoding.ASCII.GetBytes("Does't matter"), "Case insensitive checking",
                "john doe");

            Repository.Checkin("/TEST/somefile.pdf", Encoding.ASCII.GetBytes("Does't matter"), "Case insensitive checking",
                "john doe");


            var expected = new string[] { "/TEST/DUMMY.CSV", "/TEST/somefile.pdf" };

            var items = Repository.List("/test/**");

            Assert.AreEqual(expected.OrderBy(s => s).ToArray(), items.Select(s => s.Name).OrderBy(s => s).ToArray());
        }

        [Test]
        public void RetreiveListOfItemsInRepositoryWithMaskCaseGlobPattern()
        {

            Repository.Checkin("/TEST/DUMMY.CSV", Encoding.ASCII.GetBytes("Does't matter"), "Case insensitive checking",
                "john doe");

            Repository.Checkin("/thrird/afew/levels/down/dummy.csv", Encoding.ASCII.GetBytes("Does't matter"), "Case insensitive checking",
                "john doe");


            var expected = new string[] { "/TEST/DUMMY.CSV", "/second/dummy.csv", "/thrird/afew/levels/down/dummy.csv" };

            var items = Repository.List("**/dummy.csv");

            Assert.AreEqual(expected.OrderBy(s => s).ToArray(), items.Select(s => s.Name).OrderBy(s => s).ToArray());
        }


        [Test]
        public void RetreiveListOfItemsInRepositorySpecificVersion()
        {

            var firstVersion = Repository.Checkin("/list/xxx.csv", Encoding.ASCII.GetBytes("first"),
                "can we get that back?", "john doe");

            var item = Repository.Checkout("/list/xxx.csv");

            var secondVersion = Repository.Checkin("/list/yyy.csv", Encoding.ASCII.GetBytes("first"), "can we get that back?", "john doe");

            // get the items
            var expectedLatest = new string[] {"/list/xxx.csv", "/list/yyy.csv"};
            var items = Repository.List("/list/**");
            Assert.AreEqual(expectedLatest.OrderBy(s => s).ToArray(), items.Select(s => s.Name).OrderBy(s => s).ToArray());

            var expectedRev = new string[] { "/list/xxx.csv"};
            var itemsRev = Repository.List("/list/**", firstVersion);
            Assert.AreEqual(expectedRev.OrderBy(s => s).ToArray(), itemsRev.Select(s => s.Name).OrderBy(s => s).ToArray());


        }

        [Test]
        public void RemoveItemFromRepository()
        {

            var itemBefore = Repository.Checkout("/test/dummy.csv");

            Repository.Remove("/test/dummy.csv", "removed item", "jane doe");

            Assert.IsNull(Repository.Checkout("/test/dummy.csv"));
            var list = Repository.List("");
            Assert.IsNull(list.FirstOrDefault(s => s.Name == "/test/dummy.csv"));

            var itemStillThere = Repository.Checkout("/test/dummy.csv", itemBefore.Revision);
            Assert.IsNotNull(itemStillThere);
            Assert.AreSame(itemBefore, itemStillThere);

        }


        [Test]
        public void RetreiveHistoryOfItem()
        {
            Repository.Checkin("/historyCheck/test.pdf", Encoding.ASCII.GetBytes("First"), "first checkin comment",
                "John Doe");
            Repository.Checkin("/historyCheck/test.pdf", Encoding.ASCII.GetBytes("Second"), "second checkin comment",
                "Jane Doe");

            Repository.Remove("/historyCheck/test.pdf", "Deleted", "jack doe");

            var history = Repository.History("/historyCheck/**").ToList();

            history.ForEach(Console.WriteLine);

            Assert.AreEqual(3, history.Count());

            // output the history for a path
            //Repository.History("/test/**").ToList().ForEach(Console.WriteLine);


        }



        [Test]
        public void VerifyMinimatcherSearchPatternMatch()
        {
            var matcher = new Minimatcher("/test/*", new Minimatch.Options {IgnoreCase = true});


            Assert.IsTrue(matcher.IsMatch("/test/dummy.csv"));
            Assert.IsTrue(matcher.IsMatch("/TEST/dummy.csv"));
            Assert.IsFalse(matcher.IsMatch("/second/dummy.csv"));


        }



    }
}