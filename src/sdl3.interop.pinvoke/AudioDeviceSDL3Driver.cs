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
        if (_stream != IntPtr.Zero)
        {
            SDL_DestroyAudioStream(_stream);
            _stream = IntPtr.Zero;
        }
    }

    public int GetBuffersQueued()
    {
        if (_stream == IntPtr.Zero)
            return -1;
        var bytesQueued = SDL_GetAudioStreamQueued(_stream);
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

        _stream = SDL_OpenAudioDeviceStream(0xFFFFFFFFu /*SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK */, ref spec, IntPtr.Zero, IntPtr.Zero);

        if (_stream == IntPtr.Zero)
        {
            HR = -1;
            return false;
        }

        HR = 0;

        SDL_SetAudioStreamGain(_stream, 1f);
        SDL_ResumeAudioStreamDevice(_stream);

        return true;
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
      => SDL_PutAudioStreamData(_stream, buffer, buffer.Length);

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
