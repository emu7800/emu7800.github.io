using System;

namespace EMU7800.Win32.Interop;

public static class AudioDevice
{
    static int Frequency, BufferPayloadSizeInBytes, QueueLength;

    public static bool IsOpened { get; private set; }
    public static bool IsClosed => !IsOpened;

    public static uint WaveOutVolume
    {
        get => (uint)WinmmNativeMethods.GetVolume();
        set => WinmmNativeMethods.SetVolume(value);
    }

    public static uint ToVolume(int left, int right)
        => ((uint)left & 0xffff) | (((uint)right & 0xffff) << 16);

    public static int CountBuffersQueued()
        => IsOpened? WinmmNativeMethods.GetBuffersQueued() : -1;

    public static void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < BufferPayloadSizeInBytes)
            throw new ApplicationException("Bad SubmitBuffer request: buffer length is not at least " + BufferPayloadSizeInBytes);

        if (!IsOpened)
        {
            var ec = WinmmNativeMethods.Open(Frequency, BufferPayloadSizeInBytes, QueueLength);
            IsOpened = ec == 0;
        }

        if (IsOpened)
        {
            WinmmNativeMethods.Enqueue(buffer);
        }
    }

    public static void Close()
    {
        if (IsOpened)
        {
            WinmmNativeMethods.Close();
            IsOpened = false;
        }
    }

    public static void Configure(int frequency, int bufferSizeInBytes, int queueLength)
    {
        if (frequency < 0)
            frequency = 0;

        bufferSizeInBytes = bufferSizeInBytes switch
        {
            < 0     => 0,
            > 0x400 => 0x400,
            _ => bufferSizeInBytes
        };

        queueLength = queueLength switch
        {
            < 0    => 0,
            > 0x10 => 0x10,
            _ => queueLength
        };

        Frequency = frequency;
        BufferPayloadSizeInBytes = bufferSizeInBytes;
        QueueLength = queueLength;
    }
}