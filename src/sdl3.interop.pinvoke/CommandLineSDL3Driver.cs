using EMU7800.Core;
using EMU7800.Services.Dto;
using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class CommandLineSDL3Driver : ICommandLineDriver
{
    readonly ILogger _logger;

    #region ICommandLineDriver Members

    public void Start(bool startMaximized)
    {
        var window = new Window(_logger);
        var windowDriver = new WindowSDL3Driver(window, _logger, startMaximized);
        windowDriver.ProcessEvents();
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        var window = new Window(gpivi, _logger);
        var windowDriver = new WindowSDL3Driver(window, _logger, startMaximized);
        windowDriver.ProcessEvents();
    }

    #endregion

    #region Constructors

    public CommandLineSDL3Driver(ILogger logger)
      => _logger = logger;

    #endregion
}
