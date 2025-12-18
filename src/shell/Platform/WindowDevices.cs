// © Mike Murphy

namespace EMU7800.Shell;

public record WindowDevices(
    Window Window,
    IGraphicsDeviceDriver GraphicsDevice,
    IAudioDeviceDriver AudioDevice,
    IGameControllersDriver GameControllers);
