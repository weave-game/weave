using System;
using System.Collections.Generic;
using weave.Logging;
using weave.Logging.ConcreteCsv;
using weave.Utils;

namespace weave.Scoring;

public sealed class CsvScoreManager : IScoreManager
{
    public CsvScoreManager()
    {
        var loggers = new List<Func<Log>>
        {
            () => new Log("id", CurrentScore.Id),
            () => new Log("value", CurrentScore.Value.ToString()),
            () => new Log("name", CurrentScore.Name)
        };

        CsvLogger = new CsvLogger(Constants.ScoreLogFilePath, loggers, LoggerMode.Append);
    }

    private CsvLogger CsvLogger { get; }
    private ScoreUnit CurrentScore { get; set; }

    public void Save(ScoreUnit score)
    {
        CurrentScore = score;
        CsvLogger.Log();
        CsvLogger.Persist();
    }
}
