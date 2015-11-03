using System;
using System.Collections.Generic;

namespace DbvRepository
{
    public class DbvItemCaseInsensitiveComparer : IEqualityComparer<DbvItemBase>
    {
        public bool Equals(DbvItemBase x, DbvItemBase y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        }

        public int GetHashCode(DbvItemBase obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
        }
    }
}