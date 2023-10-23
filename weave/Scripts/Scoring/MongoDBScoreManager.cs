using System;
using MongoDB.Driver;
using Godot;

namespace Weave.Scoring;

public class MongoDBScoreManager : IScoreManager
{
    private const string connectionString = "mongodb+srv://erik:shyanne@weave-db.zurbpnp.mongodb.net/";
    private readonly IMongoCollection<Score> _scores;

    public MongoDBScoreManager()
    {
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
        if (_scores != null)
        {
            var filter = Builders<Score>.Filter.Eq(s => s.Id, score.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            _scores.ReplaceOne(filter, score, options);
        }
        else
        {
            GD.Print("Unable to save score, no Mongo instance");
        }
    }
}