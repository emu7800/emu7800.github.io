using EMU7800.Shell;
using System;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class AudioDeviceSDL3Driver : DisposableResource, IAudioDeviceDriver
{
    IntPtr _stream;
    int _soundFrameSize = 1;

    #region IAudioDeviceDriver Members

    public void Close()
    {
        var stream = _stream;
        if (stream == IntPtr.Zero)
            return;

        _stream = IntPtr.Zero;

        System.Threading.Thread.Sleep(100); // allow time for queuing activity to exit

        SDL_DestroyAudioStream(stream);
    }

    public int GetBuffersQueued()
    {
        var stream = _stream;
        if (stream == IntPtr.Zero)
            return -1;

        var bytesQueued = SDL_GetAudioStreamQueued(stream);
        var queued = bytesQueued / _soundFrameSize;
        return queued;
    }

    public bool Open(int frequency, int soundFrameSize, int _)
    {
        if (frequency <= 1 || soundFrameSize <= 1 || soundFrameSize > ushort.MaxValue)
        {
            HR = -1;
            return false;
        }

        Close();

        _soundFrameSize = soundFrameSize;

        SDL_AudioSpec spec;
        spec.format = SDL_AudioFormat.SDL_AUDIO_S8;
        spec.channels = 1;
        spec.freq = frequency;

        var stream = SDL_OpenAudioDeviceStream(0xFFFFFFFFu /*SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK */, ref spec, IntPtr.Zero, IntPtr.Zero);

        if (stream == IntPtr.Zero)
        {
            HR = -1;
            return false;
        }

        HR = 0;

        SDL_SetAudioStreamGain(stream, 1f);
        SDL_ResumeAudioStreamDevice(stream);

        _stream = stream;

        return true;
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
        var stream = _stream;
        if (stream == IntPtr.Zero)
            return;
        SDL_PutAudioStreamData(stream, buffer, buffer.Length);
    }


    #endregion

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            Close();
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion
}
