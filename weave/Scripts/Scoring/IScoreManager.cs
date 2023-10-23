namespace Weave.Scoring;

public interface IScoreManager
{
    /// <summary>
    ///     Gets the score with the given id.
    /// </summary>
    int GetPoints(string id);

    /// <summary>
    ///     Saves or updates the score if it has the same id.
    /// </summary>
    /// <param name="score">The score.</param>
    void Save(Score score);
}
