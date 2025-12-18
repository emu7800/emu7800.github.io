// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class AudioDeviceWinmmDriver : DisposableResource, IAudioDeviceDriver
{
    IntPtr hwo;

    #region IAudioDeviceDriver Members

    public void Close()
      => WinmmNativeMethods.Close(hwo);

    public int GetBuffersQueued()
      => WinmmNativeMethods.GetBuffersQueued(hwo);

    public bool Open(int frequency, int soundFrameSize, int queueLength)
    {
        hwo = WinmmNativeMethods.Open(frequency, soundFrameSize, queueLength, out var ec);
        HR = ec;
        return ec == 0;
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
      => WinmmNativeMethods.Enqueue(hwo, buffer);

    #endregion

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            WinmmNativeMethods.Close(hwo);
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion
}
