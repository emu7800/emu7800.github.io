namespace EMU7800.Core;

public class NullLogger : ILogger
{
    public static readonly ILogger Default = new NullLogger();
    public int Level { get; set; } = 0;
    public void Log(int level, string message) {}
    NullLogger() {}
}
