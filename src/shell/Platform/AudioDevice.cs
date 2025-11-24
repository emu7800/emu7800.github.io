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
    public static readonly EmptyAudioDeviceDriver Default = new();
    EmptyAudioDeviceDriver() {}

    #region IAudioDeviceDriver Members

    public int EC { get; }
    public void Close() {}
    public int GetBuffersQueued() => 0;
    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength) {}
    public void SubmitBuffer(ReadOnlySpan<byte> buffer) {}

    #endregion
}

public static class AudioDevice
{
    public static Func<IAudioDeviceDriver> DriverFactory { get; set; } = () => EmptyAudioDeviceDriver.Default;

    static IAudioDeviceDriver _driver = EmptyAudioDeviceDriver.Default;

    public static int Frequency { get; private set; }
    public static int BufferPayloadSizeInBytes { get; private set; }
    public static int QueueLength { get; private set; }
    public static bool IsOpened { get; private set; }
    public static bool IsClosed => !IsOpened;
    public static int EC => _driver.EC;

    public static int CountBuffersQueued()
      => IsOpened ? _driver.GetBuffersQueued() : -1;

    public static void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < BufferPayloadSizeInBytes)
            throw new ApplicationException("Bad SubmitBuffer request: buffer length is not at least " + BufferPayloadSizeInBytes);

        if (!IsOpened)
        {
            _driver.Open(Frequency, BufferPayloadSizeInBytes, QueueLength);
            IsOpened = EC == 0;
        }

        if (IsOpened)
        {
            _driver.SubmitBuffer(buffer);
        }
    }

    public static void Close()
    {
        if (IsOpened)
        {
            _driver.Close();
            IsOpened = false;
        }
    }

    public static void Configure(int frequency, int bufferSizeInBytes, int queueLength)
    {
        if (frequency < 0)
        {
            frequency = 0;
        }

        bufferSizeInBytes = bufferSizeInBytes switch
        {
            < 0 => 0,
            > 0x400 => 0x400,
            _ => bufferSizeInBytes
        };

        queueLength = queueLength switch
        {
            < 0 => 0,
            > 0x10 => 0x10,
            _ => queueLength
        };

        if (Frequency != frequency || BufferPayloadSizeInBytes != bufferSizeInBytes || QueueLength != queueLength)
        {
            Close();
        }

        Frequency = frequency;
        BufferPayloadSizeInBytes = bufferSizeInBytes;
        QueueLength = queueLength;

        _driver = DriverFactory();
    }
}
