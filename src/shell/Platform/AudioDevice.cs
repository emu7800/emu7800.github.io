using System;

namespace EMU7800.Shell;

public sealed class AudioDevice
{
    readonly IAudioDeviceDriver _driver;

    public int Frequency { get; private set; }
    public int BufferPayloadSizeInBytes { get; private set; }
    public int QueueLength { get; private set; }
    public bool IsOpened { get; private set; }
    public bool IsClosed => !IsOpened;
    public int CountBuffersQueued()
      => IsOpened ? _driver.GetBuffersQueued() : -1;

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < BufferPayloadSizeInBytes)
            throw new ApplicationException("Bad SubmitBuffer request: buffer length is not at least " + BufferPayloadSizeInBytes);

        if (!IsOpened)
        {
            IsOpened = _driver.Open(Frequency, BufferPayloadSizeInBytes, QueueLength);
        }

        if (IsOpened)
        {
            _driver.SubmitBuffer(buffer);
        }
    }

    public void Close()
    {
        if (IsOpened)
        {
            _driver.Close();
            IsOpened = false;
        }
    }

    public void Configure(int frequency, int bufferSizeInBytes, int queueLength)
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
    }

    #region Constructors

    #pragma warning disable IDE0290 // Use primary constructor

    public AudioDevice(IAudioDeviceDriver driver)
      => _driver = driver;

    #endregion
}
