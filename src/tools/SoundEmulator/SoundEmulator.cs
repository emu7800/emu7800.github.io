using System;
using System.Threading;
using EMU7800.Core;
using EMU7800.Win;

namespace EMU7800.SoundEmulator
{
    public class SoundEmulator
    {
        #region Fields

        readonly Random _random = new();
        readonly int _buffers;

        Thread _workerThread;
        bool _stopRequested, _playNoise;
        MachineSoundEmulator _machine;

        #endregion

        public Action<SoundEmulator> GetRegisterSettingsForNextFrame;

        public void PokeTia(byte tiaRegister, byte value)
        {
            if (_machine == null)
                return;
            _machine.PokeTia(tiaRegister, value);
        }

        public void PokePokey(byte pokeyRegister, byte value)
        {
            if (_machine == null)
                return;
            _machine.PokePokey(pokeyRegister, value);
        }

        public void StartNTSC()
        {
            if (_workerThread != null)
                throw new InvalidOperationException("Already started.");

            _machine = MachineSoundEmulator.CreateForNTSC();

            _workerThread = new Thread(Run);
            _workerThread.Start();
        }

        public void StartPAL()
        {
            if (_workerThread != null)
                throw new InvalidOperationException("Already started.");

            _machine = MachineSoundEmulator.CreateForPAL();

            _workerThread = new Thread(Run);
            _workerThread.Start();
        }

        public void StartNoise()
        {
            if (_workerThread != null)
                throw new InvalidOperationException("Already started.");

            _machine = MachineSoundEmulator.CreateForNTSC();
            _playNoise = true;

            _workerThread = new Thread(Run);
            _workerThread.Start();
        }

        public void Stop()
        {
            if (_workerThread == null)
                return;

            _stopRequested = true;
            _playNoise = false;

            _workerThread.Join();
            _workerThread = null;
        }

        public void WaitUntilStopped()
        {
            if (_workerThread == null)
                return;

            _workerThread.Join();
            _workerThread = null;
        }

        #region Constructors

        public SoundEmulator() : this(8)
        {
        }

        public SoundEmulator(int buffers)
        {
            if (buffers < 1 || buffers > 64)
                throw new ArgumentException("buffers must be between 1 and 64.");
            _buffers = buffers;
        }

        #endregion

        void Run()
        {
            var framebuffer = _machine.CreateFrameBuffer();

            WinmmNativeMethods.Open(_machine.SoundSampleFrequency, framebuffer.SoundBufferByteLength, _buffers);

            while (!_stopRequested)
            {
                GetRegisterSettingsForNextFrame?.Invoke(this);

                if (_playNoise)
                    ComputeNoiseFrame(framebuffer);
                else
                    _machine.ComputeNextFrame(framebuffer);

                while (!_stopRequested)
                {
                    var buffersQueued = WinmmNativeMethods.Enqueue(framebuffer);
                    if (buffersQueued >= 0)
                        break;
                    Thread.Yield();
                }
            }

            WinmmNativeMethods.Close();
        }

        void ComputeNoiseFrame(FrameBuffer framebuffer)
        {
            for (var i = 0; i < framebuffer.SoundBufferElementLength * BufferElement.SIZE; i++)
            {
                framebuffer.SoundBuffer[i >> BufferElement.SHIFT][i] = (byte)(_random.Next(2));
            }
        }
    }
}
