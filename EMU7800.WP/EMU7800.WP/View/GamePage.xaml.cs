using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using EMU7800.Core;
using EMU7800.WP.Model;
using EMU7800.WP.ViewModel;

namespace EMU7800.WP.View
{
    public sealed partial class GamePage
    {
        #region Fields

        readonly GameTimer _frameRateTimer = new GameTimer();
        readonly Stopwatch _stopwatch;

        bool _showHud, _paused, _soundOff, _powerOn, _calibrationNeeded, _soundConfigurationNeeded;

        TimeSpan _speedupFrameInterval, _normalFrameInterval, _slowdownFrameInterval;

        MachineBase _machine;
        FrameBuffer _frameBuffer;
        FrameRenderer _frameRenderer;

        UpdateWorker _updateWorker;

        readonly Random _snowGenerator = new Random();
        byte[] _audioFrame;
        DynamicSoundEffectInstance _dynamicSound;

        static App CurrentApp { get { return (App)Application.Current; } }

        readonly GameProgramInfoRepository _repository;
        readonly GameProgramSelectViewModel _viewModelRepository;

        GameProgramInfo _gameProgramInfo;

        int _framesPerSecond;

        SpriteBatch _spriteBatch;
        UIElementRenderer _elementRenderer;

        InputHandler _inputHandler;
        int _persistedCurrentPlayerNo;

#if PROFILE
        readonly DurationProfiler _profilerUpdateDuration;
        readonly DurationProfiler _profilerDrawDuration;
        readonly RateProfiler _profilerDrawRate;
#endif

        #endregion

        #region Constructors

        public GamePage()
        {
            InitializeComponent();

            _stopwatch = Stopwatch.StartNew();
#if PROFILE
            _profilerUpdateDuration = new DurationProfiler(_stopwatch);
            _profilerDrawDuration = new DurationProfiler(_stopwatch);
            _profilerDrawRate = new RateProfiler(_stopwatch);
#endif
            _repository = CurrentApp.Services.GetService<GameProgramInfoRepository>();
            _viewModelRepository = CurrentApp.Services.GetService<GameProgramSelectViewModel>();

            togglebuttonPower.IsChecked = true;
            togglebuttonPause.IsChecked = false;

            LayoutUpdated += GamePage_LayoutUpdated;
            BackKeyPress += GamePage_BackKeyPress;

            togglebuttonPower.Checked += togglebuttonPower_Checked;
            togglebuttonPower.Unchecked += togglebuttonPower_Unchecked;
            buttonColorBW.Tap += buttonColorBW_Tap;
            togglebuttonLDiff.Tap += togglebuttonLDiff_Tap;
            togglebuttonRDiff.Tap += togglebuttonRDiff_Tap;
            buttonSelect.Tap += buttonSelect_Tap;
            buttonReset.Tap += buttonReset_Tap;
            sliderFps.ValueChanged += sliderFps_ValueChanged;
            sliderFps.ManipulationCompleted += sliderFps_ManipulationCompleted;
            togglebuttonSound.Checked += togglebuttonSound_Checked;
            togglebuttonSound.Unchecked += togglebuttonSound_Unchecked;
            togglebuttonPause.Checked += togglebuttonPause_Checked;
            togglebuttonPause.Unchecked += togglebuttonPause_Unchecked;
            buttonSwap.Tap += buttonSwap_Tap;
            buttonHideHud.Tap += buttonHideHud_Click;

            _frameRateTimer.Update += GameTimer_Update;
            _frameRateTimer.Draw += GameTimer_Draw;
        }

        #endregion

        #region Navigation Overrides

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            SharedGraphicsDeviceManager.Current.SynchronizeWithVerticalRetrace = true;
            SharedGraphicsDeviceManager.Current.PresentationInterval = PresentInterval.One;

