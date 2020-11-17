namespace EMU7800.Core
{
    public class NullLogger : ILogger
    {
        public static readonly ILogger Default = new NullLogger();

        public void WriteLine(string message)
        {
        }

        public void Write(string message)
        {
        }
    }
}
