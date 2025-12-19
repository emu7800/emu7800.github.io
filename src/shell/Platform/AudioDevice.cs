using System;

namespace EMU7800.Shell;

public sealed class AudioDevice
{
    readonly IAudioDeviceDriver _driver;

    public int Frequency { get; private set; }
    public int SoundFrameSize { get; private set; }
    public int QueueLength { get; private set; }
    public bool IsOpened { get; private set; }
    public bool IsClosed => !IsOpened;
    public int CountBuffersQueued()
      => IsOpened ? _driver.GetBuffersQueued() : -1;

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < SoundFrameSize)
            throw new ApplicationException("Bad SubmitBuffer request: buffer length is not at least " + SoundFrameSize);

        if (!IsOpened)
        {
            IsOpened = _driver.Open(Frequency, SoundFrameSize, QueueLength);
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

    public void Configure(int frequency, int soundFrameSize, int queueLength)
    {
        if (frequency < 1)
        {
            frequency = 1;
        }

        soundFrameSize = soundFrameSize switch
        {
            < 1 => 1,
            > 0x400 => 0x400,
            _ => soundFrameSize
        };

        queueLength = queueLength switch
        {
            < 1 => 1,
            > 0x10 => 0x10,
            _ => queueLength
        };

        if (Frequency != frequency || SoundFrameSize != soundFrameSize || QueueLength != queueLength)
        {
            Close();
        }

        Frequency = frequency;
        SoundFrameSize = soundFrameSize;
        QueueLength = queueLength;
    }

    #region Constructors

    public AudioDevice(IAudioDeviceDriver driver)
      => _driver = driver;

    #endregion
}
