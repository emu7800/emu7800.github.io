using EMU7800.Core;
using EMU7800.WP.Model;
using EMU7800.WP.ViewModel;
using EMU7800.WP8.Interop;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.UI.Core;

namespace EMU7800.WP.View
{
    public partial class GamePage
    {
        #region Fields

        static App CurrentApp { get { return (App)Application.Current; } }

        readonly GameProgramInfoRepository _repository;
        readonly GameProgramSelectViewModel _viewModelRepository;
        readonly MogaController _mogaController;

        readonly Direct3DInterop _interop = new Direct3DInterop();
        bool _drawingSurfaceLoaded;

        volatile bool _showHud, _paused, _soundOff, _powerOn;

        MachineBase _machine;
        FrameBuffer _frameBuffer;
        FrameRenderer _frameRenderer;

        Thread _workerThread;
        volatile bool _stopRequested;

        volatile int _framesPerSecond, _proposedFrameRate;
        volatile bool _calibrationNeeded, _calibrating, _frameRateChangeNeeded;

        GameProgramInfo _gameProgramInfo;

        readonly uint[] _frameDurationBuckets = new uint[0x100];
        readonly double _stopwatchFrequencyInMilliseconds = Stopwatch.Frequency / 1000.0;
        uint _frameDurationBucketSamples;

        readonly Random _snowGenerator = new Random();

        InputHandler _inputHandler;
        int _persistedCurrentPlayerNo;

        bool _hasNavigatedHere, _hasNavigatedAway;

        #endregion

        public GamePage()
        {
            InitializeComponent();

            _repository = CurrentApp.Services.GetService<GameProgramInfoRepository>();
            _viewModelRepository = CurrentApp.Services.GetService<GameProgramSelectViewModel>();
            _mogaController = CurrentApp.Services.GetService<MogaController>();

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

            HideHud();
        }

        #region Interop Handlers

        void DrawingSurface_OnLoaded(object sender, RoutedEventArgs e)
        {
            _interop.PointerPressed  += OnPointerPressed;
            _interop.PointerMoved    += OnPointerMoved;
            _interop.PointerReleased += OnPointerReleased;

            _interop.ActualWidth  = ActualWidth;
            _interop.ActualHeight = ActualHeight;
            _interop.ScaleFactor  = Application.Current.Host.Content.ScaleFactor;
            // WVGA=100
            // WXGA=160
            // 720P=150

            var nativeWidth  = ToNativeDimension(ActualWidth);
            var nativeHeight = ToNativeDimension(ActualHeight);
            _interop.RenderWidth  = nativeWidth;
            _interop.RenderHeight = nativeHeight;

            var destWidth  = nativeHeight*4/3;
            var destX = (nativeWidth - destWidth) / 2;
            _interop.DestRectLeft   = destX;
            _interop.DestRectTop    = 0;
            _interop.DestRectRight  = destX + destWidth;
            _interop.DestRectBottom = nativeHeight;

            // Hook-up native component to DrawingSurface
            var cp = _interop.CreateContentProvider();
            DrawingSurface.SetContentProvider(cp);
            DrawingSurface.SetManipulationHandler(_interop);

            // Start up the worker thread that will run the emulator machinery
            StartWorker();

            _drawingSurfaceLoaded = true;
        }

        static int ToNativeDimension(double dipValue)
        {
            var scaleFactor = Application.Current.Host.Content.ScaleFactor;
            return (int)Math.Floor((float)dipValue * scaleFactor / 100.0f + 0.5f);
        }

        void OnPointerPressed(PointerEventArgs args)
        {
            if (_inputHandler == null)
                return;
            _inputHandler.OnPointerPressed(args);
        }

        void OnPointerMoved(PointerEventArgs args)
        {
            if (_inputHandler == null)
                return;
            _inputHandler.OnPointerMoved(args);
        }

        void OnPointerReleased(PointerEventArgs args)
        {
            if (_inputHandler == null)
                return;
            _inputHandler.OnPointerReleased(args);
        }

        #endregion

        #region Navigation Overrides

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // paranoia: ensure idempotency
            if (_hasNavigatedHere)
                return;

            base.OnNavigatedTo(e);

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

            sliderFps.Value = _framesPerSecond;

            _powerOn = true;
            _showHud = false;
            SetPauseOff();

            // Create and wire up structures to the virtual machine now on hand
            AttachToMachine();

