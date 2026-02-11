namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        // Optimize for List<T> which has a more efficient AddRange implementation
        if (collection is List<T> list)
        {
            list.AddRange(items);
        }
        else
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }

    public static int IndexOf<T>(this T[] array, T obj) => Array.IndexOf(array, obj);
}
