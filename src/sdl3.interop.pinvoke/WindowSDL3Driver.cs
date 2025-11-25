using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class WindowSDL3Driver : IWindowDriver
{
    public static WindowSDL3Driver Factory() => new();
    WindowSDL3Driver() {}

    #region IWindowDriver Members

    public void StartWindowAndProcessEvents(bool startMaximized)
    {
        AudioDevice.DriverFactory = AudioDeviceSDL3Driver.Factory;

        // prints out the available audio drivers
        AudioDevice.Configure(1, 0x400, 0x10);
        AudioDevice.SubmitBuffer(new byte[0x400]);
    }

    #endregion
}