            // Sync up the UI
            runCurrentPlayer.Text = _inputHandler.CurrentPlayerNoText;
            togglebuttonSound.IsChecked = !_soundOff;

            if (_drawingSurfaceLoaded)
            {
                // Start up the worker thread that will run the emulator machinery
                // But only do it here if DrawingSurfaceLoaded event handler has already run
                // ...which will be the case if we are returning after being suspended
                StartWorker();
            }

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            _hasNavigatedHere = true;
            _hasNavigatedAway = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // paranoia: ensure idempotency
            if (_hasNavigatedAway)
                return;

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;

            // Ensure game state is quiesced prior to peristing
            StopWorker();

            // Sync the pause/play icon since we have the latest information for that
            _viewModelRepository.SetPausedState(_gameProgramInfo.Id, _powerOn);

            if (_powerOn)
                PersistGameState();
            else
                PurgeGameState();

            _hasNavigatedHere = false;
            _hasNavigatedAway = true;

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region GamePage Handlers

        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
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

        #region HUD Handlers

        void togglebuttonPower_Checked(object sender, RoutedEventArgs e)
        {
            CreateMachine();
            AttachToMachine();
            SetPauseOff();
            _powerOn = true;
            _calibrationNeeded = true;
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
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Color);
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
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Select);
        }

        void buttonReset_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
            SetPauseOff();
            _inputHandler.RaiseMachineInputWithButtonUpCounter(MachineInput.Reset);
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
            _proposedFrameRate = newFps;
            _frameRateChangeNeeded = true;
        }

        void togglebuttonSound_Unchecked(object sender, RoutedEventArgs e)
        {
            _soundOff = true;
        }

        void togglebuttonSound_Checked(object sender, RoutedEventArgs e)
        {
            if (!_soundOff)
                return;
            _soundOff = false;
            _calibrationNeeded = true;
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
            Hud.Visibility = Visibility.Visible;
            if (_frameRenderer != null)
                _frameRenderer.NotifyHudIsUp();
        }

        void HideHud()
        {
            _showHud = false;
            Hud.Visibility = Visibility.Collapsed;
            if (_frameRenderer != null)
                _frameRenderer.NotifyHudIsDown();
        }

        #endregion

        #region Sound Rendering Helpers

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

            if (_showHud)
                _frameRenderer.NotifyHudIsUp();
            else
                _frameRenderer.NotifyHudIsDown();

            togglebuttonLDiff.IsChecked = _machine.InputState.IsLeftDifficultyAConsoleSwitchSet;
            togglebuttonRDiff.IsChecked = _machine.InputState.IsRightDifficultyAConsoleSwitchSet;

            switch (_gameProgramInfo.Controller.ControllerType)
            {
                case Controller.ProLineJoystick:
                    _inputHandler = new InputHandlerDPad(_machine, _interop, _mogaController, false);
                    runControllerName.Text = "ProLine Joystick";
                    break;
                case Controller.Joystick:
                    if (_gameProgramInfo.Id == GameProgramId.Qbert || _gameProgramInfo.Id == GameProgramId.bnQ)
                    {
                        _inputHandler = new InputHandlerQbert(_machine, _interop, _mogaController);
                        runControllerName.Text = "ProLine Joystick";
                        break;
                    }
                    _inputHandler = new InputHandlerDPad(_machine, _interop, _mogaController, true);
                    runControllerName.Text = "Joystick";
                    break;
                case Controller.Paddles:
                    _inputHandler = new InputHandlerPaddle(_machine);
                    runControllerName.Text = "Paddles";
                    break;
                case Controller.Lightgun:
                    _inputHandler = new InputHandlerLightgun(_machine, _interop);
                    runControllerName.Text = "Lightgun";
                    break;
                default:
                    _inputHandler = new InputHandlerDefault(_machine);
                    runControllerName.Text = string.Empty;
                    break;
            }

            // Try to collect ScreenWidth/Height here - possible when Power button is cycled
            if (ActualWidth > 0)
                _inputHandler.ScreenWidth = (int)ActualWidth;
            if (ActualHeight > 0)
                _inputHandler.ScreenHeight = (int)ActualHeight;

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
                || ex is StackOverflowException
                || ex is ThreadAbortException
                || ex is TypeInitializationException)
                    throw;
            }
            return false;
        }

        #endregion
    }
}