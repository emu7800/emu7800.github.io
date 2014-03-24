/*
 * HostDirectX
 * 
 * A DirectX 9 based host.
 * 
 * Copyright © 2008 Mike Murphy
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using EMU7800.Core;

namespace EMU7800.Win.DirectX
{
    public enum Emu7800DirectXState
    {
        None,
        Initialize,
        Run,
        Acquire,
        Finalize
    };

    public class HostDirectX : HostBase
    {
        #region Fields

        readonly FontRenderer _fontRenderer = new FontRenderer();
        readonly DirectXInitParameters _directXInitParameters;
        readonly ILogger _logger;

        Emu7800DirectXState _state;
        int _dx, _dy;

        bool _showSnow;
        int _showSnowCounter;
        byte _fontColor;

        IDictionary<Key, MachineInput> _keyBindings;
        DirectInput _directInput;
        bool _joysticksSwapped, _leftPaddlesSwapped, _rightPaddlesSwapped;

        // for lightgun emulation
        bool _showMouseCursorForLightgunEmulation;
        int _mouseX, _mouseY;

        // for driving controller emulation
        bool _raiseEmulatedDrivingInput;

        // from app.config
        bool _noPauseOnLostFocus;
        int _initShowSnowCounter;

        const int FrameSamplesShift = 7;
        const int FrameSamplesMask = FrameSamplesShift - 1;
        readonly int[] _runMachineTicks = new int[1 << FrameSamplesShift];
        readonly int[] _frameDurationTicks = new int[1 << FrameSamplesShift];
        readonly Stopwatch _stopwatch = new Stopwatch();
        long _lastEndOfCycleTick, _nextEndOfFrameTick, _ticksPerFrame;
        int _usedAudioBuffers, _soundSampleRate;

        SynchronizationContext _synchronizationContext;
        Thread _workerThread;

        #endregion

        #region Constructors

        public HostDirectX(MachineBase m, ILogger logger) : this(m, logger, false)
        {
        }

        protected HostDirectX(MachineBase m, ILogger logger, bool fullScreen) : base(m, logger)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("EMU7800.Win.DirectX.HostDirectX only supports little endian architectures.");

            _logger = logger;
            ReadConfiguration();

            // 320x240 used to be specified as the TargetFrameWidth/Height to minimize pixel count in fullscreen mode.
            // However, a newer generation device was found that was automatically anti-aliasing in this resolution.
            // Hopefully, moving to 640x480 preserves graphic blockiness while minimizing pixel count.

            _directXInitParameters = new DirectXInitParameters
            {
                Adapter = 0,
                Icon = HostDirectXResources.EMUIcon,
                FullScreen = fullScreen,
                DeviceWindowWidth = 640,
                DeviceWindowHeight = 480,
                TargetFrameWidth = 640,
                TargetFrameHeight = 480,
                FrameBufferHandles = new SafeBufferHandles(M.CreateFrameBuffer(), M.Palette),
            };
        }

        #endregion

        public override void Run()
        {
            base.Run();

            // ensures a current synchronization context is established
            Application.DoEvents();

            try
            {
                DirectXNativeMethods.Initialize(_directXInitParameters);
            }
            catch (DllNotFoundException ex)
            {
                throw new ApplicationException("Unable to initialize EMU7800DirectX: " + ex.Message);
            }

            if (DirectXNativeMethods.IsDeviceErrored)
                throw new ApplicationException("Unable to initialize EMU7800DirectX: " + DirectXNativeMethods.HResult.ToString("X8"));

            _synchronizationContext = SynchronizationContext.Current;
            if (_synchronizationContext == null)
                throw new ApplicationException("No current SynchronizationContext");

            _state = Emu7800DirectXState.Initialize;

            PrepareFrameInitialize();

            _workerThread = new Thread(RunPresentationLoop) { IsBackground = false };
            _workerThread.Start();

            while (_workerThread.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }

            _workerThread.Join();

            PrepareFrameFinalize();
        }

        #region Presentation Loop

        void RunPresentationLoop()
        {
            _lastEndOfCycleTick = _stopwatch.ElapsedTicks;
            _nextEndOfFrameTick = _lastEndOfCycleTick + _ticksPerFrame;

            while (true)
            {
                switch (_state)
                {
                    case Emu7800DirectXState.Run:
                        PrepareFrameRun();
                        break;
                    case Emu7800DirectXState.Acquire:
                        _synchronizationContext.Send(state => PrepareFrameAcquire(), null);
                        break;
                    case Emu7800DirectXState.Finalize:
                        return;
                }

                DirectXNativeMethods.PresentFrame(_showSnow, _dx, _dy);

                // Adjust the frame duration to keep the audio backlog between 2-4 buffers (frames.)
                if (_usedAudioBuffers > 4)
                    _nextEndOfFrameTick += (_ticksPerFrame >> 1);
                else if (_usedAudioBuffers < 2)
                    _nextEndOfFrameTick -= (_ticksPerFrame >> 1);

                // Delay until the end of the next frame interval (or beyond.)
                long endOfCycleTick;
                while (true)
                {
                    endOfCycleTick = _stopwatch.ElapsedTicks;
                    if (endOfCycleTick > _nextEndOfFrameTick)
                        break;
                    // Some sort of more polite way to busy wait.
                    Thread.SpinWait(1000);
                }

                // Ensure _nextEndOfFrameTick is in the future, protecting against occasional but significant latency spikes.
                // e.g., on some hardware it seems the first PresentFrame() can incur a relatively large delay.
                // Without this remedy, the emulator would then run at excessive speed until caught up.
                // This behavior can be problematic when resuming game state.
                // Persistent slowness scenarios remain unaffected.
                while (true)
                {
                    _nextEndOfFrameTick += _ticksPerFrame;
                    if (endOfCycleTick < _nextEndOfFrameTick)
                        break;
                }

                var statIndex = M.FrameNumber & FrameSamplesMask;
                _frameDurationTicks[statIndex] = (int)(endOfCycleTick - _lastEndOfCycleTick);
                _lastEndOfCycleTick = endOfCycleTick;

                if (DirectXNativeMethods.IsDeviceLost || DirectXNativeMethods.IsDeviceStopped || DirectXNativeMethods.IsDeviceErrored)
                {
                    var message = string.Format("Emu7800DirectX: Stopped from HR={0:X8}: {1}", DirectXNativeMethods.HResult,
                       DirectXNativeMethods.IsDeviceLost ? " (device lost)" : (DirectXNativeMethods.IsDeviceStopped ? " (stop requested)" : string.Empty));
                    Log(message);
                    _state = Emu7800DirectXState.Finalize;
                    return;
                }
            }
        }

        void PrepareFrameRun()
        {
            var startOfCycleTick = _stopwatch.ElapsedTicks;

            if (!_directInput.Poll(false))
            {
                Log("HostDirectX: Lost input device(s): hr={0:x4}", _directInput.LastHResult);
                if (!_directXInitParameters.FullScreen && !_noPauseOnLostFocus)
                    RaiseInput(MachineInput.Pause, true);
                _state = _directXInitParameters.FullScreen ? Emu7800DirectXState.Finalize : Emu7800DirectXState.Acquire;
                return;
            }

            if (_raiseEmulatedDrivingInput && (M.FrameNumber & 3) == 0)
                RaiseEmulatedDrivingInput();

            if (Ended || M.MachineHalt)
            {
                _state = Emu7800DirectXState.Finalize;
                return;
            }

            _dx = LeftOffset;
            _dy = ClipStart;

            var fb = _directXInitParameters.FrameBufferHandles.FrameBuffer;

            if (_showSnowCounter > 0)
            {
                _showSnow = --_showSnowCounter > 0;
                var r = new Random();
                var bufferElement = new BufferElement();
                for (var i = 0; i < fb.SoundBufferElementLength; i++)
                {
                    for (var j = 0; j < BufferElement.SIZE; j++)
                        bufferElement[j] = (byte)r.Next(2);
                    fb.SoundBuffer[i] = bufferElement;
                }
            }
            else if (Paused)
            {
                for (var i = 0; i < fb.SoundBufferElementLength; i++)
                    fb.SoundBuffer[i].ClearAll();
            }
            else
            {
                M.ComputeNextFrame(fb);
                if (_showMouseCursorForLightgunEmulation)
                {
                    for (int i = _mouseY * fb.VisiblePitch + _mouseX - 2, j = 0; j < 5; i++, j++)
                        fb.VideoBuffer[i >> BufferElement.SHIFT][i] = _fontColor;
                }
            }

            if (!Muted)
            {
                if (_soundSampleRate > 0)
                {
                    try
                    {
                        _usedAudioBuffers = EnqueueAudio(fb, _soundSampleRate);
                    }
                    catch (ApplicationException ex)
                    {
                        Log("Audio deactivated: {0}", ex.Message);
                        _soundSampleRate = 0;
                        _usedAudioBuffers = 0;
                    }
                }
            }

            _fontColor++;
            if (PostedMsg.Length > 0)
                _fontRenderer.DrawText(fb, PostedMsg, LeftOffset + 2, ClipStart + 4, _fontColor, 0);

            var endOfCycleTick = _stopwatch.ElapsedTicks;

            var statIndex = M.FrameNumber & FrameSamplesMask;
            _runMachineTicks[statIndex] = (int)(endOfCycleTick - startOfCycleTick);
        }

        void PrepareFrameAcquire()
        {
            if (!_directInput.Poll(true))
                return;
            Log("HostDirectX: Reacquired input device(s)");
            _state = Emu7800DirectXState.Run;
        }

        void PrepareFrameInitialize()
        {
            _showMouseCursorForLightgunEmulation = (M.InputState.LeftControllerJack == Controller.Lightgun);

            var fb = _directXInitParameters.FrameBufferHandles.FrameBuffer;

            // center mouse dot
            _mouseX = fb.VisiblePitch >> 1;
            _mouseY = fb.Scanlines >> 1;

            _directInput = new DirectInput(DirectXNativeMethods.Statistics.DeviceWindow, _directXInitParameters.FullScreen);

            _keyBindings = UpdateKeyBindingsFromGlobalSettings(CreateDefaultKeyBindings());

            var stelladaptor0 = _directInput.IsStelladaptor(0);
            var stelladaptor1 = _directInput.IsStelladaptor(1);
            var daptor0 = _directInput.IsDaptor2(0);
            var daptor1 = _directInput.IsDaptor2(1);

            if (stelladaptor0 || stelladaptor1 || daptor0 || daptor1)
            {
                var msg = new System.Text.StringBuilder();
                if (stelladaptor0 || daptor0)
                {
                    msg.AppendFormat("P1:");
                    if (daptor0)
                    {
                        msg.AppendFormat("{0}-daptor", _directInput.GetDaptor2ModeText(_directInput.GetDaptor2Mode(0)));
                    }
                    else
                    {
                        msg.Append("2600-daptor");
                    }
                    SetDirectInputDaptorHandlersForPlayer(0);
                }
                if (stelladaptor1 || daptor1)
                {
                    if (msg.Length > 0)
                        msg.Append("  ");
                    msg.Append("P2:");
                    if (daptor1)
                    {
                        msg.AppendFormat("{0}-daptor", _directInput.GetDaptor2ModeText(_directInput.GetDaptor2Mode(1)));
                    }
                    else
                    {
                        msg.Append("2600-daptor");
                    }
                    SetDirectInputDaptorHandlersForPlayer(1);
                }
                Log(msg.ToString());
                PostedMsg = msg.ToString();
            }
            else
            {
                SetDirectInputHandlers();
            }

            _ticksPerFrame = Stopwatch.Frequency / EffectiveFPS;
            _soundSampleRate = M.SoundSampleFrequency * EffectiveFPS / M.FrameHZ;
            _usedAudioBuffers = 0;

            var statistics = DirectXNativeMethods.Statistics;

            var message = string.Format("HostDirectX Info: Frame=({0},{1}), VideoHz={2}, AudioHz={3}",
                statistics.FrameWidth, statistics.FrameHeight, statistics.Hz, _soundSampleRate);
            Log(message);

            _showSnowCounter = _initShowSnowCounter;

            for (var i = 0; i < fb.SoundBufferElementLength; i++)
                fb.SoundBuffer[i].ClearAll();

            _stopwatch.Start();

            _state = Emu7800DirectXState.Run;
        }

        void PrepareFrameFinalize()
        {
            if (_directInput != null)
                _directInput.Dispose();
            if (_directXInitParameters != null && _directXInitParameters.FrameBufferHandles != null)
                _directXInitParameters.FrameBufferHandles.Dispose();
            DirectXNativeMethods.Shutdown();
            CloseAudio();
        }

        #endregion

        #region Input Handling

        void SetDirectInputHandlers()
        {
            switch (M.InputState.LeftControllerJack)
            {
                case Controller.Joystick:
                case Controller.ProLineJoystick:
                case Controller.BoosterGrip:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.JoystickChanged = OnJoystickChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Lightgun:
                    _raiseEmulatedDrivingInput = false;
                    if (_directXInitParameters.FullScreen)
                    {
                        _directInput.MouseChanged = OnMouseLightgunChanged;
                        _directInput.MouseButtonChanged = OnMouseLightgunButtonChanged;
                        _directInput.KeyboardChanged = OnKeyboardChangedForFullscreenLightgun;
                    }
                    else
                    {
                        _directInput.KeyboardChanged = OnKeyboardChangedForNonFullscreenLightgun;
                    }
                    break;
                case Controller.Paddles:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.MousePaddleChanged = OnMousePaddleChanged;
                    _directInput.MouseButtonChanged = OnMouseButtonChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Driving:
                    _raiseEmulatedDrivingInput = true;
                    _directInput.JoystickChanged = OnJoystickChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Keypad:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
            }
        }

        void SetDirectInputDaptorHandlersForPlayer(int deviceno)
        {
            var controller = (deviceno == 0) ? M.InputState.LeftControllerJack : M.InputState.RightControllerJack;

            var daptor2Mode = _directInput.GetDaptor2Mode(deviceno);
            if (daptor2Mode == 0)
            {
                if (controller == Controller.ProLineJoystick)
                {
                    controller = Controller.Joystick;
                    if (deviceno == 0)
                        M.InputState.LeftControllerJack = controller;
                    else
                        M.InputState.RightControllerJack = controller;
                    Log("Changing P{0} controller from ProLineJoystick to Joystick to match recognized 2600 adaptor mode.", deviceno + 1);
                }
            }

            Log("2600daptor P{0} mode: {1}", deviceno + 1, _directInput.GetDaptor2ModeText(daptor2Mode));

            switch (controller)
            {
                case Controller.Joystick:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.JoystickChanged = OnJoystickChangedForDaptor2AndJoystick;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.ProLineJoystick:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.JoystickChanged = OnJoystickChangedForDaptor2AndProlineJoystick;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.BoosterGrip:
                    _raiseEmulatedDrivingInput = false;
                    if (daptor2Mode == 1)
                        _directInput.JoystickChanged = OnJoystickChangedForDaptor2AndBoosterGrip;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Lightgun:
                    _raiseEmulatedDrivingInput = false;
                    if (_directXInitParameters.FullScreen)
                    {
                        _directInput.MouseChanged = OnMouseLightgunChanged;
                        _directInput.MouseButtonChanged = OnMouseLightgunButtonChanged;
                        _directInput.KeyboardChanged = OnKeyboardChangedForFullscreenLightgun;
                    }
                    else
                    {
                        _directInput.KeyboardChanged = OnKeyboardChangedForNonFullscreenLightgun;
                    }
                    break;
                case Controller.Paddles:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.StelladaptorPaddleChanged = OnStelladaptorPaddleChanged;
                    _directInput.StelladaptorPaddleButtonChanged = OnStelladaptorPaddleButtonChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Driving:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.StelladaptorDrivingChanged = OnStelladaptorDrivingChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
                case Controller.Keypad:
                    _raiseEmulatedDrivingInput = false;
                    _directInput.Daptor2KeypadChanged += OnDaptorKeypadChanged;
                    _directInput.KeyboardChanged = OnKeyboardChanged;
                    break;
            }
        }

        void OnStelladaptorDrivingChanged(int deviceno, int position, bool fire)
        {
            deviceno ^= (_joysticksSwapped ? 1 : 0);
            switch (position)
            {
                case 0:
                    RaiseInput(deviceno, MachineInput.Driving0, true);
                    break;
                case 1:
                    RaiseInput(deviceno, MachineInput.Driving1, true);
                    break;
                case 2:
                    RaiseInput(deviceno, MachineInput.Driving2, true);
                    break;
                case 3:
                    RaiseInput(deviceno, MachineInput.Driving3, true);
                    break;
            }
            RaiseInput(deviceno, MachineInput.Fire, fire);
        }

        void OnJoystickChanged(int deviceno, bool left, bool right, bool up, bool down, bool fire, bool fire2, bool fire3)
        {
            deviceno ^= (_joysticksSwapped ? 1 : 0);
            RaiseInput(deviceno, MachineInput.Left, left);
            RaiseInput(deviceno, MachineInput.Right, right);
            RaiseInput(deviceno, MachineInput.Up, up);
            RaiseInput(deviceno, MachineInput.Down, down);
            RaiseInput(deviceno, MachineInput.Fire, fire);
            RaiseInput(deviceno, MachineInput.Fire2, fire2);
        }

        void OnJoystickChangedForDaptor2AndJoystick(int deviceno, bool left, bool right, bool up, bool down, bool leftFire, bool rightFire, bool fire2600)
        {
            OnJoystickChanged(deviceno, left, right, up, down, rightFire || leftFire || fire2600, false, false);
        }

        void OnJoystickChangedForDaptor2AndProlineJoystick(int deviceno, bool left, bool right, bool up, bool down, bool leftFire, bool rightFire, bool fire2600)
        {
            OnJoystickChanged(deviceno, left, right, up, down, rightFire, leftFire, false);
        }

        void OnJoystickChangedForDaptor2AndBoosterGrip(int deviceno, bool left, bool right, bool up, bool down, bool topFire, bool triggerFire, bool fire2600)
        {
            OnJoystickChanged(deviceno, left, right, up, down, topFire || fire2600, triggerFire, false);
        }

        void OnMouseLightgunChanged(int dx, int dy)
        {
            var fb = _directXInitParameters.FrameBufferHandles.FrameBuffer;

            dx >>= 1;
            dy >>= 1;
            if (fb.VisiblePitch == 160)
                dx >>= 1;

            _mouseX += dx;
            _mouseY += dy;
            if (_mouseX < 0)
            {
                _mouseX = 0;
            }
            else if (_mouseX >= fb.VisiblePitch)
            {
                _mouseX = fb.VisiblePitch - 1;
            }
            if (_mouseY < 0)
            {
                _mouseY = 0;
            }
            else if (_mouseY >= fb.Scanlines)
            {
                _mouseY = fb.Scanlines - 1;
            }
        }

        void OnMouseLightgunButtonChanged(bool fire)
        {
            RaiseLightGunInput(_mouseY, _mouseX);
            RaiseInput(MachineInput.Fire, fire);
        }

        void OnKeyboardChangedForFullscreenLightgun(Key key, bool down)
        {
            MachineInput hostInput;
            if (!_keyBindings.TryGetValue(key, out hostInput))
                return;

            RaiseInput(hostInput, down);
            RaiseLightGunInput(_mouseY, _mouseX);
        }


        void OnKeyboardChangedForNonFullscreenLightgun(Key key, bool down)
        {
            MachineInput hostInput;
            if (!_keyBindings.TryGetValue(key, out hostInput))
                return;

            switch (hostInput)
            {
                case MachineInput.Left:
                    _mouseX -= 2;
                    break;
                case MachineInput.Right:
                    _mouseX += 2;
                    break;
                case MachineInput.Up:
                    _mouseY -= 2;
                    break;
                case MachineInput.Down:
                    _mouseY += 2;
                    break;
                default:
                    RaiseInput(hostInput, down);
                    break;
            }

            RaiseLightGunInput(_mouseY, _mouseX);
        }

        void OnStelladaptorPaddleChanged(int paddleno, int val)
        {
            paddleno ^= (_joysticksSwapped ? 2 : 0);
            if (paddleno <= 1)
            {
                paddleno ^= (_leftPaddlesSwapped ? 1 : 0);
            }
            else
            {
                paddleno ^= (_rightPaddlesSwapped ? 1 : 0);
            }
            RaisePaddleInput(paddleno, DirectInput.StelladaptorPaddleRange, val);
        }

        void OnStelladaptorPaddleButtonChanged(int paddleno, bool fire)
        {
            paddleno ^= (_joysticksSwapped ? 2 : 0);
            if (paddleno <= 1)
            {
                paddleno ^= (_leftPaddlesSwapped ? 1 : 0);
            }
            else
            {
                paddleno ^= (_rightPaddlesSwapped ? 1 : 0);
            }
            RaiseInput(paddleno, MachineInput.Fire, fire);
        }

        void OnDaptorKeypadChanged(int deviceno, MachineInput key, bool down)
        {
            deviceno ^= (_joysticksSwapped ? 1 : 0);
            RaiseInput(deviceno, key, down);
        }

        void OnMousePaddleChanged(int val)
        {
            RaisePaddleInput(_directInput.MousePaddleRange, val);
        }

        void OnMouseButtonChanged(bool fire)
        {
            RaiseInput(MachineInput.Fire, fire);
        }

        void OnKeyboardChanged(Key key, bool down)
        {
            MachineInput hostInput;
            if (!_keyBindings.TryGetValue(key, out hostInput))
                return;

            switch (hostInput)
            {
                case MachineInput.ShowFrameStats:
                    if (!down || !PostedMsg.Length.Equals(0))
                        break;
                    var rmTicks = 0.0;
                    var fdTicks = 0.0;
                    var divisor = 0;
                    var factor = 1000.0 / Stopwatch.Frequency;
                    for (var i = 0; i < (1 << FrameSamplesShift); i++)
                    {
                        rmTicks += _runMachineTicks[i];
                        fdTicks += _frameDurationTicks[i];
                        if (_frameDurationTicks[i] > 0)
                            divisor++;
                    }
                    if (divisor > 0)
                    {
                        rmTicks /= divisor;
                        fdTicks /= divisor;
                    }
                    rmTicks *= factor;
                    fdTicks *= factor;
                    var fps = 1000.0 / fdTicks;
                    PostedMsg = string.Format("{0:0.0} {1:0.0} {2:0.0} {3:0.0} {4:0}", rmTicks, fdTicks - rmTicks, fdTicks, fps, _usedAudioBuffers);
                    break;
                case MachineInput.LeftPaddleSwap:
                    if (!down)
                        break;
                    _leftPaddlesSwapped = !_leftPaddlesSwapped;
                    PostedMsg = string.Format("Left Paddles {0}wapped", _leftPaddlesSwapped ? "S" : "Uns");
                    break;
                case MachineInput.GameControllerSwap:
                    if (!down)
                        break;
                    _joysticksSwapped = !_joysticksSwapped;
                    PostedMsg = string.Format("Game Controllers {0}wapped", _joysticksSwapped ? "S" : "Uns");
                    break;
                case MachineInput.RightPaddleSwap:
                    if (!down)
                        break;
                    _rightPaddlesSwapped = !_rightPaddlesSwapped;
                    PostedMsg = string.Format("Right Paddles {0}wapped", _rightPaddlesSwapped ? "S" : "Uns");
                    break;
                case MachineInput.TakeScreenshot:
                    if (down) TakeScreenshot();
                    break;
                default:
                    RaiseInput(hostInput, down);
                    break;
            }
        }

        #endregion

        void TakeScreenshot()
        {
            var fb = _directXInitParameters.FrameBufferHandles.FrameBuffer;

            var palette = M.Palette;
            var fb2 = new int[320 * fb.Scanlines];
            if (fb.VisiblePitch.Equals(160))
            {
                for (int i = 0, di = 0; i < fb.VideoBufferByteLength; i++, di += 2)
                {
                    fb2[di] = fb2[di + 1] = palette[fb.VideoBuffer[i >> BufferElement.SHIFT][i]];
                }
            }
            else
            {
                for (var i = 0; i < fb.VideoBufferByteLength; i++)
                {
                    fb2[i] = palette[fb.VideoBuffer[i >> BufferElement.SHIFT][i]];
                }
            }

            var root = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var fileName = string.Format("emu7800 screenshot {0:yyyy} {0:MM}-{0:dd} {0:HH}{0:mm}{0:ss}.bmp", DateTime.Now);
            var fullName = Path.Combine(root, fileName);
            var h = GCHandle.Alloc(fb2, GCHandleType.Pinned);
            try
            {
                using (var bm = new Bitmap(320, fb.Scanlines, 4 * 320, PixelFormat.Format32bppRgb, h.AddrOfPinnedObject()))
                {
                    bm.Save(fullName);
                }
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                Log("Error while taking screenshot: {0}", ex);
                PostedMsg = "Screenshot failed";
                return;
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }

            Log("Screenshot taken: {0}", fullName);
            PostedMsg = "Screenshot taken";
        }

        #region Configuration Helpers

        void ReadConfiguration()
        {
            NameValueCollection settings;
            try
            {
                settings = ConfigurationManager.GetSection("EMU7800/Settings") as NameValueCollection;
            }
            catch (ConfigurationErrorsException ex)
            {
                Log(ex.ToString());
                return;
            }
            _noPauseOnLostFocus = IsSettingTrue(settings, @"EMU7800.Host.DirectX.HostDirectX.NoPauseOnLostFocus", false);
            _initShowSnowCounter = GetSettingInt(settings, @"EMU7800.Host.DirectX.HostDirectX.InitShowSnowCounter", 30);
        }

        static bool IsSettingTrue(NameValueCollection settings, string settingName, bool defaultValue)
        {
            return (settings != null) ? settings[settingName].Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase) : defaultValue;
        }

        static int GetSettingInt(NameValueCollection settings, string settingName, int defaultValue)
        {
            var intVal = defaultValue;
            if (settings != null)
                Int32.TryParse(settings[settingName], out intVal);
            return intVal;
        }

        #endregion

        #region Helpers

        void Log(string format, params object[] args)
        {
            _logger.WriteLine(format, args);
        }

        #endregion
    }
}