using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class GameControllersSDL3InputDriver : IGameControllersDriver
{
    public static GameControllersSDL3InputDriver Factory() => new();

    GameControllersSDL3InputDriver() {}

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; } = [];

    public void Initialize()
    {
    }

    public void Poll()
    {
    }

    public void Shutdown()
    {
    }

    #endregion
}