            // Video clocks (pixels) are rendered as rectangles to a single SpriteBatch per frame
            _spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            // Obtain the requested game program from the query string of the incoming navigation URL, throw if somehow invalid
            var id = NavigationContext.QueryString["id"];
            var selectedGameProgramId = (GameProgramId)Enum.Parse(typeof(GameProgramId), id, false);

            // Look up the game details in the repository, throws if invalid somehow
            _gameProgramInfo = _repository.GetGameProgram(selectedGameProgramId);

            // Try restoring any previously saved game state
            if (!TryRestoringGameState())
            {
                // Start fresh if game state does not exist or somehow is corrupted
                _framesPerSecond = 60;
                _soundOff = false;
                _calibrationNeeded = true;
                CreateMachine();
            }

            // Maintain some startup invariants
            if (_framesPerSecond < 4)
                _framesPerSecond = 4;
            if (_framesPerSecond > 60)
                _framesPerSecond = 60;
            _powerOn = true;
            _showHud = false;
            _paused = false;

            // Create and wire up structures to the virtual machine now on hand
            AttachToMachine();

            // Sync up the UI
            LayoutRoot.Visibility = Visibility.Collapsed;
            runCurrentPlayer.Text = _inputHandler.CurrentPlayerNoText;
            SetPauseOff();

            // Configure the frame rates and start the timer
            ConfigureFrameRates();
            _frameRateTimer.Start();

            // Start up the worker thread that will run the emulator machinery
            _updateWorker = new UpdateWorker(_stopwatch);
            _updateWorker.Update += UpdateWorker_Update;
            _updateWorker.Start();

#if PROFILE
            _profilerUpdateDuration.Reset();
            _profilerDrawDuration.Reset();
            _profilerDrawRate.Reset();
#endif
            // Ensure the sound is audible
            SoundEffect.MasterVolume = 1.0f;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _updateWorker.Stop();
            _frameRateTimer.Stop();

            // Sync the pause/play icon since we have the latest information for that
            _viewModelRepository.SetPausedState(_gameProgramInfo.Id, _powerOn);

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            // The worker must be fully quieced before proceeding further
            _updateWorker.Dispose();

            DisposeCurrentDynamicSound();

            if (_powerOn)
                PersistGameState();
            else
                PurgeGameState();

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Handlers

        #region GamePage Handlers

        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            var actualWidth = (int)ActualWidth;
            var actualHeight = (int)ActualHeight;
            if (actualWidth <= 0 || actualHeight <= 0)
                return;
            if (_elementRenderer != null)
                _elementRenderer.Dispose();
            SharedGraphicsDeviceManager.Current.PreferredBackBufferWidth = actualWidth;
            SharedGraphicsDeviceManager.Current.PreferredBackBufferHeight = actualHeight;
            _elementRenderer = new UIElementRenderer(this, actualWidth, actualHeight);
        }

