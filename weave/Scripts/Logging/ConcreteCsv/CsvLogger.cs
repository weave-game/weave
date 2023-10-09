using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GodotSharper;

namespace weave.Logging.ConcreteCsv;

public sealed class CsvLogger : ICsvLogger
{
    private readonly string _filePath;
    private readonly LoggerMode _loggerMode;
    private readonly IEnumerable<Func<Log>> _loggers;

    /// <summary>
    ///     A list of logs to log. Each list of logs represents a single log event.
    ///     Cached in memory until <see cref="Persist" /> is called to not write to disk too often.
    /// </summary>
    private readonly IList<IList<Log>> _logsToLog;

    private bool _firstPersist = true;

    public CsvLogger(string filePath, IEnumerable<Func<Log>> loggers, LoggerMode loggerMode)
    {
        _logsToLog = new List<IList<Log>>();
        _filePath = filePath;
        _loggers = loggers;
        _loggerMode = loggerMode;
    }

    public void Log()
    {
        _logsToLog.Add(_loggers.Select(logger => logger()).ToList());
    }

    public void Persist()
    {
        if (_firstPersist)
        {
            HandleFirstPersist();
            _firstPersist = false;
        }

        var rows = _logsToLog.Select(logs => string.Join(",", logs.Select(log => log.Value)));
        rows.ForEach(row => File.AppendAllText(_filePath, row + Environment.NewLine));
        _logsToLog.Clear();
    }

    private void HandleFirstPersist()
    {
        if (_loggerMode == LoggerMode.Append && HasFileHeaders())
            return;

        ClearFile();
        WriteHeaders();
    }

    private bool HasFileHeaders()
    {
        // Simplified check, but should be enough for our purposes
        return File.Exists(_filePath)
            && !string.IsNullOrWhiteSpace(File.ReadAllLines(_filePath).First());
    }

    private void ClearFile()
    {
        File.WriteAllText(_filePath, string.Empty);
    }

    private void WriteHeaders()
    {
        var headers = _loggers.Select(logger => logger().Name);
        File.AppendAllText(_filePath, string.Join(",", headers) + Environment.NewLine);
    }
}
