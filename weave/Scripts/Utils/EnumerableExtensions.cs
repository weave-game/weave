using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Weave.Utils;

public static class EnumerableExtensions
{
    private static readonly RNGCryptoServiceProvider s_rng = new();

    /// <summary>
    ///     Returns a random element from the given enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to select a random element from.</param>
    /// <returns>A random element from the given enumerable.</returns>
    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        var a = enumerable as T[] ?? enumerable.ToArray();

        if (a.Length == 0)
        {
            throw new InvalidOperationException("The collection is empty.");
        }

        var index = GetRandomNumber(0, a.Length);
        return a[index];
    }

    private static int GetRandomNumber(int min, int max)
    {
        var randomNumber = new byte[4];
        s_rng.GetBytes(randomNumber);
        var value = BitConverter.ToInt32(randomNumber, 0);
        return (Math.Abs(value) % (max - min)) + min;
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
