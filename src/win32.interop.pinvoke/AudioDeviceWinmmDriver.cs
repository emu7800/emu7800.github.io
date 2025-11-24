// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class AudioDeviceWinmmDriver : IAudioDeviceDriver
{
    public static AudioDeviceWinmmDriver Factory() => new();
    AudioDeviceWinmmDriver() {}

    #region IAudioDeviceDriver Members

    public int EC { get; private set; }

    public void Close()
      => WinmmNativeMethods.Close();

    public int GetBuffersQueued()
      => WinmmNativeMethods.GetBuffersQueued();

    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength)
      => EC = WinmmNativeMethods.Open(frequency, bufferPayloadSizeInBytes, queueLength);

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
      => WinmmNativeMethods.Enqueue(buffer);

    #endregion
}
