using System;
using Godot;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Weave.Scoring;

public sealed class MongoDbScoreManager : IScoreManager
{
    private const string DefaultConnectionString = "mongodb://localhost:27017";
    private readonly IMongoCollection<Score> _scores;

    public MongoDbScoreManager()
    {
        var connectionString = GetConnectionString();

        // Fallback to local mongo instance
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = DefaultConnectionString;

        try
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("weave");
            _scores = database.GetCollection<Score>("scores");
        }
        catch (Exception)
        {
            GD.Print("Unable to connect to Mongo database");
        }
    }

    public void Save(Score score)
    {
        if (_scores == null)
        {
            GD.Print("Unable to save score, no Mongo instance");
            return;
        }

        var filter = Builders<Score>.Filter.Eq(s => s.Id, score.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        _scores.ReplaceOne(filter, score, options);
    }

    private static string GetConnectionString()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Main>().Build();
        var connectionString = config["ConnectionString"];
        return connectionString;
    }
}
