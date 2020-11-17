/*
 * WinmmNativeMethods.cs
 * 
 * .NET interface to the Windows Multimedia Library
 * 
 * Copyright � 2006-2008 Mike Murphy
 * 
 */
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using EMU7800.Core;

namespace EMU7800.Win
{
    internal unsafe static class WinmmNativeMethods
    {
        static IntPtr Hwo;
        static IntPtr Storage = IntPtr.Zero;
        static int StorageSize;
        static int SoundFrameSize;
        static int QueueLen;

        const uint WHDR_DONE = 0x00000001;  // WAVEHDR done flag

        [StructLayout(LayoutKind.Sequential)]
        struct WAVEFORMATEX
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
        struct WAVEHDR
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

        internal static int Open(int freq, int soundFrameSize, int queueLen)
        {
            QueueLen = queueLen & 0x3f;
            if (QueueLen < 2) QueueLen = 2;
            
            WAVEFORMATEX wfx;
            wfx.wFormatTag = 1; // WAVE_FORMAT_PCM
            wfx.nChannels = 1;
            wfx.wBitsPerSample = 8;
            wfx.nSamplesPerSec = (uint)freq;
            wfx.nAvgBytesPerSec = (uint)freq;
            wfx.nBlockAlign = 1;
            wfx.cbSize = 0;

            Hwo = IntPtr.Zero;
            int mmResult;
            fixed (IntPtr* phwo = &Hwo)
            {
                const uint WAVE_MAPPER = unchecked((uint) -1);
                mmResult = waveOutOpen(phwo, WAVE_MAPPER, &wfx, IntPtr.Zero, IntPtr.Zero, 0);
            }

            if (!mmResult.Equals(0))
                return mmResult;

            SoundFrameSize = soundFrameSize;
            StorageSize = QueueLen * (sizeof(WAVEHDR) + SoundFrameSize);
            Storage = Marshal.AllocHGlobal(StorageSize);
            var ptr = (byte*)Storage;
            for (var i = 0; i < QueueLen; i++)
            {
                var waveHdr = (WAVEHDR*)ptr;
                waveHdr->dwFlags = WHDR_DONE;
                ptr += sizeof(WAVEHDR);
                ptr += SoundFrameSize;
            }

            return 0;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static int SetVolume(int left, int right)
        {
            var uLeft = (uint)left;
            var uRight = (uint)right;
            var nVolume = (uLeft & 0xffff) | ((uRight & 0xffff) << 16);
            return waveOutSetVolume(Hwo, nVolume);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static int GetVolume()
        {
            uint nVolume;
            waveOutGetVolume(Hwo, &nVolume);
            return (int)nVolume;
        }

        internal static int Enqueue(FrameBuffer frameBuffer)
        {
            if (frameBuffer.SoundBufferByteLength < SoundFrameSize)
                throw new ApplicationException("Bad enqueue request: buffer length is not at least " + SoundFrameSize);

            var queued = false;
            var usedBuffers = 0;

            var ptr = (byte*)Storage;
            for (var i = 0; i < QueueLen; i++, ptr += sizeof(WAVEHDR) + SoundFrameSize)
            {
                var waveHdr = (WAVEHDR*)ptr;
                if ((waveHdr->dwFlags & WHDR_DONE) == WHDR_DONE)
                {
                    if (queued) continue;
                    waveOutUnprepareHeader(Hwo, waveHdr, (uint)sizeof(WAVEHDR));
                    waveHdr->dwBufferLength = (uint)SoundFrameSize;
                    waveHdr->dwFlags = 0;
                    waveHdr->lpData = ptr + sizeof(WAVEHDR);
                    for (int j = 0, s = 0; j < frameBuffer.SoundBufferElementLength; j++)
                    {
                        var v = frameBuffer.SoundBuffer[j];
                        // convert to WAV format
                        for (var k = 0; k < BufferElement.SIZE; k++, s++)
                            waveHdr->lpData[s] = (byte)(v[s] + 0x80);
                    }
                    waveOutPrepareHeader(Hwo, waveHdr, (uint)sizeof(WAVEHDR));
                    waveOutWrite(Hwo, waveHdr, (uint)sizeof(WAVEHDR));
                    queued = true;
                    continue;
                }
                usedBuffers++;
            }

            return queued ? usedBuffers : -1;
        }

        internal static void Close()
        {
            if (Storage.Equals(IntPtr.Zero)) return;

            waveOutReset(Hwo);
            waveOutClose(Hwo);
            Marshal.FreeHGlobal(Storage);
            Storage = IntPtr.Zero;
        }

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutOpen(IntPtr* phwo, uint uDeviceID, WAVEFORMATEX* pwfx, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutGetVolume(IntPtr hwo, uint* pdwVolume);

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutPrepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutUnprepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutWrite(IntPtr hwo, WAVEHDR* wh, uint cbwh);

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int waveOutReset(IntPtr hwo);

        [DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
        private static extern uint waveOutClose(IntPtr hwo);
    }
}