        void GamePage_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (!_showHud)
            {
                ShowHud();
                e.Cancel = true;
            }
            else
            {
                try
                {
                    NavigationService.GoBack();
                }
                catch (InvalidOperationException)
                {
                    // navigation already in progress.
                }
            }
        }

        #endregion

        #region GameTimer Handlers

        void GameTimer_Update(object sender, GameTimerEventArgs e)
        {
            if (!_paused)
                _inputHandler.Update();

            if (!_paused && _powerOn && !_showHud)
                _inputHandler.HandleInput();

            if (_calibrationNeeded && _updateWorker.CalibrationInfoReady)
            {
                var updateRequestsPerSecond = _updateWorker.UpdateRequestsPerSecond;
                var updatesPerSecond = (int)_updateWorker.UpdatesPerSecond;
                if ((updateRequestsPerSecond - updatesPerSecond) > 4)
                {
                    if (updatesPerSecond < _framesPerSecond)
                    {
                        _framesPerSecond = updatesPerSecond;
                        ConfigureFrameRates();
                        if (_showHud)
                            textblockFpsInfo.Visibility = Visibility.Visible;
                    }
                }
                _calibrationNeeded = false;
                _soundConfigurationNeeded = true;
                runCalibrating.Text = string.Empty;
            }

            // Keeps HUD in sync with sound on/off state
            if (_showHud && togglebuttonSound.IsChecked == _soundOff)
                togglebuttonSound.IsChecked = !_soundOff;

            _updateWorker.RequestUpdate();
        }

        void UpdateWorker_Update(object sender, EventArgs e)
        {
#if PROFILE
            _profilerUpdateDuration.Begin();
#endif
            // Run the emulator core to compute the next frame.
            if (_paused)
            {
            }
            else if (_powerOn)
            {
                _machine.ComputeNextFrame(_frameBuffer);
            }
            else
            {
                ComputeSnowSoundFrame(_frameBuffer);
            }

            if (_soundConfigurationNeeded)
            {
                // Rapid FPS manipulation has been observed to possibly cause crashes (via unhandled exceptions), but this has not been reliably reproduced.
                // Suspect it is occurring intermittently here, so we'll catch/retry until successful.
                try
                {
                    ConfigureSoundPlaybackRate();
                    _soundConfigurationNeeded = false;
                }
                catch (Exception ex)
                {
                    if (ex is OutOfMemoryException || ex is System.Threading.ThreadAbortException)
                        throw;
                }
            }

            if (_dynamicSound != null && !_soundConfigurationNeeded)
            {
                switch (_dynamicSound.State)
                {
                    case SoundState.Paused:
                    case SoundState.Stopped:
                        _dynamicSound.Play();
                        break;
                    case SoundState.Playing:
                        if (_dynamicSound.PendingBufferCount == 0 || _paused)
                            _dynamicSound.Pause();
                        break;
                }

                if (_frameRateTimer.UpdateInterval.Ticks > 0)
                {
                    var pendingBufferCount = _dynamicSound.PendingBufferCount;
                    if (pendingBufferCount < 3)
                        _frameRateTimer.UpdateInterval = _speedupFrameInterval;
                    else if (pendingBufferCount > 5)
                        _frameRateTimer.UpdateInterval = _slowdownFrameInterval;
                    else
                        _frameRateTimer.UpdateInterval = _normalFrameInterval;
                }

                SubmitSoundBuffer(_frameBuffer);
            }

            _frameRenderer.Update(_powerOn ? _frameBuffer : null);
#if PROFILE
            _profilerUpdateDuration.End();
#endif
        }

        void GameTimer_Draw(object sender, GameTimerEventArgs e)
        {
#if PROFILE
            _profilerDrawRate.Sample();
            _profilerDrawDuration.Begin();
#endif
            _frameRenderer.Draw(_powerOn ? _frameBuffer : null);

            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.Black);

            if (_showHud)
                _elementRenderer.Render();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);

            _spriteBatch.Draw(_frameRenderer.Texture, _frameRenderer.TargetRect, null, Color.White);

            if (_showHud)
            {
                _spriteBatch.Draw(_elementRenderer.Texture, Vector2.Zero, Color.White);
            }
            else
            {
                _inputHandler.Draw(_spriteBatch);
            }

            _spriteBatch.End();
#if PROFILE
            _profilerDrawDuration.End();
