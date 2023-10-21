using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.Utils;

public static class EnumerableExtensions
{
    private static readonly Random s_random = new();

    /// <summary>
    ///     Returns a random element from the given enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to select a random element from.</param>
    /// <returns>A random element from the given enumerable.</returns>
    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        var array = enumerable as T[] ?? enumerable.ToArray();
        return array[s_random.Next(array.Length)];
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

    /// <summary>
    ///     Returns a new list containing the elements of the original list in a shuffled order.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the list.</typeparam>
    /// <param name="list">The list to be shuffled.</param>
    /// <returns>A new list containing the shuffled elements of the original list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided list is null.</exception>
    public static IList<T> Shuffled<T>(this IList<T> list)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        return list.OrderBy(_ => s_random.Next()).ToList();
    }
}
