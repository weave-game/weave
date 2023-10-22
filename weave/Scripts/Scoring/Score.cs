using System;

namespace Weave.Scoring;

public class Score
{
    public Score(int points, string name, int players, int rounds)
    {
        Id = Guid.NewGuid().ToString();
        Points = points;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Players = players;
        Rounds = rounds;
    }

    /// <summary>
    ///     The unique id of the score.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     The team name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     How many players were in the game.
    /// </summary>
    public int Players { get; }

    /// <summary>
    ///     How many rounds were played.
    /// </summary>
    public int Rounds { get; }

    /// <summary>
    ///     The score.
    /// </summary>
    public int Points { get; }
}
