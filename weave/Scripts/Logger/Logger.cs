using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GodotSharper;

namespace weave.Logger;

public class Logger
{
    private readonly string _filePath;
    private readonly IEnumerable<Func<Log>> _loggers;
    private bool _firstPersist = true;

    /// <summary>
    ///     A list of logs to log. Each list of logs represents a single log event.
    ///     Cached in memory until <see cref="Persist" /> is called to not write to disk too often.
    /// </summary>
    private readonly IList<IList<Log>> _logsToLog;

    public Logger(string filePath, IEnumerable<Func<Log>> loggers)
    {
        _logsToLog = new List<IList<Log>>();
        _filePath = filePath;
        _loggers = loggers;
    }

    public void Log()
    {
        _logsToLog.Add(_loggers.Select(logger => logger()).ToList());
    }

    public void Persist()
    {
        if (_firstPersist)
        {
            ClearFile();
            WriteHeaders();
        }

        var rows = _logsToLog.Select(logs => string.Join(",", logs.Select(log => log.Value)));
        rows.ForEach(row => { File.AppendAllText(_filePath, row + Environment.NewLine); });
        _logsToLog.Clear();
    }
    
    private void ClearFile() => File.WriteAllText(_filePath, string.Empty);

    private void WriteHeaders()
    {
        var headers = _loggers.Select(logger => logger().Name);
        File.AppendAllText(_filePath, string.Join(",", headers) + Environment.NewLine);
    }
}