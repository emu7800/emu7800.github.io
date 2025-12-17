namespace EMU7800.Core;

public interface ILogger
{
    int Level {  get; set; }
    void Log(int level, string message);
}