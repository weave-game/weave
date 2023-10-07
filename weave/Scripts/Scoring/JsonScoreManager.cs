using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace weave.Scoring;

public class JsonScoreManager : IScoreManager
{
    private readonly string _filePath;
    private readonly IDictionary<string, ScoreUnit> _scores;

    public JsonScoreManager(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));

        _filePath = filePath;
        _scores = LoadScoresFromFile();
    }

    private IDictionary<string, ScoreUnit> LoadScoresFromFile()
    {
        if (!File.Exists(_filePath)) return new Dictionary<string, ScoreUnit>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Dictionary<string, ScoreUnit>>(json);
    }

    public void Save(ScoreUnit score)
    {
        _scores[score.Id] = score;
        SaveScoresToFile();
    }

    private void SaveScoresToFile()
    {
        var json = JsonSerializer.Serialize(_scores);
        File.WriteAllText(_filePath, json);
    }
}