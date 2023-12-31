﻿namespace MoreFodyHelpers.Extensions;

public static class IListExtensions
{
    public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
    {
        for (var i = list.Count - 1; i >= 0; --i)
        {
            if (predicate(list[i]))
                list.RemoveAt(i);
        }
    }
}