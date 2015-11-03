using System.Collections.Generic;

namespace DbvRepository
{
    public interface IDbvItemStore
    {
        DbvItem Checkout(string itemPath, int revision);
        int Checkin(DbvItem item);
        IEnumerable<DbvItemBase> List(int revision);
        IEnumerable<DbvItemBase> ListLatestOnly(int revision);
    }
}