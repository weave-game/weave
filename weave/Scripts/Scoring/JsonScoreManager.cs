using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Weave.Scoring;

public class JsonScoreManager : IScoreManager
{
    private readonly string _filePath;
    private readonly IDictionary<string, Score> _scores;

    public JsonScoreManager(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
        }

        _filePath = filePath;
        _scores = LoadScoresFromFile();
    }

    public void Save(Score score)
    {
        _scores[score.Id] = score;
        SaveScoresToFile();
    }

    private IDictionary<string, Score> LoadScoresFromFile()
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<string, Score>();
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Dictionary<string, Score>>(json);
    }

    private void SaveScoresToFile()
    {
        var json = JsonSerializer.Serialize(_scores);
        File.WriteAllText(_filePath, json);
    }
}
