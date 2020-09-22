// © Mike Murphy

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EMU7800.Core;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControl : ControlBase
    {
        static readonly AudioDevice AudioDeviceDefault = new AudioDevice(0, 0, 0);

        #region Fields

        readonly uint[] _normalPalette = new uint[0x100];
        readonly uint[] _darkerPalette = new uint[0x100];
        uint[] _currentPalette = Array.Empty<uint>();

        readonly static object _dynamicBitmapLocker = new();
        readonly static byte[] _dynamicBitmapData = new byte[4 * 320 * 230];
        readonly static SizeU _dynamicBitmapDataSize = Struct.ToSizeU(320, 230);
        IFrameRenderer _frameRenderer = new FrameRendererDefault();
        D2DBitmapInterpolationMode _dynamicBitmapInterpolationMode = D2DBitmapInterpolationMode.NearestNeighbor;
        DynamicBitmap _dynamicBitmap = DynamicBitmapDefault;
        RectF _dynamicBitmapRect;
        bool _dynamicBitmapDataUpdated;

        static readonly InputState _defaultInputState = new();
        InputState _inputState = _defaultInputState;

        static readonly InputAdapterNull _defaultInputAdapter = new();
        readonly IInputAdapter[] _inputAdapters = { _defaultInputAdapter, _defaultInputAdapter };
        readonly int[] _playerJackMapping = { 0, 1, 0, 1 };
        readonly int[] _paddleSwaps = { 0, 1, 2, 3 };
        int _currentKeyboardPlayerNo;

        Task _workerTask = Task.CompletedTask;
        bool _stopRequested, _paused, _soundOff;

        bool _calibrationNeeded, _calibrating, _frameRateChangeNeeded;
        readonly uint[] _frameDurationBuckets = new uint[0x100];
        readonly long _stopwatchFrequencyInMilliseconds = Stopwatch.Frequency / 1000;
        uint _frameDurationBucketSamples;
        int _proposedFrameRate, _maxFrameRate = 60;

        #endregion

        public bool IsGameBWConsoleSwitchSet
        {
            get
            {
                var inputState = _inputState;
                return (inputState != null) && inputState.IsGameBWConsoleSwitchSet;
            }
        }

        public bool IsLeftDifficultyAConsoleSwitchSet
        {
            get
            {
                var inputState = _inputState;
                return (inputState != null) && inputState.IsLeftDifficultyAConsoleSwitchSet;
            }
        }

        public bool IsRightDifficultyAConsoleSwitchSet
        {
            get
            {
                var inputState = _inputState;
                return (inputState != null) && inputState.IsRightDifficultyAConsoleSwitchSet;
            }
        }

        public bool IsPaused
        {
            get { return _paused; }
            set
            {
                if (_paused == value)
                    return;
                _paused = value;
                if (!_paused)
                    _calibrationNeeded = true;
            }
        }

        public bool IsSoundOn
        {
            get { return !_soundOff; }
            set
            {
                _soundOff = !value;
                if (!_soundOff)
                    _calibrationNeeded = true;
            }
        }

        public bool IsInTouchMode { get; set; }

        public bool IsAntiAliasOn
        {
            get { return _dynamicBitmapInterpolationMode == D2DBitmapInterpolationMode.Linear; }
            set
            {
                _dynamicBitmapInterpolationMode
                    = value ? D2DBitmapInterpolationMode.Linear : D2DBitmapInterpolationMode.NearestNeighbor;
            }
        }

        public int CurrentFrameRate { get; private set; }

        public float FrameIdleTime { get; private set; }

        public int BuffersQueued { get; private set; }

        public int MinFramesPerSecond { get { return 4; } }

        public int MaxFramesPerSecond { get { return _maxFrameRate; } }

        public void ProposeNewFrameRate(int frameRate)
        {
            if (frameRate < MinFramesPerSecond)
                frameRate = MinFramesPerSecond;
            else if (frameRate > _maxFrameRate)
                frameRate = _maxFrameRate;
            _proposedFrameRate = frameRate;
            _frameRateChangeNeeded = true;
        }

        public void Start(ImportedGameProgramInfo importedGameProgramInfo, bool startFresh = false)
        {
            if (!_workerTask.IsCompleted)
                return;

            _stopRequested = false;
            for (var i = 0; i < _dynamicBitmapData.Length; i++)
                _dynamicBitmapData[i] = 0;

            var state = Tuple.Create(importedGameProgramInfo, startFresh);
            _workerTask = Task.Factory.StartNew(Run, state, TaskCreationOptions.LongRunning);
        }

        public void StartSnow()
        {
            if (!_workerTask.IsCompleted)
                return;
            _stopRequested = false;
            _workerTask = Task.Factory.StartNew(RunSnow, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            if (_workerTask.IsCompleted)
                return;
            _stopRequested = true;
            try
            {
                _workerTask.Wait(5000);
            }
            catch (AggregateException)
            {
            }
            SafeDispose(ref _dynamicBitmap);
        }

        public void SwitchToDarkerPalette()
        {
            _currentPalette = _darkerPalette;
        }

        public void SwitchToNormalPalette()
        {
            _currentPalette = _normalPalette;
        }

        public void ChangeCurrentKeyboardPlayerNo(int newPlayerNo)
        {
            newPlayerNo &= 3;
            if (_currentKeyboardPlayerNo == newPlayerNo)
                return;
            ClearPerPlayerButtonInput(_currentKeyboardPlayerNo);
            _currentKeyboardPlayerNo = newPlayerNo;
        }

        public void SwapJacks()
        {
            for (var pi = 0; pi < 4; pi++)
            {
                _playerJackMapping[pi] ^= 1;
            }
        }

        public void SwapLeftControllerPaddles()
        {
            var tmp0 = _paddleSwaps[0];
            _paddleSwaps[0] = _paddleSwaps[1];
            _paddleSwaps[1] = tmp0;
        }

        public void SwapRightControllerPaddles()
        {
            var tmp2 = _paddleSwaps[2];
            _paddleSwaps[2] = _paddleSwaps[3];
            _paddleSwaps[3] = tmp2;
        }

        public void RaiseMachineInput(MachineInput machineInput, bool down)
        {
            _inputState.RaiseInput(_currentKeyboardPlayerNo, machineInput, down);
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            _inputAdapters[_playerJackMapping[playerNo & 3]].JoystickChanged(playerNo, machineInput, down);
        }

        public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            _inputAdapters[_playerJackMapping[playerNo & 3]].ProLineJoystickChanged(playerNo, machineInput, down);
        }

        public void PaddleChanged(int playerNo, int valMax, int val)
        {
            var swappedPlayerNo = _paddleSwaps[playerNo & 3];
            _inputAdapters[_playerJackMapping[swappedPlayerNo]].PaddleChanged(swappedPlayerNo, valMax, val);
        }

        public void PaddleButtonChanged(int playerNo, bool down)
        {
            var swappedPlayerNo = _paddleSwaps[playerNo & 3];
            _inputAdapters[_playerJackMapping[swappedPlayerNo]].JoystickChanged(swappedPlayerNo, MachineInput.Fire, down);
        }

        public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
        {
            _inputAdapters[_playerJackMapping[playerNo & 3]].DrivingPaddleChanged(playerNo, machineInput);
        }

        #region ControlBase Overrides

        public override void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
            _inputAdapters[_playerJackMapping[_currentKeyboardPlayerNo]].KeyboardKeyPressed(_currentKeyboardPlayerNo, key, down);
        }

        public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
        {
            _inputAdapters[_playerJackMapping[_currentKeyboardPlayerNo]].MouseMoved(_currentKeyboardPlayerNo, x, y, dx, dy);
        }

        public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
        {
            _inputAdapters[_playerJackMapping[_currentKeyboardPlayerNo]].MouseButtonChanged(_currentKeyboardPlayerNo, x, y, down, IsInTouchMode);
        }

        public override void LocationChanged()
        {
            _dynamicBitmapRect = Struct.ToRectF(Location, Size);
            _inputAdapters[0].ScreenResized(Location, Size);
            _inputAdapters[1].ScreenResized(Location, Size);
        }

        public override void SizeChanged()
        {
            _dynamicBitmapRect = Struct.ToRectF(Location, Size);
            _inputAdapters[0].ScreenResized(Location, Size);
            _inputAdapters[1].ScreenResized(Location, Size);
        }

        public override void Update(TimerDevice td)
        {
            _inputAdapters[0].Update(td);
            _inputAdapters[1].Update(td);
        }

        public override void Render(GraphicsDevice gd)
        {
            lock (_dynamicBitmapLocker)
            {
                if (_dynamicBitmap == DynamicBitmapDefault)
                {
                    _dynamicBitmap = gd.CreateDynamicBitmap(_dynamicBitmapDataSize);
                    _dynamicBitmap.CopyFromMemory(_dynamicBitmapData);
                }
                else if (_dynamicBitmapDataUpdated)
                {
                    _dynamicBitmap.CopyFromMemory(_dynamicBitmapData);
                }
                _dynamicBitmapDataUpdated = false;
                _frameRenderer.OnDynamicBitmapDataDelivered();
            }

            gd.DrawBitmap(_dynamicBitmap, _dynamicBitmapRect, _dynamicBitmapInterpolationMode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            base.Dispose(disposing);
        }

        protected override void DisposeResources()
        {
            SafeDispose(ref _dynamicBitmap);
            base.DisposeResources();
        }

        #endregion

        #region Worker

        void Run(object state)
        {
            var args = (Tuple<ImportedGameProgramInfo, bool>)state;
            var importedGameProgramInfo = args.Item1;
            var startFresh = args.Item2;

            var machineStateInfo = MachineStateInfo.Default;
            var dsSvc = new DatastoreService();

            if (!startFresh)
            {
                var (result, newNachineStateInfo) = dsSvc.RestoreMachine(importedGameProgramInfo.GameProgramInfo);
                if (result.IsOk)
                {
                    machineStateInfo = newNachineStateInfo;
                }
            }

            if (machineStateInfo == MachineStateInfo.Default)
            {
                var mSvc = new MachineFactory();
                var (result, newMachineStateInfo) = mSvc.Create(importedGameProgramInfo);
                if (result.IsOk)
                {
                    machineStateInfo = newMachineStateInfo;
                }
                _calibrationNeeded = true;
            }

            if (machineStateInfo == MachineStateInfo.Default)
            {
                _stopRequested = true;
                return;
            }

            var machine = machineStateInfo.Machine;

            _soundOff = machineStateInfo.SoundOff;

            _maxFrameRate = machine.FrameHZ;
            CurrentFrameRate = _maxFrameRate;
            ProposeNewFrameRate(machineStateInfo.FramesPerSecond);

            _dynamicBitmapInterpolationMode = (D2DBitmapInterpolationMode)machineStateInfo.InterpolationMode;

            _currentKeyboardPlayerNo = machineStateInfo.CurrentPlayerNo - 1;
            _inputState = machine.InputState;

            _inputAdapters[0] = ToInputAdapter(machineStateInfo, 0);
            _inputAdapters[1] = ToInputAdapter(machineStateInfo, 1);

            var pi = 0;
            if (machine.InputState.LeftControllerJack == Controller.Paddles)
            {
                _playerJackMapping[pi++] = 0;
                _playerJackMapping[pi++] = 0;
            }
            else if (machine.InputState.LeftControllerJack != Controller.None)
            {
                _playerJackMapping[pi++] = 0;
            }

            if (machine.InputState.RightControllerJack == Controller.Paddles)
            {
                _playerJackMapping[pi++] = 1;
                _playerJackMapping[pi] = 1;
            }
            else if (machine.InputState.RightControllerJack != Controller.None)
            {
                _playerJackMapping[pi] = 1;
            }

            InitializePalettes(machine.Palette);
            _currentPalette = _normalPalette;

            var frameBuffer = machine.CreateFrameBuffer();
            _frameRenderer = ToFrameRenderer(machineStateInfo, frameBuffer);
            var audioBytes = new byte[frameBuffer.SoundBufferByteLength];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            long ticksPerFrame = 0;
            var audioDevice = AudioDeviceDefault;

            while (!_stopRequested)
            {
                var startTick = stopwatch.ElapsedTicks;
                var endTick = startTick + ticksPerFrame;

                if (_calibrationNeeded)
                {
                    _calibrationNeeded = false;
                    _calibrating = !_soundOff;
                }

                if (_calibrating && _frameDurationBucketSamples > 200)
                {
                    _calibrating = false;
                    var frameDurationSamplesNeeded = (int)(_frameDurationBucketSamples * 0.90);
                    var samplesCount = 0U;
                    for (var i = 0; i < _frameDurationBuckets.Length; i++)
                    {
                        samplesCount += _frameDurationBuckets[i];
                        if (i <= 0 || samplesCount < frameDurationSamplesNeeded)
                            continue;
                        _proposedFrameRate = 1000 / i;
                        break;
                    }
                    if (_proposedFrameRate > 60)
                        _proposedFrameRate = 60;
                    else if (_proposedFrameRate < 4)
                        _proposedFrameRate = 4;

                    if (CurrentFrameRate > _proposedFrameRate)
                        _frameRateChangeNeeded = true;
                }

                if (_frameRateChangeNeeded)
                {
                    _frameRateChangeNeeded = false;
                    if (audioDevice != AudioDeviceDefault)
                    {
                        audioDevice.Dispose();
                        audioDevice = AudioDeviceDefault;
                    }
                    if (_proposedFrameRate > CurrentFrameRate)
                        _calibrationNeeded = true;
                    CurrentFrameRate = _proposedFrameRate;
                    ticksPerFrame = Stopwatch.Frequency / CurrentFrameRate;
                }

                if (!_soundOff && audioDevice == AudioDeviceDefault)
                {
                    var soundFrequency = frameBuffer.SoundBufferByteLength * CurrentFrameRate;
                    audioDevice = new AudioDevice(soundFrequency, frameBuffer.SoundBufferByteLength, 8);
                }

                var buffersQueued = (audioDevice != AudioDeviceDefault) ? audioDevice.BuffersQueued : -1;
                long adjustment = 0;
                if (buffersQueued < 0 || _soundOff || _paused)
                    adjustment = 0;
                else if (buffersQueued < 2)
                    adjustment = -(ticksPerFrame >> 1);
                else if (buffersQueued > 4)
                    adjustment = ticksPerFrame >> 1;
                endTick += adjustment;

                if (!_paused)
                    machine.ComputeNextFrame(frameBuffer);

                if (!_soundOff && !_paused && audioDevice != AudioDeviceDefault)
                {
                    UpdateAudioBytes(frameBuffer, audioBytes);
                    audioDevice.SubmitBuffer(audioBytes);
                }

                lock (_dynamicBitmapLocker)
                {
                    _frameRenderer.UpdateDynamicBitmapData(_currentPalette);
                    _dynamicBitmapDataUpdated = true;
                }

                var elaspedTicks = stopwatch.ElapsedTicks;

                var frameMilliseconds = (uint)((elaspedTicks - startTick) / _stopwatchFrequencyInMilliseconds);
                if (!_soundOff && frameMilliseconds < _frameDurationBuckets.Length)
                {
                    _frameDurationBuckets[frameMilliseconds]++;
                    _frameDurationBucketSamples++;
                }

                FrameIdleTime = (float)(endTick - elaspedTicks) / ticksPerFrame;
                BuffersQueued = buffersQueued;

                while (stopwatch.ElapsedTicks < endTick)
                {
                    Task.Yield();
                }
            }

            if (audioDevice != AudioDeviceDefault)
                audioDevice.Dispose();

            machineStateInfo.CurrentPlayerNo = _currentKeyboardPlayerNo + 1;
            machineStateInfo.FramesPerSecond = CurrentFrameRate;
            machineStateInfo.InterpolationMode = (int)_dynamicBitmapInterpolationMode;
            machineStateInfo.SoundOff = _soundOff;

            dsSvc.PersistMachine(machineStateInfo);
            dsSvc.PersistScreenshot(machineStateInfo, _dynamicBitmapData);
        }

        void RunSnow()
        {
            var random = new Random();

            CurrentFrameRate = 60;
            const int soundBufferByteLength = 524;
            var soundFrequency = soundBufferByteLength * CurrentFrameRate;
            var ticksPerFrame = Stopwatch.Frequency / CurrentFrameRate;
            var audioDevice = _soundOff ? AudioDeviceDefault : new AudioDevice(soundFrequency, soundBufferByteLength, 8);
            var audioBytes = new byte[soundBufferByteLength];

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!_stopRequested)
            {
                var startTick = stopwatch.ElapsedTicks;
                var endTick = startTick + ticksPerFrame;

                var buffersQueued = (!_soundOff && audioDevice != AudioDeviceDefault) ? audioDevice.BuffersQueued : -1;
                long adjustment = 0;
                if (buffersQueued < 0)
                    adjustment = 0;
                else if (buffersQueued < 2)
                    adjustment = -(ticksPerFrame >> 1);
                else if (buffersQueued > 4)
                    adjustment = ticksPerFrame >> 1;
                endTick += adjustment;

                if (!_soundOff && audioDevice != AudioDeviceDefault)
                {
                    for (var i = 0; i < audioBytes.Length; i++)
                        audioBytes[i] = (byte) (random.Next(2) | 0x80);
                    audioDevice.SubmitBuffer(audioBytes);
                }

                lock (_dynamicBitmapLocker)
                {
                    for (var i = 0; i < _dynamicBitmapData.Length; i += 4)
                    {
                        var c = (byte)random.Next(0xc0);
                        _dynamicBitmapData[i] = c;
                        _dynamicBitmapData[i + 1] = c;
                        _dynamicBitmapData[i + 2] = c;
                    }
                    _dynamicBitmapDataUpdated = true;
                }

                while (stopwatch.ElapsedTicks < endTick && !_stopRequested)
                {
                    Task.Yield();
                }
            }

            if (audioDevice != AudioDeviceDefault)
                audioDevice.Dispose();
        }

        static IFrameRenderer ToFrameRenderer(MachineStateInfo machineStateInfo, FrameBuffer frameBuffer)
            => machineStateInfo.GameProgramInfo.MachineType switch
            {
                MachineType.A2600NTSC or MachineType.A2600PAL => new FrameRenderer160Blender(machineStateInfo.Machine.FirstScanline, frameBuffer, _dynamicBitmapData),
              //MachineType.A2600NTSC or MachineType.A2600PAL => new FrameRenderer160(machineStateInfo.Machine.FirstScanline, frameBuffer, _dynamicBitmapData);
                MachineType.A7800NTSC or MachineType.A7800PAL => new FrameRenderer320(machineStateInfo.Machine.FirstScanline, frameBuffer, _dynamicBitmapData),
                _ => throw new ArgumentException("Unknown MachineType", nameof(machineStateInfo))
            };

        IInputAdapter ToInputAdapter(MachineStateInfo machineState, int jackNo)
        {
            IInputAdapter inputAdapter = (ToController(machineState, jackNo)) switch
            {
                Controller.Joystick or Controller.BoosterGrip => new InputAdapterJoystick(_inputState, jackNo),
                Controller.ProLineJoystick => new InputAdapterProlineJoystick(_inputState, jackNo),
                Controller.Keypad          => new InputAdapterKeypad(_inputState, jackNo),
                Controller.Paddles         => new InputAdapterPaddle(_inputState, jackNo),
                Controller.Driving         => new InputAdapterDrivingPaddle(_inputState, jackNo),
                Controller.Lightgun        => new InputAdapterLightgun(_inputState, jackNo, machineState.Machine.FirstScanline, machineState.GameProgramInfo.MachineType),
                _                          => _defaultInputAdapter,
            };
            inputAdapter.ScreenResized(Location, Size);
            return inputAdapter;
        }

        static Controller ToController(MachineStateInfo machineState, int jackNo)
            => (jackNo & 1) == 0
                ? machineState.Machine.InputState.LeftControllerJack
                : machineState.Machine.InputState.RightControllerJack;

        void InitializePalettes(int[] sourcePalette)
        {
            for (var i = 0; i < _normalPalette.Length && i < sourcePalette.Length; i++)
            {
                _normalPalette[i] = (uint)sourcePalette[i];

                var color = sourcePalette[i];
                var r = (color >> 16) & 0xFF;
                var g = (color >> 8) & 0xFF;
                var b = (color >> 0) & 0xFF;
                r >>= 1;
                g >>= 1;
                b >>= 1;
                _darkerPalette[i] = (uint)(b | (g << 8) | (r << 16));
            }
        }

        static void UpdateAudioBytes(FrameBuffer frameBuffer, byte[] audioBytes)
        {
            for (var i = 0; i < frameBuffer.SoundBufferElementLength; i++)
            {
                var be = frameBuffer.SoundBuffer[i];
                var si = i << BufferElement.SHIFT;
                for (var j = 0; j < BufferElement.SIZE; j++)
                {
                    audioBytes[si | j] = (byte)(be[j] | 0x80);
                }
            }
        }

        void ClearPerPlayerButtonInput(int playerNo)
        {
            var buttonInput = new[]
            {
                MachineInput.Fire,
                MachineInput.Fire2,
                MachineInput.Up,
                MachineInput.Down,
                MachineInput.Left,
                MachineInput.Right
            };
            for (var i = 0; i < buttonInput.Length; i++)
                _inputState.RaiseInput(playerNo, buttonInput[i], false);
        }

        #endregion
    }
}
