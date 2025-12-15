// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class AudioDeviceWinmmDriver : IAudioDeviceDriver
{
    IntPtr hwo;

    #region IAudioDeviceDriver Members

    public int EC { get; private set; }

    public void Close()
      => WinmmNativeMethods.Close(hwo);

    public int GetBuffersQueued()
      => WinmmNativeMethods.GetBuffersQueued(hwo);

    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength)
    {
        hwo = WinmmNativeMethods.Open(frequency, bufferPayloadSizeInBytes, queueLength, out var ec);
        EC = ec;
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
      => WinmmNativeMethods.Enqueue(hwo, buffer);

    #endregion
}
