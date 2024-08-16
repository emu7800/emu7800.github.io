namespace EMU7800.Core;

public class NullLogger : ILogger
{
    public static readonly ILogger Default = new NullLogger();

    public void WriteLine(string message) {}

    public void Write(string message) {}

    private NullLogger() {}
}

public class DebugLogger : ILogger
{
    public static readonly ILogger Default = new DebugLogger();

    public void WriteLine(string message)
        => System.Diagnostics.Debug.WriteLine(message);

    public void Write(string message)
        => System.Diagnostics.Debug.Write(message);

    private DebugLogger() {}
}