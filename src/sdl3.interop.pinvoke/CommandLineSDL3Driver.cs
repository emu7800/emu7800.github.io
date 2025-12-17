using EMU7800.Services.Dto;
using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public class CommandLineSDL3Driver : ICommandLineDriver
{
    #region ICommandLineDriver Members

    public virtual void AttachConsole(bool _) {}

    public void Start(bool startMaximized)
    {
        var logger = new SDLConsoleLogger { Level = 9 };
        var window = new Window(logger);
        var windowDriver = new WindowSDL3Driver(window, logger, startMaximized);
        windowDriver.ProcessEvents();
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized)
    {
        var logger = new SDLConsoleLogger { Level = 9 };
        var window = new Window(gpivi, logger);
        var windowDriver = new WindowSDL3Driver(window, logger, startMaximized);
        windowDriver.ProcessEvents();
    }

    #endregion

    #region Constructors

    public CommandLineSDL3Driver() {}

    #endregion
}
