// © Mike Murphy

namespace EMU7800.Shell;

public interface IGameControllersDriver
{
    GameController[] Controllers { get; }
    void Poll();
}

public sealed class EmptyGameControllersDriver : IGameControllersDriver
{
    public readonly static EmptyGameControllersDriver Default = new();
    EmptyGameControllersDriver() {}

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; } = [];
    public void Poll() {}

    #endregion
}
