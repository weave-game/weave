using System;
using System.Collections.Generic;

namespace weave.Utils;

/// <summary>
///     Provides extension methods for working with enumerables.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Executes the specified action a specified number of times.
    ///     TODO: Move to GodotSharper.
    /// </summary>
    /// <param name="count">The number of times to execute the action.</param>
    /// <param name="action">The action to execute.</param>
    public static void TimesDo(this int count, Action<int> action)
    {
        if (count < 0)
            throw new ArgumentException("Count must be greater than or equal to 0", nameof(count));

        for (var i = 0; i < count; i++)
            action(i);
    }

    /// <summary>
    ///     Executes a specified action on each element in the collection.
    ///     TODO: Move to GodotSharper
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="enumeration">The collection to iterate over.</param>
    /// <param name="action">The action to execute on each element.</param>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (var item in enumeration)
            action(item);
    }
}
