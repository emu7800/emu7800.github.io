/*
 * WinmmNativeMethods.cs
 *
 * .NET interface to the Windows Multimedia Library
 *
 * Copyright © 2006-2008 Mike Murphy
 *
 */
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop;

internal static unsafe partial class WinmmNativeMethods
{
    const int ChannelLimit = 10;
    static readonly IntPtr[] Index       = new nint[ChannelLimit];
    static readonly IntPtr[] Storage     = new nint[ChannelLimit];
    static readonly int[] StorageSize    = new int[ChannelLimit];
    static readonly int[] SoundFrameSize = new int[ChannelLimit];
    static readonly int[] QueueLen       = new int[ChannelLimit];

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
        WAVEFORMATEX wfx;
        wfx.wFormatTag = 1; // WAVE_FORMAT_PCM
        wfx.nChannels = 1;
        wfx.wBitsPerSample = 8;
        wfx.nSamplesPerSec = (uint)freq;
        wfx.nAvgBytesPerSec = (uint)freq;
        wfx.nBlockAlign = 1;
        wfx.cbSize = 0;

        var hwo = IntPtr.Zero;

        const uint WAVE_MAPPER = unchecked((uint)-1);
        mmResult = waveOutOpen(&hwo, WAVE_MAPPER, &wfx, IntPtr.Zero, IntPtr.Zero, 0);

        if (mmResult != 0)
            return IntPtr.Zero;

        int i;
        for (i = 0; i < Index.Length && Index[i] != IntPtr.Zero; i++);

        if (i < Index.Length)
        {
            queueLen &= 0x3f;
            if (queueLen < 2)
                queueLen = 2;

            Index[i] = hwo;
            QueueLen[i] = queueLen;
            SoundFrameSize[i] = soundFrameSize;
            StorageSize[i] = queueLen * (sizeof(WAVEHDR) + soundFrameSize);
            Storage[i] = Marshal.AllocHGlobal(StorageSize[i]);

            var ptr = (byte*)Storage[i];
            for (var j = 0; j < queueLen; j++)
            {
                var waveHdr = (WAVEHDR*)ptr;
                waveHdr->dwFlags = WHDR_DONE;
                ptr += sizeof(WAVEHDR);
                ptr += SoundFrameSize[i];
            }
        }

        return hwo;
    }

    internal static int SetVolume(IntPtr hwo, int left, int right)
    {
        var uLeft = (uint)left;
        var uRight = (uint)right;
        var nVolume = (uLeft & 0xffff) | ((uRight & 0xffff) << 16);
        return waveOutSetVolume(hwo, nVolume);
    }

    internal static int SetVolume(IntPtr hwo, uint nVolume)
    {
        return waveOutSetVolume(hwo, nVolume);
    }

    internal static int GetVolume(IntPtr hwo)
    {
        uint nVolume;
        _ = waveOutGetVolume(hwo, &nVolume);
        return (int)nVolume;
    }

    internal static int Enqueue(IntPtr hwo, ReadOnlySpan<byte> buffer)
    {
        int i;
        for (i = 0; i < Index.Length && Index[i] != hwo; i++);

        if (i >=  Index.Length)
            return -1;

        if (buffer.Length < SoundFrameSize[i])
            throw new ApplicationException("Bad enqueue request: buffer length is not at least " + SoundFrameSize[i]);

        var queued = false;
        var usedBuffers = 0;

        var ptr = (byte*)Storage[i];
        for (var j = 0; j < QueueLen[i]; j++, ptr += sizeof(WAVEHDR) + SoundFrameSize[i])
        {
            var waveHdr = (WAVEHDR*)ptr;
            if ((waveHdr->dwFlags & WHDR_DONE) == WHDR_DONE)
            {
                if (queued)
                    continue;
                _ = waveOutUnprepareHeader(hwo, waveHdr, (uint)sizeof(WAVEHDR));
                waveHdr->dwBufferLength = (uint)SoundFrameSize[i];
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
        if (hwo == IntPtr.Zero)
            return -1;

        int i;
        for (i = 0; i < Index.Length && Index[i] != hwo; i++);

        if (i >= Index.Length)
            return -1;

        var queued = 0;

        var ptr = (byte*)Storage[i];
        for (var j = 0; j < QueueLen[i]; j++, ptr += sizeof(WAVEHDR) + SoundFrameSize[i])
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
        if (Storage.Equals(IntPtr.Zero))
            return;

        int i;
        for (i = 0; i < Index.Length && Index[i] != hwo; i++);

        if (i >= Index.Length)
            return;

        _ = waveOutReset(hwo);
        _ = waveOutClose(hwo);

        Marshal.FreeHGlobal(Storage[i]);
        Index[i] = Storage[i] = IntPtr.Zero;
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