// © Mike Murphy

using EMU7800.Core;
using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class SDLConsoleLogger : ILogger
{
    public int Level { get; set; }

    public void Log(int level, string message)
    {
        if (level <= Level)
            SDL_Log(message);
    }
}
