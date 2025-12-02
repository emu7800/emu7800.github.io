using EMU7800.Shell;
using System;

namespace EMU7800.SDL3.Interop;

public sealed class AudioDeviceSDL3Driver : IAudioDeviceDriver
{
    public static AudioDeviceSDL3Driver Factory() => new();
    AudioDeviceSDL3Driver() {}

    #region IAudioDeviceDriver Members

    public int EC { get; }

    public void Close()
    {
    }

    public int GetBuffersQueued()
    {
        return 0;
    }

    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength)
    {
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
    }

    #endregion
}