#endif
        }

        #endregion

        #region HUD Handlers

        void togglebuttonPower_Checked(object sender, RoutedEventArgs e)
        {
            _powerOn = true;
            SetPauseOff();
            CreateMachine();
            AttachToMachine();
            if (_showHud)
                _frameRenderer.NotifyHudIsUp();
            else
                _frameRenderer.NotifyHudIsDown();
            StartCalibration();
        }

        void togglebuttonPower_Unchecked(object sender, RoutedEventArgs e)
        {
            _powerOn = false;
            PurgeGameState();
        }

        void buttonColorBW_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Color, true);
        }

        void togglebuttonLDiff_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInput(MachineInput.LeftDifficulty, true);
        }

        void togglebuttonRDiff_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInput(MachineInput.RightDifficulty, true);
        }

        void buttonSelect_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Select, true);
        }

        void buttonReset_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Reset, true);
        }

        void sliderFps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newValue = (int)e.NewValue;
            runFps.Text = newValue.ToString();
        }

        void sliderFps_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            var newFps = (int)sliderFps.Value;
            if (newFps == _framesPerSecond)
                return;
            SetPauseOff();
            _framesPerSecond = newFps;
            ConfigureFrameRates();
            StartCalibration();
        }

        void togglebuttonSound_Unchecked(object sender, RoutedEventArgs e)
        {
            _soundOff = true;
            _soundConfigurationNeeded = true;
        }

        void togglebuttonSound_Checked(object sender, RoutedEventArgs e)
        {
            if (!_soundOff)
                return;
            _soundOff = false;
            StartCalibration();
        }

        void togglebuttonPause_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPauseOff();
        }

        void togglebuttonPause_Checked(object sender, RoutedEventArgs e)
        {
            SetPauseOn();
        }

        void buttonSwap_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            switch (_gameProgramInfo.Controller.ControllerType)
            {
                case Controller.Joystick:
                case Controller.ProLineJoystick:
                    _inputHandler.CurrentPlayerNo ^= 1;
                    break;
                case Controller.Paddles:
                    _inputHandler.CurrentPlayerNo++;
                    break;
            }
            runCurrentPlayer.Text = _inputHandler.CurrentPlayerNoText;
        }

        void buttonHideHud_Click(object sender, RoutedEventArgs e)
        {
            SetPauseOff();
            HideHud();
        }

        #endregion

        #endregion

        #region Helpers

        void SetPauseOn()
        {
            _paused = true;
            togglebuttonPause.IsChecked = true;
        }

        void SetPauseOff()
        {
            _paused = false;
            togglebuttonPause.IsChecked = false;
            textblockFpsInfo.Visibility = Visibility.Collapsed;
        }

        void ShowHud()
        {
            _showHud = true;
            togglebuttonSound.IsChecked = !_soundOff;
            LayoutRoot.Visibility = Visibility.Visible;
            _frameRenderer.NotifyHudIsUp();
#if PROFILE
            var updateDurationAvgMillisecondsPerSample = _profilerUpdateDuration.AvgMillisecondsPerSample;
            var drawDurationAvgMillisecondsPerSample = _profilerDrawDuration.AvgMillisecondsPerSample;
            var updateRateRequestSamplesPerSecond = _updateWorker.UpdateRequestsPerSecond;
            var updateRateActualSamplesPerSecond = _updateWorker.UpdatesPerSecond;
            var drawRateSamplesPerSecond = _profilerDrawRate.SamplesPerSecond;
            var updateBacklogCount = _updateWorker.UpdateBacklogCount;

            var updDrawRatio = updateRateRequestSamplesPerSecond / drawRateSamplesPerSecond;

            _profilerUpdateDuration.Reset();
            _profilerDrawDuration.Reset();
            _profilerDrawRate.Reset();

            _updateWorker.ResetBacklogCount();

            runProfileStats.Text = string.Format("  U:{0:0.0}ms D:{1:0.0}ms Ur/s:{2:0} Ua/s:{3:0} D/s:{4:0} U/D:{5:0.00} BL:{6}",
                updateDurationAvgMillisecondsPerSample,
                drawDurationAvgMillisecondsPerSample,
                updateRateRequestSamplesPerSecond,
                updateRateActualSamplesPerSecond,
                drawRateSamplesPerSecond,
                updDrawRatio,
                updateBacklogCount);
#endif
        }

        void HideHud()
        {
            _showHud = false;
            LayoutRoot.Visibility = Visibility.Collapsed;
            _frameRenderer.NotifyHudIsDown();
        }

        void ConfigureFrameRates()
        {
            if (_framesPerSecond < 4)
                _framesPerSecond = 4;
            else if (_framesPerSecond > 60)
                _framesPerSecond = 60;

            var ticksPerFrame = TimeSpan.TicksPerSecond / _framesPerSecond;
            _normalFrameInterval   = TimeSpan.FromTicks(ticksPerFrame);
            _speedupFrameInterval  = _normalFrameInterval - TimeSpan.FromTicks(ticksPerFrame >> 1);
            _slowdownFrameInterval = _normalFrameInterval + TimeSpan.FromTicks(ticksPerFrame >> 1);

            _frameRateTimer.UpdateInterval = _normalFrameInterval;

            if (_updateWorker != null)
                _updateWorker.ResetBacklogCount();

            _soundConfigurationNeeded = true;

            sliderFps.Value = _framesPerSecond;
        }

        void StartCalibration()
        {
            _calibrationNeeded = true;

            // Calibration will still occur, there will just not be any HUD visibility of it
            if (_updateWorker != null)
            {
                runCalibrating.Text = "(Calibrating)";
                _updateWorker.ResetCalibrationInterval();
            }
        }

        void ConfigureSoundPlaybackRate()
        {
            DisposeCurrentDynamicSound();

            // Do not automatically turn sound on, let the user to this explicitly
            if (_soundOff)
                return;

            if (_framesPerSecond >= 16)
            {
                var sampleRate = _framesPerSecond * 524;
                _dynamicSound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Mono) { Volume = 1.0f };
            }
            else if (_framesPerSecond == 15)
            {
                _dynamicSound = new DynamicSoundEffectInstance(8000, AudioChannels.Mono) { Volume = 1.0f };
            }
            else
            {
                _soundOff = true;
            }
        }

        void DisposeCurrentDynamicSound()
        {
            var ds = _dynamicSound;
            if (ds == null)
                return;
            _dynamicSound = null;
            ds.Stop();
            ds.Dispose();
        }

        #endregion

        #region Sound Rendering Helpers

        void ComputeSnowSoundFrame(FrameBuffer frameBuffer)
        {
            for (var i = 0; i < frameBuffer.SoundBufferElementLength; i++)
            {
                var be = frameBuffer.SoundBuffer[i];
                for (var j = 0; j < BufferElement.SIZE; j++)
                    be[j] = (byte)_snowGenerator.Next(2);
                frameBuffer.SoundBuffer[i] = be;
            }
        }

        void SubmitSoundBuffer(FrameBuffer frameBuffer)
        {
            if (_audioFrame == null)
                _audioFrame = new byte[frameBuffer.SoundBufferByteLength * 2];

            for (int i = 0, j = 1; i < frameBuffer.SoundBufferElementLength; i++)
            {
                var be = frameBuffer.SoundBuffer[i];
                for (var k = 0; k < BufferElement.SIZE; k++, j += 2)
                    _audioFrame[j] = be[k];
            }
            _dynamicSound.SubmitBuffer(_audioFrame);
        }

        #endregion

        #region Machine Instantiation Helpers

        void CreateMachine()
        {
            var romBytes = _gameProgramInfo.RomBytes();
            var cart = Cart.Create(romBytes, _gameProgramInfo.CartType);
            var machine = MachineBase.Create(_gameProgramInfo.MachineType, cart, null, null,
                _gameProgramInfo.Controller.ControllerType, _gameProgramInfo.Controller.ControllerType, null);
            machine.Reset();
            _machine = machine;
        }

        void AttachToMachine()
        {
            _frameBuffer = _machine.CreateFrameBuffer();

            switch (_gameProgramInfo.MachineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    switch (_gameProgramInfo.Id)
                    {
                        case GameProgramId.Asteroids:
                        case GameProgramId.Frogger:
                        case GameProgramId.MissleCommand:
                        case GameProgramId.NightDriver:
                        case GameProgramId.Pacman:
                        case GameProgramId.YarsRevenge:
                            _frameRenderer = new FrameRenderer160Blender(_machine.Palette);
                            break;
                        default:
                            _frameRenderer = new FrameRenderer160(_machine.Palette);
                            break;
                    }
                    break;
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    _frameRenderer = new FrameRenderer320(_machine.Palette);
                    break;
            }

            togglebuttonLDiff.IsChecked = _machine.InputState.IsLeftDifficultyAConsoleSwitchSet;
            togglebuttonRDiff.IsChecked = _machine.InputState.IsRightDifficultyAConsoleSwitchSet;

            switch (_gameProgramInfo.Controller.ControllerType)
            {
                case Controller.ProLineJoystick:
                    _inputHandler = new InputHandlerDPad(_machine, false);
                    break;
                case Controller.Joystick:
                    if (_gameProgramInfo.Id == GameProgramId.Qbert || _gameProgramInfo.Id == GameProgramId.bnQ)
                    {
                        _inputHandler = new InputHandlerQbert(_machine);
                        break;
                    }
                    _inputHandler = new InputHandlerDPad(_machine, true);
                    break;
                case Controller.Paddles:
                    _inputHandler = new InputHandlerPaddle(_machine);
                    break;
                default:
                    _inputHandler = new InputHandlerDefault(_machine);
                    break;
            }

            if (_persistedCurrentPlayerNo >= 0)
                _inputHandler.CurrentPlayerNo = _persistedCurrentPlayerNo;
        }

        #endregion

        #region Isolated Storage GameState Persistence Helpers

        void PurgeGameState()
        {
            var fileName = StateUtils.ToSerializationFileName(_gameProgramInfo.Id);
            try
            {
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    userStore.DeleteFile(fileName);
                }
            }
            catch (IOException)
            {
            }
            catch (IsolatedStorageException)
            {
            }
        }

        void PersistGameState()
        {
            var fileName = StateUtils.ToSerializationFileName(_gameProgramInfo.Id);
            try
            {
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                using (var bw = new BinaryWriter(userStore.OpenFile(fileName, FileMode.Create)))
                {
                    bw.Write(2);
                    bw.Write(_framesPerSecond);
                    bw.Write(_soundOff);
                    bw.Write(_inputHandler.CurrentPlayerNo);
                    _machine.Serialize(bw);
                    bw.Flush();
                    bw.Close();
                }
            }
            catch (IOException)
            {
            }
            catch (IsolatedStorageException)
            {
            }
            catch (Emu7800SerializationException)
            {
            }
        }

        bool TryRestoringGameState()
        {
            _persistedCurrentPlayerNo = -1;
            var fileName = StateUtils.ToSerializationFileName(_gameProgramInfo.Id);
            try
            {
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                using (var br = new BinaryReader(userStore.OpenFile(fileName, FileMode.Open)))
                {
                    var version = br.ReadInt32();
                    if (version < 1 && version > 2)
                        return false;
                    _framesPerSecond = br.ReadInt32();
                    _soundOff = br.ReadBoolean();
                    switch (version)
                    {
                        case 1:
                            _persistedCurrentPlayerNo = br.ReadBoolean() ? 0 : 1;
                            break;
                        case 2:
                            _persistedCurrentPlayerNo = br.ReadInt32();
                            break;
                    }
                    _machine = MachineBase.Deserialize(br);
                    return true;
                }
            }
            catch (IOException)
            {
            }
            catch (IsolatedStorageException)
            {
            }
            catch (Emu7800SerializationException)
            {
            }
            catch (Exception ex)
            {
                // Extra assurance that garbage save state never blocks game availability.
                // 78S4 cart had a serialization defect, though the above should have been
                // sufficient to prevent those crash dump reports.

                if (ex is OutOfMemoryException
                ||  ex is StackOverflowException
                ||  ex is System.Threading.ThreadAbortException
                ||  ex is TypeInitializationException)
                    throw;
            }
            return false;
        }

        #endregion
    }
}