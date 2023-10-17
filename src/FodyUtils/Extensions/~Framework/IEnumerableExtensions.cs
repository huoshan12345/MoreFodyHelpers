namespace FodyUtils.Extensions;

public static class IEnumerableExtensions
{
    public static int IndexOfFirst<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        var index = 0;

        foreach (var item in items)
        {
            if (predicate(item))
                return index;

            ++index;
        }

        return -1;
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        => new(items); 
        
    public static string JoinWith<T>(this IEnumerable<T> enumerable, string? separator)
    {
        return string.Join(separator, enumerable);
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }
}