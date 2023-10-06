using System;
using System.Collections.Generic;
using weave.Logging;
using weave.Scoring;
using weave.Utils;

namespace weave;

public sealed class ScoreManager : IScoreManager
{
    private Logger Logger { get; }
    private ISet<Score> Scores { get; }
    private ScoreUnit CurrentScore { get; set; }

    public ScoreManager()
    {
        Scores = new HashSet<Score>();

        var loggers = new List<Func<Log>>
        {
            () => new Log("id", CurrentScore.Id),
            () => new Log("value", CurrentScore.Value.ToString()),
            () => new Log("name", CurrentScore.Name)
        };

        Logger = new Logger(Constants.ScoreLogFilePath, loggers, LoggerMode.Append);
    }

    public void Save(ScoreUnit score)
    {
        CurrentScore = score;
        Logger.Log();
        Logger.Persist();
    }
}