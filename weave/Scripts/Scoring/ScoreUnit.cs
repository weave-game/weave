using System;

namespace weave.Scoring;

// TODO: Rename to Score once the other PR is merged
public struct ScoreUnit
{
    public ScoreUnit(string id, int value, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Value = value;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Id { get; private set; }
    public int Value { get; private set; }
    public string Name { get; private set; }
}
