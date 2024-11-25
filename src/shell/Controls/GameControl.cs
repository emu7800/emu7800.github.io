﻿// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services;
using EMU7800.Services.Dto;
using EMU7800.Win32.Interop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell;

public sealed class GameControl : ControlBase
{
    #region Fields

    ReadOnlyMemory<uint> _normalPalette  = ReadOnlyMemory<uint>.Empty;
    ReadOnlyMemory<uint> _darkerPalette  = ReadOnlyMemory<uint>.Empty;
    ReadOnlyMemory<uint> _currentPalette = ReadOnlyMemory<uint>.Empty;

    static readonly System.Threading.Lock _dynamicBitmapLocker = new();
    static readonly Memory<byte> _dynamicBitmapData = new(new byte[4 * 320 * 230]);
    static readonly D2D_SIZE_U _dynamicBitmapDataSize = new(320, 230);
    IFrameRenderer _frameRenderer = new FrameRendererDefault();
    D2DBitmapInterpolationMode _dynamicBitmapInterpolationMode = D2DBitmapInterpolationMode.NearestNeighbor;
    DynamicBitmap _dynamicBitmap = DynamicBitmap.Default;
    D2D_RECT_F _dynamicBitmapRect;
    bool _dynamicBitmapDataUpdated;

    static readonly InputState _defaultInputState = new();
    InputState _inputState = _defaultInputState;

    static readonly InputAdapterNull _defaultInputAdapter = new();
    readonly IInputAdapter[] _inputAdapters = [_defaultInputAdapter, _defaultInputAdapter];
    readonly int[] _jackSwaps = [0, 1];
    readonly int[] _paddleSwaps = [0, 1, 0, 1];
    int _currentKeyboardPlayerNo;

    Task _workerTask = Task.CompletedTask;
    bool _stopRequested, _paused, _soundOff;

    bool _calibrationNeeded, _calibrating, _frameRateChangeNeeded;
    readonly uint[] _frameDurationBuckets = new uint[0x100];
    readonly long _stopwatchFrequencyInMilliseconds = Stopwatch.Frequency / 1000;
    uint _frameDurationBucketSamples;
    int _proposedFrameRate, _maxFrameRate = 60;

    #endregion

    public bool IsGameBWConsoleSwitchSet => _inputState.IsGameBWConsoleSwitchSet;

    public bool IsLeftDifficultyAConsoleSwitchSet => _inputState.IsLeftDifficultyAConsoleSwitchSet;

    public bool IsRightDifficultyAConsoleSwitchSet => _inputState.IsRightDifficultyAConsoleSwitchSet;

    public bool IsPaused
    {
        get => _paused;
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
        get => !_soundOff;
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
        get => _dynamicBitmapInterpolationMode == D2DBitmapInterpolationMode.Linear;
        set => _dynamicBitmapInterpolationMode = value
            ? D2DBitmapInterpolationMode.Linear
            : D2DBitmapInterpolationMode.NearestNeighbor;
    }

    public int CurrentFrameRate { get; private set; }

    public float FrameIdleTime { get; private set; }

    public int BuffersQueued { get; private set; }

    public static int MinFramesPerSecond => 4;

    public int MaxFramesPerSecond => _maxFrameRate;

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
        _dynamicBitmapData.Span.Clear();

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

    public bool SwapJacks()
    {
        (_jackSwaps[0], _jackSwaps[1]) = (_jackSwaps[1], _jackSwaps[0]);
        return _jackSwaps[0] == 1;
    }

    public bool SwapLeftControllerPaddles()
    {
        (_paddleSwaps[0], _paddleSwaps[1]) = (_paddleSwaps[1], _paddleSwaps[0]);
        return _paddleSwaps[0] == 1;
    }

    public bool SwapRightControllerPaddles()
    {
        (_paddleSwaps[2], _paddleSwaps[3]) = (_paddleSwaps[3], _paddleSwaps[2]);
        return _paddleSwaps[2] == 3;
    }

    public void RaiseMachineInput(MachineInput machineInput, bool down)
    {
        _inputState.RaiseInput(_currentKeyboardPlayerNo, machineInput, down);
    }

