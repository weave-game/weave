using System;

namespace weave.Scoring;

// TODO: Rename to Score once the other PR is merged
public struct ScoreUnit
{
    public ScoreUnit(int value, string name)
    {
        Id = Guid.NewGuid().ToString();
        Value = value;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Id { get; private set; }
    public int Value { get; private set; }
    public string Name { get; set; }
}
