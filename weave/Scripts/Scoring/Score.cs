using System;

namespace Weave.Scoring;

public class Score
{
    public Score(string id, int points, string name, int players, int rounds)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or whitespace.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(id));

        Id = id;
        Points = points;
        Name = name;
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
    public int Points { get; set; }
}
