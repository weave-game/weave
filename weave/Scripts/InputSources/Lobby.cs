using System.Collections.Generic;
using System.Linq;

namespace weave.InputSources;

public sealed class Lobby
{
    private readonly IList<IInputSource> _inputSources = new List<IInputSource>();
    public IReadOnlyList<IInputSource> InputSources => _inputSources.AsReadOnly();
    public int Count => _inputSources.Count;

    public void Join(IInputSource inputSource)
    {
        var alreadyExists = _inputSources.FirstOrDefault(input => input.Equals(inputSource));
        if (alreadyExists != null)
            return;

        _inputSources.Add(inputSource);
    }

    public void Leave(IInputSource inputSource)
    {
        _inputSources.Remove(inputSource);
    }
}
