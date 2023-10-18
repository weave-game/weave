using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.Utils;

public static class EnumerableExtensions
{
    /// <summary>
    ///     Returns a random element from the given enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to select a random element from.</param>
    /// <returns>A random element from the given enumerable.</returns>
    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        var rnd = new Random();
        var a = enumerable as T[] ?? enumerable.ToArray();
        var index = rnd.Next(0, a.Length);
        return a[index];
    }

    /// <summary>
    ///     Removes the first element from the given List that matches the given condition.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="condition"></param>
    /// <typeparam name="T">The type of the elements in the list</typeparam>
    public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> condition) where T : class
    {
        var item = list.FirstOrDefault(condition);
        if (item != null)
        {
            list.Remove(item);
        }
    }
}
