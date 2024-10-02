using System;
using System.Collections.Generic;
using System.Text;

namespace MoreFodyHelpers.Extensions;

public static class StackExtensions
{
    /// <summary>
    /// see details in https://stackoverflow.com/questions/7391348/c-sharp-clone-a-stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    public static Stack<T> Clone<T>(this Stack<T> original)
    {
        var arr = new T[original.Count];
        original.CopyTo(arr, 0);
        Array.Reverse(arr);
        return new Stack<T>(arr);
    }
}