using System;
using System.Collections.Generic;
using System.Text;

namespace MoreFodyHelpers.Extensions;

public static class KeyValuePairExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        where TKey : notnull
    {
        key = pair.Key;
        value = pair.Value;
    }
}