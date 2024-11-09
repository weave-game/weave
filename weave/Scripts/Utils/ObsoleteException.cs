using System;

namespace Weave;

/// <summary>
///     Exception thrown when a obselete class is used.
/// </summary>
public sealed class ObsoleteException : Exception
{
    public ObsoleteException(string message)
        : base(message) { }
}
