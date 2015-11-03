using System.Collections.Generic;
using System.Linq;

namespace DbvRepository
{
    public class DbvItemMemoryStore : IDbvItemStore
    {
        private List<DbvItem> _itemStore;
        private IEqualityComparer<DbvItemBase> _cmp;

        public DbvItemMemoryStore()
        {
            _itemStore = new List<DbvItem>();
            _cmp = new DbvItemCaseInsensitiveComparer();
        }

        public DbvItem Checkout(string itemPath, int revision)
        {
            var searchItem = new DbvItem() {Name = itemPath};
            return
                _itemStore.Where(item => revision == 0 || item.Revision <= revision)
                    .LastOrDefault(item => _cmp.Equals(searchItem, item));
        }

        public int Checkin(DbvItem item)
        {
            _itemStore.Add(item);
            item.Revision = _itemStore.Count;
            return _itemStore.Count;
        }

        public IEnumerable<DbvItemBase> List(int revision)
        {
            return _itemStore.Where(item => revision == 0 || item.Revision <= revision);
        }

        public IEnumerable<DbvItemBase> ListLatestOnly(int revision)
        {
            return _itemStore.Where(item => revision == 0 || item.Revision <= revision).LatestRevisionOnly();
        }
    }
}