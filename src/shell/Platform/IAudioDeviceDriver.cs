using System;

namespace EMU7800.Shell;

public interface IAudioDeviceDriver
{
    void Close();
    int GetBuffersQueued();
    bool Open(int frequency, int bufferPayloadSizeInBytes, int queueLength);
    void SubmitBuffer(ReadOnlySpan<byte> buffer);
}

public sealed class EmptyAudioDeviceDriver : IAudioDeviceDriver
{
    public readonly static EmptyAudioDeviceDriver Default = new();
    EmptyAudioDeviceDriver() {}

    #region IAudioDeviceDriver Members
    public void Close() {}
    public int GetBuffersQueued() => 0;
    public bool Open(int frequency, int bufferPayloadSizeInBytes, int queueLength) => false;
    public void SubmitBuffer(ReadOnlySpan<byte> buffer) { }

    #endregion
}