using weave.InputSources;

namespace weave;

/// <summary>
///     Data that NEEDS to be transferred between scenes. Only use this for data that is absolutely necessary.
/// </summary>
public static class GameConfig
{
    public static Lobby Lobby { get; set; }
}
