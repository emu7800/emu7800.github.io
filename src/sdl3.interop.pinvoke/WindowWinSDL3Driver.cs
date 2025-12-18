// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace EMU7800.SDL3.Interop;

public sealed partial class WindowWinSDL3Driver : IWindowDriver
{
    readonly WindowSDL3Driver _sdl3Driver;
    readonly ILogger _logger;
    readonly bool _openConsole;

    #region IWindowDriver Members

    public unsafe void Start(Window window, bool startMaximized)
    {
        if (_openConsole)
        {
            AllocConsole();
            _logger.Log(5, "Opened console window.");
        }

        _sdl3Driver.Start(window, startMaximized);
    }

    #endregion

    #region Constructors

    public WindowWinSDL3Driver(string[] args, ILogger logger)
      => (_sdl3Driver, _logger, _openConsole) = (new(logger), logger, args.Any(arg => CiOneOf(arg, "-c", "/c")));

    #endregion

    #region Helpers

    static bool CiOneOf(string arg, params string[] items)
      => items.Any(item => item.Equals(arg, StringComparison.OrdinalIgnoreCase));

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    #endregion
}