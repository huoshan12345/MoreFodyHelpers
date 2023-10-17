namespace FodyUtils.Extensions;

public static class ICollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }

    public static void AddRange<T>(this ICollection<T> collection, params T[] items)
        => AddRange(collection, items.AsEnumerable());

}