using EMU7800.Shell;
using System;

namespace EMU7800.SDL3.Interop;

public sealed class AudioDeviceSDL3Driver : IAudioDeviceDriver
{
    public int EC { get; }

    public static AudioDeviceSDL3Driver Factory() => new();

    public void Close()
    {
    }

    public int GetBuffersQueued()
    {
        return -1;
    }

    public void Open(int frequency, int bufferPayloadSizeInBytes, int queueLength)
    {
        var count = SDL3.SDL_GetNumAudioDrivers();
        for (var i = 0; i < count; i++)
        {
            var name = SDL3.SDL_GetAudioDriver(i);
            Console.WriteLine("SDL Audio Driver: " + name);
        }
    }

    public void SubmitBuffer(ReadOnlySpan<byte> buffer)
    {
    }

    AudioDeviceSDL3Driver() { }
}
