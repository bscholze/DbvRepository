using System;
using System.Collections.Generic;
using System.Linq;
using DbvRepository.Minimatch;


namespace DbvRepository
{

    public class DbvItemRepository
    {
        private IDbvItemStore _revisionStore;

        public DbvItemRepository(IDbvItemStore itemStore)
        {
            _revisionStore = itemStore;
        }

        public DbvItem Checkout(string itemPath)
        {
            return FilterDeleted(_revisionStore.Checkout(itemPath, 0));
        }

        public DbvItem Checkout(string itemPath, int revision)
        {
            return FilterDeleted(_revisionStore.Checkout(itemPath, revision));
        }

        private DbvItem FilterDeleted(DbvItem item)
        {
            if (item != null && !IsDeleted(item))
                return item;
            return null;
        }

        private bool IsDeleted(DbvItemBase item)
        {
            return item.Deleted == 'Y';
        }

        private int Checkin(DbvItem item)
        {
            return _revisionStore.Checkin(item);
        }

        public int Checkin(string itemPath, byte[] content, string comment, string author)
        {
            var item = new DbvItem()
            {
                Name = itemPath,
                Comments = comment,
                Author = author,
                Deleted = 'N',
                Content = content,
                Timestamp = DateTime.Now,
            };
            return Checkin(item);
        }

        public IEnumerable<DbvItemBase> List(string itemMask)
        {
            return List(itemMask, 0);
        }


        public IEnumerable<DbvItemBase> List(string itemMask, int revision)
        {
            return FilterDeleted(_revisionStore.ListLatestOnly(revision), itemMask);
        }


        private IEnumerable<DbvItemBase> FilterDeleted(IEnumerable<DbvItemBase> source, string itemMask)
        {
            var matcher = new Minimatcher(string.IsNullOrEmpty(itemMask) ? "**" : itemMask, new Options { IgnoreCase = true, MatchBase = true });

            // code duplication with the above
            return source.Where(item => matcher.IsMatch(item.Name))
                         .Where(item => !IsDeleted(item));
            
        }


        public int Remove(string itemPath, string comment, string author)
        {
            var item = Checkout(itemPath);

            if (item != null)
            {
                var deletedRecord = new DbvItem()
                {
                    Name = item.Name,
                    Comments = comment,
                    Author = author,
                    Content = null,
                    Deleted = 'Y',
                    Timestamp = DateTime.Now,
                };
                return Checkin(deletedRecord);
            }
            return -1;
        }

        public IEnumerable<DbvItemBase> History(string itemMask)
        {
            var matcher = new Minimatcher(string.IsNullOrEmpty(itemMask) ? "**" : itemMask, new Options { IgnoreCase = true, MatchBase = true });

            return _revisionStore.List(0)
                                 .Where(item => matcher.IsMatch(item.Name))
                                 .OrderBy(k => k.Revision);

        }
    }
}