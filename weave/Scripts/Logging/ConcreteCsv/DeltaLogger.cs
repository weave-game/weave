using System.Globalization;
using Godot;

namespace Weave.Logging.ConcreteCsv;

public sealed class DeltaLogger
{
    private float _firstLog = -1f;

    public Log Log()
    {
        var ms = Time.GetTicksMsec();

        if (_firstLog < 0f)
            _firstLog = ms;

        return new Log("delta_ms", (ms - _firstLog).ToString(CultureInfo.InvariantCulture));
    }
}
