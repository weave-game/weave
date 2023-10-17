using System;

namespace Weave.Scoring;

public class ScoreRecord
{
    public ScoreRecord(int value, string name)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Id { get; }

    //ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int Value { get; }

    public string Name { get; set; }
}
