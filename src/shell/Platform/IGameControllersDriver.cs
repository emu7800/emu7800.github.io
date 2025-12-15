// © Mike Murphy

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
    public readonly static EmptyGameControllersDriver Default = new();
    EmptyGameControllersDriver() {}

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; } = [];
    public void Initialize() {}
    public void Poll() {}
    public void Shutdown() {}

    #endregion
}
