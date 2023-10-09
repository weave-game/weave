namespace Weave.Scoring;

public interface IScoreManager
{
    /// <summary>
    ///     Saves or updates the score if it has the same id.
    /// </summary>
    /// <param name="score">The score.</param>
    void Save(ScoreRecord score);
}