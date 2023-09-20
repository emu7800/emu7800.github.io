using System;
using System.Threading;
using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.SoundEmulator;

public class SoundEmulator
{
    #region Fields

    readonly Random _random = new();

    Thread _workerThread = new(() => {});
    bool _stopRequested, _playNoise;
    MachineSoundEmulator _machine = MachineSoundEmulator.Default;

    #endregion

    public int Buffers { get; set; } = 8;

    public Action<SoundEmulator> GetRegisterSettingsForNextFrame = (se) => {};

    public void PokeTia(byte tiaRegister, byte value)
    {
        if (_machine == MachineSoundEmulator.Default)
            return;
        _machine.PokeTia(tiaRegister, value);
    }

    public void PokePokey(byte pokeyRegister, byte value)
    {
        if (_machine == MachineSoundEmulator.Default)
            return;
        _machine.PokePokey(pokeyRegister, value);
    }

    public void StartNTSC()
    {
        if (_workerThread.IsAlive)
            throw new InvalidOperationException("Already started");

        _machine = MachineSoundEmulator.CreateForNTSC();

        _workerThread = new Thread(Run);
        _workerThread.Start();
    }

    public void StartPAL()
    {
        if (_workerThread.IsAlive)
            throw new InvalidOperationException("Already started");

        _machine = MachineSoundEmulator.CreateForPAL();

        _workerThread = new Thread(Run);
        _workerThread.Start();
    }

    public void StartNoise()
    {
        if (_workerThread.IsAlive)
            throw new InvalidOperationException("Already started");

        _machine = MachineSoundEmulator.CreateForNTSC();
        _playNoise = true;

        _workerThread = new Thread(Run);
        _workerThread.Start();
    }

    public void Stop()
    {
        if (_workerThread.IsAlive)
        {
            _stopRequested = true;
            _playNoise = false;
            _workerThread.Join();
        }
     }

    public void WaitUntilStopped()
    {
        if (_workerThread.IsAlive)
        {
            _workerThread.Join();
        }
    }

    void Run()
    {
        var framebuffer = _machine.CreateFrameBuffer();

        var buffers = Buffers > 0 && Buffers < 65 ? Buffers : 8;

        WinmmNativeMethods.Open(_machine.SoundSampleFrequency, framebuffer.SoundBuffer.Length, Buffers);

        while (!_stopRequested)
        {
            GetRegisterSettingsForNextFrame?.Invoke(this);

            if (_playNoise)
                ComputeNoiseFrame(framebuffer);
            else
                _machine.ComputeNextFrame(framebuffer);

            while (!_stopRequested)
            {
                var buffersQueued = WinmmNativeMethods.Enqueue(framebuffer.SoundBuffer.Span);
                if (buffersQueued >= 0)
                    break;
                Thread.Yield();
            }
        }

        WinmmNativeMethods.Close();
    }

    void ComputeNoiseFrame(FrameBuffer framebuffer)
    {
        for (var i = 0; i < framebuffer.SoundBuffer.Length; i++)
        {
            framebuffer.SoundBuffer.Span[i] = (byte)_random.Next(2);
        }
    }
}
