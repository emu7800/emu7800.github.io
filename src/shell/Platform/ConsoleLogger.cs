namespace EMU7800.Shell;

public sealed class ConsoleLogger : Core.ILogger
{
    public int Level { get; set; }

    public void Log(int level, string message)
    {
        if (level <= Level)
            System.Console.WriteLine(message);
    }
}

public class DebugLogger : Core.ILogger
{
    public int Level { get; set; } = 0;
    public void Log(int level, string message)
    {
        if (level <= Level)
            System.Diagnostics.Debug.WriteLine(message);
    }
}
