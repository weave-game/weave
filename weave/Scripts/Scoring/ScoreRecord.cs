using System;

namespace weave.Scoring;

public class ScoreRecord
{
    public ScoreRecord(int value, string name)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Id { get; private set; }
    public int Value { get; private set; }
    public string Name { get; set; }
}
