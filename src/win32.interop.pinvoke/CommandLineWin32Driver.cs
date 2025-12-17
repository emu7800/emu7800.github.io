using EMU7800.Core;
using EMU7800.Services.Dto;
using EMU7800.Shell;
using System.Linq;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class CommandLineWin32Driver : ICommandLineDriver
{
    readonly ILogger _logger;

    #region ICommandLineDriver Members

    public void Start(bool startMaximized)
    {
        var window = new Window(_logger);
        var windowDriver = new WindowWin32Driver(window, _logger);
        windowDriver.ProcessEvents(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        var window = new Window(gpivi, _logger);
        var windowDriver = new WindowWin32Driver(window, _logger);
        windowDriver.ProcessEvents(startMaximized);
    }

    #endregion

    #region Constructors

    public CommandLineWin32Driver(string[] args, ILogger logger)
    {
        _logger = logger;
        if (args.Any(a => a.Equals("-c", System.StringComparison.OrdinalIgnoreCase) || a.Equals("/c", System.StringComparison.OrdinalIgnoreCase)))
        {
            AllocConsole();
        }
    }

    #endregion

    #region Helpers

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    #endregion
}
