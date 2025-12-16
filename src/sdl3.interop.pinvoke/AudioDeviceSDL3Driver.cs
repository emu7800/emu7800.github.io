using EMU7800.Shell;
using System;

namespace EMU7800.SDL3.Interop;

public sealed class AudioDeviceSDL3Driver : DisposableResource, IAudioDeviceDriver
{
    #region IAudioDeviceDriver Members

    public void Close()
    {
    }

    public int GetBuffersQueued()
    {
        return 0;
    }

    public bool Open(int frequency, int bufferPayloadSizeInBytes, int queueLength)
      => false;

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
    }

    #endregion

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion
}
