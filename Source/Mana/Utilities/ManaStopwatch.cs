using System.Diagnostics;
using Mana.Utilities;

public class ManaStopwatch
{
    private static readonly Logger _log = new Logger("Stopwatch");
    private readonly Stopwatch _stopwatch;
    private string _name;

    public ManaStopwatch(string name)
    {
        _name = name;
        _stopwatch = new Stopwatch();
    }

    public static ManaStopwatch StartNew(string name = "")
    {
        var sw = new ManaStopwatch(name);
        sw.Start();
        return sw;
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public void Tally(string message)
    {
        _stopwatch.Stop();
        _log.Info($"{message} at {_stopwatch.Elapsed.TotalMilliseconds} ms");
        _stopwatch.Restart();
    }

    public void Restart()
    {
        _stopwatch.Restart();
    }
}
