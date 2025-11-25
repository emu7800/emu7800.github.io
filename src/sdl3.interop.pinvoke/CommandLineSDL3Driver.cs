using EMU7800.Services.Dto;
using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class CommandLineSDL3Driver : ICommandLineDriver
{
    public static CommandLineSDL3Driver Factory() => new();
    CommandLineSDL3Driver() {}

    #region ICommandLineDriver Members

    public void AttachConsole(bool _) {}

    public void Start(bool startMaximized)
    {
        Window.DriverFactory = WindowSDL3Driver.Factory;
        Window.Start(startMaximized);
    }

    public void StartGameProgram(GameProgramInfoViewItem gpivi)
    {
        Window.DriverFactory = WindowSDL3Driver.Factory;
        Window.Start(true, gpivi);
    }

    #endregion
}