    #region ControlBase Overrides

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        _inputAdapters[_jackSwaps[_currentKeyboardPlayerNo & 1]].KeyboardKeyPressed(_currentKeyboardPlayerNo, key, down);
    }

    public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
        _inputAdapters[_jackSwaps[_currentKeyboardPlayerNo & 1]].MouseMoved(_currentKeyboardPlayerNo, x, y, dx, dy);
    }

    public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        _inputAdapters[_jackSwaps[_currentKeyboardPlayerNo & 1]].MouseButtonChanged(_currentKeyboardPlayerNo, x, y, down, IsInTouchMode);
    }

    public override void ControllerButtonChanged(int controllerNo, MachineInput machineInput, bool down)
    {
        controllerNo &= 1;
        var jackNo = _jackSwaps[controllerNo];
        var playerNo = jackNo;
        _inputAdapters[jackNo].JoystickChanged(playerNo, machineInput, down);
        _inputAdapters[jackNo].ProLineJoystickChanged(playerNo, machineInput, down);
    }

    public override void PaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
    {
        controllerNo &= 1;
        var jackNo = _jackSwaps[controllerNo];
        var playerNo = (jackNo << 1) | _paddleSwaps[(jackNo << 1) | paddleNo & 1];
        _inputAdapters[jackNo].PaddleChanged(playerNo, ohms);
    }

    public override void PaddleButtonChanged(int controllerNo, int paddleNo, bool down)
    {
        controllerNo &= 1;
        var jackNo = _jackSwaps[controllerNo];
        var playerNo = (jackNo << 1) | _paddleSwaps[(jackNo << 1) | paddleNo & 1];
        _inputAdapters[jackNo].PaddleButtonChanged(playerNo, down);
    }

    public override void DrivingPositionChanged(int controllerNo, MachineInput machineInput)
    {
        controllerNo &= 1;
        var jackNo = _jackSwaps[controllerNo];
        var playerNo = jackNo;
        _inputAdapters[jackNo].DrivingPaddleChanged(playerNo, machineInput);
    }

    public override void LocationChanged()
    {
        _dynamicBitmapRect = new(Location, Size);
        _inputAdapters[0].ScreenResized(Location, Size);
        _inputAdapters[1].ScreenResized(Location, Size);
    }

    public override void SizeChanged()
    {
        _dynamicBitmapRect = new(Location, Size);
        _inputAdapters[0].ScreenResized(Location, Size);
        _inputAdapters[1].ScreenResized(Location, Size);
    }

    public override void Update(TimerDevice td)
    {
        _inputAdapters[0].Update(td);
        _inputAdapters[1].Update(td);
    }

    public override void Render()
    {
        lock (_dynamicBitmapLocker)
        {
            if (_dynamicBitmap == DynamicBitmap.Default)
            {
                _dynamicBitmap = new DynamicBitmap(_dynamicBitmapDataSize);
                _dynamicBitmap.Load(_dynamicBitmapData.Span);
            }
            else if (_dynamicBitmapDataUpdated)
            {
                _dynamicBitmap.Load(_dynamicBitmapData.Span);
            }
            _dynamicBitmapDataUpdated = false;
        }

        GraphicsDevice.Draw(_dynamicBitmap, _dynamicBitmapRect, _dynamicBitmapInterpolationMode);
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

    void Run(object? state)
    {
        if (state is not Tuple<ImportedGameProgramInfo, bool> typedState)
        {
            return;
        }

        var (importedGameProgramInfo, startFresh) = typedState;

        var machineStateInfo = MachineStateInfo.Default;

        if (!startFresh)
        {
            machineStateInfo = DatastoreService.RestoreMachine(importedGameProgramInfo.GameProgramInfo);
        }

        if (machineStateInfo == MachineStateInfo.Default)
        {
            machineStateInfo = MachineFactory.Create(importedGameProgramInfo);
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

        _jackSwaps[0] = 0;
        _jackSwaps[1] = 1;
        _paddleSwaps[0] = 0;
        _paddleSwaps[1] = 1;
        _paddleSwaps[2] = 0;
        _paddleSwaps[3] = 1;

        InitializePalettes(machine.Palette);
        _currentPalette = _normalPalette;

        var frameBuffer = machine.CreateFrameBuffer();

        _frameRenderer = ToFrameRenderer(machineStateInfo);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        long ticksPerFrame = 0;

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

                _proposedFrameRate = _proposedFrameRate switch
                {
                    > 60 => 60,
                    < 4 => 4,
                    _ => _proposedFrameRate
                };

                if (CurrentFrameRate > _proposedFrameRate)
                    _frameRateChangeNeeded = true;
            }

            if (_frameRateChangeNeeded)
            {
                _frameRateChangeNeeded = false;
                AudioDevice.Close();
                if (_proposedFrameRate > CurrentFrameRate)
                    _calibrationNeeded = true;
                CurrentFrameRate = _proposedFrameRate;
                ticksPerFrame = Stopwatch.Frequency / CurrentFrameRate;
            }

            if (!_soundOff && AudioDevice.IsClosed)
            {
                var soundFrequency = frameBuffer.SoundBuffer.Length * CurrentFrameRate;
                AudioDevice.Configure(soundFrequency, frameBuffer.SoundBuffer.Length, 8);
            }

            var buffersQueued = AudioDevice.CountBuffersQueued();
            var adjustment = buffersQueued < 0 || _soundOff || _paused
                ? 0
                : buffersQueued switch
                {
                    < 2 => -(ticksPerFrame >> 1),
                    > 4 => ticksPerFrame >> 1,
                    _ => 0
                };
            endTick += adjustment;

            if (!_paused)
                machine.ComputeNextFrame(frameBuffer);

            if (!_soundOff && !_paused)
            {
                AudioDevice.SubmitBuffer(frameBuffer.SoundBuffer.Span);
            }

            lock (_dynamicBitmapLocker)
            {
                _frameRenderer.UpdateDynamicBitmapData(_currentPalette.Span, frameBuffer.VideoBuffer.Span, _dynamicBitmapData.Span);
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

        AudioDevice.Close();

        machineStateInfo = machineStateInfo with
        {
            CurrentPlayerNo   = _currentKeyboardPlayerNo + 1,
            FramesPerSecond   = CurrentFrameRate,
            InterpolationMode = (int)_dynamicBitmapInterpolationMode,
            SoundOff          = _soundOff
        };

        DatastoreService.PersistMachine(machineStateInfo, _dynamicBitmapData);
    }

    void RunSnow()
    {
        var random = new Random();

        CurrentFrameRate = 60;
        var soundBuffer = new Memory<byte>(new byte[524]);
        var soundFrequency = soundBuffer.Length * CurrentFrameRate;
        var ticksPerFrame = Stopwatch.Frequency / CurrentFrameRate;
        AudioDevice.Configure(soundFrequency, soundBuffer.Length, 8);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        while (!_stopRequested)
        {
            var startTick = stopwatch.ElapsedTicks;
            var endTick = startTick + ticksPerFrame;

            var buffersQueued = AudioDevice.CountBuffersQueued();
            var adjustment = buffersQueued switch
            {
                < 2 => -(ticksPerFrame >> 1),
                > 4 => ticksPerFrame >> 1,
                _  => 0
            };
            endTick += adjustment;

            if (!_soundOff)
            {
                for (var i = 0; i < soundBuffer.Length; i++)
                    soundBuffer.Span[i] = (byte)random.Next(2);
                AudioDevice.SubmitBuffer(soundBuffer.Span);
            }

            lock (_dynamicBitmapLocker)
            {
                var dbdSpan = _dynamicBitmapData.Span;
                for (var i = 0; i < _dynamicBitmapData.Length; i += 4)
                {
                    var c = (byte)random.Next(0xc0);
                    dbdSpan[i] = c;
                    dbdSpan[i + 1] = c;
                    dbdSpan[i + 2] = c;
                }
                _dynamicBitmapDataUpdated = true;
            }

            while (stopwatch.ElapsedTicks < endTick && !_stopRequested)
            {
                Task.Yield();
            }
        }

        AudioDevice.Close();
    }

    static IFrameRenderer ToFrameRenderer(MachineStateInfo machineStateInfo)
        => MachineTypeUtil.Is2600(machineStateInfo.GameProgramInfo.MachineType) ? new FrameRenderer160(machineStateInfo.Machine.FirstScanline) :
           MachineTypeUtil.Is7800(machineStateInfo.GameProgramInfo.MachineType) ? new FrameRenderer320(machineStateInfo.Machine.FirstScanline) :
           throw new ArgumentException("Unknown MachineType", nameof(machineStateInfo));

    IInputAdapter ToInputAdapter(MachineStateInfo machineState, int jackNo)
    {
        var controller = (jackNo & 1) == 0 ? machineState.Machine.InputState.LeftControllerJack : machineState.Machine.InputState.RightControllerJack;
        IInputAdapter inputAdapter = controller switch
        {
            Controller.Joystick or Controller.BoosterGrip => new InputAdapterJoystick(_inputState),
            Controller.ProLineJoystick => new InputAdapterProlineJoystick(_inputState),
            Controller.Keypad   => new InputAdapterKeypad(_inputState),
            Controller.Paddles  => new InputAdapterPaddle(_inputState),
            Controller.Driving  => new InputAdapterDrivingPaddle(_inputState),
            Controller.Lightgun => new InputAdapterLightgun(_inputState, machineState.Machine.FirstScanline, machineState.GameProgramInfo.MachineType),
            _                   => _defaultInputAdapter
        };
        inputAdapter.ScreenResized(Location, Size);
        return inputAdapter;
    }

    void InitializePalettes(ReadOnlyMemory<uint> sourcePalette)
    {
        _normalPalette = sourcePalette;

        var normalPaletteSpan = _normalPalette.Span;
        var darkerPalette = new uint[_normalPalette.Length];

        for (var i = 0; i < _normalPalette.Length; i++)
        {
            var color = normalPaletteSpan[i];
            var r = (color >> 16) & 0xFF;
            var g = (color >>  8) & 0xFF;
            var b = (color >>  0) & 0xFF;
            r >>= 1;
            g >>= 1;
            b >>= 1;
            darkerPalette[i] = b | (g << 8) | (r << 16);
        }

        _darkerPalette = new ReadOnlyMemory<uint>(darkerPalette);
    }

    void ClearPerPlayerButtonInput(int playerNo)
    {
        _inputState.RaiseInput(playerNo, MachineInput.Fire,  false);
        _inputState.RaiseInput(playerNo, MachineInput.Fire2, false);
        _inputState.RaiseInput(playerNo, MachineInput.Up,    false);
        _inputState.RaiseInput(playerNo, MachineInput.Down,  false);
        _inputState.RaiseInput(playerNo, MachineInput.Left,  false);
        _inputState.RaiseInput(playerNo, MachineInput.Right, false);
    }

    #endregion
}