// © Mike Murphy

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop;

internal static unsafe partial class WinmmNativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    struct SoundQueueEntry
    {
        public IntPtr hwo;
        public IntPtr storage;
        public int storageSize;
        public ushort soundFrameSize;
        public byte queueLen;
    }
    static readonly SoundQueueEntry[] SoundQueues = new SoundQueueEntry[10];

    const uint WHDR_DONE = 0x00000001;  // WAVEHDR done flag

    [StructLayout(LayoutKind.Sequential)]
    internal struct WAVEFORMATEX
    {
        internal ushort wFormatTag;     // format type
        internal ushort nChannels;      // number of channels (i.e. mono, stereo...)
        internal uint nSamplesPerSec;   // sample rate
        internal uint nAvgBytesPerSec;  // for buffer estimation
        internal ushort nBlockAlign;    // block size of data
        internal ushort wBitsPerSample; // number of bits per sample of mono data
        internal ushort cbSize;         // the count in bytes of the size of extra information (after cbSize)
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WAVEHDR
    {
        internal byte* lpData;          // pointer to locked data buffer
        internal uint dwBufferLength;   // length of data buffer
        internal uint dwBytesRecorded;  // used for input only
        internal uint* dwUser;          // for client's use
        internal uint dwFlags;          // assorted flags (see defines)
        internal uint dwLoops;          // loop control counter
        internal WAVEHDR* lpNext;       // reserved for driver
        internal uint* reserved;        // reserved for driver
    }

    internal static IntPtr Open(int freq, int soundFrameSize, int queueLen, out int mmResult)
    {
        if (freq < 1 || soundFrameSize < 1 || soundFrameSize > ushort.MaxValue)
        {
            mmResult = -1;
            return IntPtr.Zero;
        }

        WAVEFORMATEX wfx;
        wfx.wFormatTag = 1; // WAVE_FORMAT_PCM
        wfx.nChannels = 1;
        wfx.wBitsPerSample = 8;
        wfx.nSamplesPerSec = (uint)freq;
        wfx.nAvgBytesPerSec = (uint)freq;
        wfx.nBlockAlign = 1;
        wfx.cbSize = 0;

        var hwo = IntPtr.Zero;

        var i = FindSoundQueueEntryIndex(hwo);
        if (i < 0)
        {
            mmResult = -2;
            return IntPtr.Zero;
        }

        const uint WAVE_MAPPER = unchecked((uint)-1);
        mmResult = waveOutOpen(&hwo, WAVE_MAPPER, &wfx, IntPtr.Zero, IntPtr.Zero, 0);

        if (mmResult != 0)
            return IntPtr.Zero;

        SoundQueues[i].hwo = hwo;
        SoundQueues[i].soundFrameSize = (ushort)soundFrameSize;

        queueLen &= 0x3f;
        if (queueLen < 2)
            queueLen = 2;

        SoundQueues[i].queueLen = (byte)queueLen;
        SoundQueues[i].storageSize = (ushort)(SoundQueues[i].queueLen * (sizeof(WAVEHDR) + SoundQueues[i].soundFrameSize));
        SoundQueues[i].storage = Marshal.AllocHGlobal(SoundQueues[i].storageSize);

        var ptr = (byte*)SoundQueues[i].storage;
        for (var j = 0; j < queueLen; j++)
        {
            var waveHdr = (WAVEHDR*)ptr;
            waveHdr->dwFlags = WHDR_DONE;
            ptr += sizeof(WAVEHDR);
            ptr += SoundQueues[i].soundFrameSize;
        }

        return hwo;
    }

    internal static int SetVolume(IntPtr hwo, int left, int right)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return -1;

        var uLeft = (uint)left;
        var uRight = (uint)right;
        var nVolume = (uLeft & 0xffff) | ((uRight & 0xffff) << 16);
        return waveOutSetVolume(hwo, nVolume);
    }

    internal static int SetVolume(IntPtr hwo, uint nVolume)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return -1;

        return waveOutSetVolume(hwo, nVolume);
    }

    internal static int GetVolume(IntPtr hwo)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return -1;

        uint nVolume;
        _ = waveOutGetVolume(hwo, &nVolume);
        return (int)nVolume;
    }

    internal static int Enqueue(IntPtr hwo, ReadOnlySpan<byte> buffer)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return -1;

        var sqe = SoundQueues[i];

        if (buffer.Length < sqe.soundFrameSize)
            throw new ApplicationException("Bad enqueue request: buffer length is not at least " + sqe.soundFrameSize);

        var usedBuffers = 0;
        var queued = false;

        var ptr = (byte*)sqe.storage;
        var ptrInc = sizeof(WAVEHDR) + sqe.soundFrameSize;
        for (var j = 0; j < sqe.queueLen; j++, ptr += ptrInc)
        {
            var waveHdr = (WAVEHDR*)ptr;
            if ((waveHdr->dwFlags & WHDR_DONE) == WHDR_DONE)
            {
                if (queued)
                    continue;
                _ = waveOutUnprepareHeader(hwo, waveHdr, (uint)sizeof(WAVEHDR));
                waveHdr->dwBufferLength = sqe.soundFrameSize;
                waveHdr->dwFlags = 0;
                waveHdr->lpData = ptr + sizeof(WAVEHDR);
                for (var k = 0; k < buffer.Length; k++)
                {
                    // convert to WAV format
                    waveHdr->lpData[k] = (byte)(buffer[k] | 0x80);
                }
                _ = waveOutPrepareHeader(hwo, waveHdr, (uint)sizeof(WAVEHDR));
                _ = waveOutWrite(hwo, waveHdr, (uint)sizeof(WAVEHDR));
                queued = true;
                continue;
            }
            usedBuffers++;
        }

        return queued ? usedBuffers : -1;
    }

    internal static int GetBuffersQueued(IntPtr hwo)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return -1;

        var sqe = SoundQueues[i];
        var queued = 0;

        var ptr = (byte*)sqe.storage;
        var ptrInc = sizeof(WAVEHDR) + sqe.soundFrameSize;
        for (var j = 0; j < sqe.queueLen; j++, ptr += ptrInc)
        {
            var waveHdr = (WAVEHDR*)ptr;
            if ((waveHdr->dwFlags & WHDR_DONE) != WHDR_DONE)
            {
                queued++;
            }
        }

        return queued;
    }

    internal static void Close(IntPtr hwo)
    {
        var i = hwo != IntPtr.Zero ? FindSoundQueueEntryIndex(hwo) : -1;
        if (i < 0)
            return;

        _ = waveOutReset(hwo);
        _ = waveOutClose(hwo);

        Marshal.FreeHGlobal(SoundQueues[i].storage);
        SoundQueues[i].hwo = IntPtr.Zero;
        SoundQueues[i].storage = IntPtr.Zero;
        SoundQueues[i].storageSize = 0;
        SoundQueues[i].soundFrameSize = 0;
        SoundQueues[i].queueLen = 0;
    }

    static int FindSoundQueueEntryIndex(IntPtr hwo)
    {
        var i = 0;
        for (; i < SoundQueues.Length && SoundQueues[i].hwo != hwo; i++);
        return i < SoundQueues.Length ? i : -1;
    }

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutOpen(IntPtr* phwo, uint uDeviceID, WAVEFORMATEX* pwfx, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutSetVolume(IntPtr hwo, uint dwVolume);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutGetVolume(IntPtr hwo, uint* pdwVolume);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutPrepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutUnprepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutWrite(IntPtr hwo, WAVEHDR* wh, uint cbwh);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutReset(IntPtr hwo);

    [LibraryImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial int waveOutClose(IntPtr hwo);
}