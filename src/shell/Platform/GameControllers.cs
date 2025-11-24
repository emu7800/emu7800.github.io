// © Mike Murphy

using System;

namespace EMU7800.Shell;

public interface IGameControllersDriver
{
    GameController[] Controllers { get; }
    void Initialize();
    void Poll();
    void Shutdown();
}

public sealed class EmptyGameControllersDriver : IGameControllersDriver
{
    public static readonly EmptyGameControllersDriver Default = new();
    EmptyGameControllersDriver() {}

    #region IGameControllersDriver Members

    public GameController[] Controllers => [];
    public void Initialize() {}
    public void Poll() {}
    public void Shutdown() {}

    #endregion
}

public static class GameControllers
{
    public static Func<IGameControllersDriver> DriverFactory { get; set; } = () => EmptyGameControllersDriver.Default;

    static IGameControllersDriver _driver = EmptyGameControllersDriver.Default;

    public static GameController[] Controllers { get; } = _driver.Controllers;

    public static void Initialize()
    {
        _driver = DriverFactory();
        _driver.Initialize();
    }

    public static void Poll()
      => _driver.Poll();

    public static void Shutdown()
      => _driver.Shutdown();
}