using System;

namespace EMU7800.Shell;

public interface IAudioDeviceDriver
{
    public int EC { get; }
    void Close();
    int GetBuffersQueued();
    void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength);
    void SubmitBuffer(ReadOnlySpan<byte> buffer);
}

public sealed class EmptyAudioDeviceDriver : IAudioDeviceDriver
{
    public readonly static EmptyAudioDeviceDriver Default = new();
    EmptyAudioDeviceDriver() { }

    #region IAudioDeviceDriver Members

    public int EC { get; }
    public void Close() { }
    public int GetBuffersQueued() => 0;
    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength) { }
    public void SubmitBuffer(ReadOnlySpan<byte> buffer) { }

    #endregion
}