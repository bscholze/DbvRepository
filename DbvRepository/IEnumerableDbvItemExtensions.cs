using System;
using System.Collections.Generic;

namespace DbvRepository
{
    public static class IEnumerableDbvItemExtensions
    {

        internal static IEnumerable<DbvItemBase> LatestRevisionOnly(this IEnumerable<DbvItemBase> source)
        {
            var itemSet = new Dictionary<string,DbvItemBase>(StringComparer.OrdinalIgnoreCase);

            foreach (var dbvItem in source)
            {
                if (itemSet.ContainsKey(dbvItem.Name))
                {
                    if (itemSet[dbvItem.Name].Revision < dbvItem.Revision)
                        itemSet[dbvItem.Name] = dbvItem;
                }
                else
                    itemSet[dbvItem.Name] = dbvItem;
            }

            return itemSet.Values;
        }
    }
}