using System.Collections.Generic;
using System.Linq;

namespace weave.InputSources;

public sealed class Lobby
{
    private readonly IList<IInputSource> _inputSources = new List<IInputSource>();
    public IList<IInputSource> InputSources => _inputSources.ToList();
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
        var existing = _inputSources.FirstOrDefault(input => input.Equals(inputSource));
        if (existing == null)
            return;

        _inputSources.Remove(inputSource);
    }

    public void Leave(int deviceId)
    {
        var existing = _inputSources.FirstOrDefault(input => input.DeviceId == deviceId);
        if (existing == null)
            return;

        _inputSources.Remove(existing);
    }
}