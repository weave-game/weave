namespace weave.Logging.ConcreteCsv;

public readonly struct Log
{
    public Log(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}
