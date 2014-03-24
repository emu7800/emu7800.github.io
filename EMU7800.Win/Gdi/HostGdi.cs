/*
 * HostGdi
 *
 * A GDI-based Host.
 *
 * Copyright © 2003-2007 Mike Murphy
 *
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EMU7800.Core;

namespace EMU7800.Win.Gdi
{
    public class HostGdiForm : Form
    {
        HostGdi H { get; set; }
        MachineBase M { get; set; }

        byte[] _scanlineBuffer;
        Rectangulator _rectangulator;
        bool _reqRefresh;

        Font _textFont;
        SolidBrush _textBrush;
        Graphics _rGraphics;
        readonly SolidBrush _sBrush = new SolidBrush(Color.Black);

        Dictionary<Keys, MachineInput> _keyBindings;

        readonly Stopwatch _stopwatch = new Stopwatch();
        const int FRAME_SAMPLES = 120;
        int _frameRectangleCount, _usedAudioBuffers;
        long _frameDuration, _frameSleepableDuration;
        readonly int[] _runMachineTicks = new int[FRAME_SAMPLES];
        readonly int[] _waitTicks = new int[FRAME_SAMPLES];
        readonly int[] _rectangles = new int[FRAME_SAMPLES];

        readonly FrameBuffer _frameBuffer;

        readonly ILogger _logger;

        #region Constructors

        private HostGdiForm()
        {
            InitializeComponent();
        }

        public HostGdiForm(HostGdi host, MachineBase m, ILogger logger) : this()
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (m == null)
                throw new ArgumentNullException("m");
            if (logger == null)
                throw new ArgumentNullException("logger");

            H = host;
            M = m;

            _frameBuffer = M.CreateFrameBuffer();

            _logger = logger;
        }

        #endregion

        public void Run()
        {
            _logger.WriteLine("GDI Host startup");

            Text = EMU7800Application.Title;

            _scanlineBuffer = new byte[_frameBuffer.VisiblePitch];
            var r = new Rectangulator(_frameBuffer.VisiblePitch, _frameBuffer.Scanlines);
            r.UpdateRect += OnUpdateRect;
            r.Palette = M.Palette;
            r.PixelAspectXRatio = 320 / _frameBuffer.VisiblePitch;
            r.OffsetLeft = 0;
            r.ClipTop = M.FirstScanline;
            r.ClipHeight = 240;
            r.UpdateTransformationParameters();
            _rectangulator = r;

            ShowInTaskbar = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            CenterToScreen();

            ClientSize = new Size(640, 480);
            MinimumSize = new Size(320 + 8, 240 + 27);

            // Enable double-buffering to avoid flicker
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            _textFont = new Font("Courier New", 18);
            _textBrush = new SolidBrush(Color.White);

            _frameDuration = Stopwatch.Frequency / H.EffectiveFPS;
            _frameSleepableDuration = _frameDuration - 4*Stopwatch.Frequency/1000;

            InitializeKeyBindings();

            Paint += OnPaint;
            Layout += OnLayout;
            Closing += OnClosing;

            MouseDown += (sender, e) => OnMouseClick(e, true);
            MouseUp += (sender, e) => OnMouseClick(e, false);
            MouseMove += (sender, e) => OnMouseMove_(e);

            KeyDown += (sender, e) => OnKeyPress(e, true);
            KeyUp += (sender, e) => OnKeyPress(e, false);
            KeyPreview = true;

            Show();

            _reqRefresh = true;

            RunMainLoop();

            _logger.WriteLine("GDI Host shutdown");

            Close();
        }

        void RunMainLoop()
        {
            while (true)
            {
                _stopwatch.Reset();
                _stopwatch.Start();

                _frameRectangleCount = 0;

                Application.DoEvents();

                if (H.Ended || !Created)
                {
                    break;
                }

                if ((M.FrameNumber & 3) == 0)
                    H.RaiseEmulatedDrivingInput();

                if (_reqRefresh)
                {
                    _rGraphics.Clear(Color.Black);
                    _rectangulator.DrawEntireFrame(H.Paused);
                    _reqRefresh = false;
                }

                if (!H.Paused && !M.MachineHalt)
                {
                    M.ComputeNextFrame(_frameBuffer);
                    SubmitFrameToRectangulator();
                }
                else if (H.Paused)
                {
                    for (var i = 0; i < _frameBuffer.SoundBufferElementLength; i++)
                        _frameBuffer.SoundBuffer[i].ClearAll();
                }

                var endOfRunMachineTick = _stopwatch.ElapsedTicks;

                ShowPostedMsg();

                var soundSampleRate = M.SoundSampleFrequency * H.EffectiveFPS / M.FrameHZ;
                _usedAudioBuffers = H.EnqueueAudio(_frameBuffer, soundSampleRate);

                while (_stopwatch.ElapsedTicks < _frameSleepableDuration) Thread.Sleep(1);
                while (_stopwatch.ElapsedTicks < _frameDuration) { }

                var endOfCycleTick = _stopwatch.ElapsedTicks;

                var statIndex = (int)M.FrameNumber % FRAME_SAMPLES;
                _runMachineTicks[statIndex] = (int)endOfRunMachineTick;
                _waitTicks[statIndex] = (int)(endOfCycleTick - endOfRunMachineTick);
                if (_frameRectangleCount != 0) _rectangles[statIndex] = _frameRectangleCount;
            }

            H.CloseAudio();
        }

        void InitializeKeyBindings()
        {
            _keyBindings = new Dictionary<Keys, MachineInput>
            {
                {Keys.Escape,       MachineInput.End},
                {Keys.P,            MachineInput.Pause},
                {Keys.X,            MachineInput.Fire},
                {Keys.Z,            MachineInput.Fire2},
                {Keys.Left,         MachineInput.Left},
                {Keys.Up,           MachineInput.Up},
                {Keys.Right,        MachineInput.Right},
                {Keys.Down,         MachineInput.Down},
                {Keys.NumPad7,      MachineInput.NumPad7},
                {Keys.NumPad8,      MachineInput.NumPad8},
                {Keys.NumPad9,      MachineInput.NumPad9},
                {Keys.NumPad4,      MachineInput.NumPad4},
                {Keys.NumPad5,      MachineInput.NumPad5},
                {Keys.NumPad6,      MachineInput.NumPad6},
                {Keys.NumPad1,      MachineInput.NumPad1},
                {Keys.NumPad2,      MachineInput.NumPad2},
                {Keys.NumPad3,      MachineInput.NumPad3},
                {Keys.Multiply,     MachineInput.NumPadMult},
                {Keys.NumPad0,      MachineInput.NumPad0},
                {Keys.Divide,       MachineInput.NumPadHash},
                {Keys.D1,           MachineInput.LeftDifficulty},
                {Keys.D2,           MachineInput.RightDifficulty},
                {Keys.F1,           MachineInput.SetKeyboardToPlayer1},
                {Keys.F2,           MachineInput.SetKeyboardToPlayer2},
                {Keys.F3,           MachineInput.SetKeyboardToPlayer3},
                {Keys.F4,           MachineInput.SetKeyboardToPlayer4},
                {Keys.F5,           MachineInput.PanLeft},
                {Keys.F6,           MachineInput.PanRight},
                {Keys.F7,           MachineInput.PanUp},
                {Keys.F8,           MachineInput.PanDown},
                {Keys.F11,          MachineInput.SaveMachine},
                {Keys.F12,          MachineInput.TakeScreenshot},
                {Keys.C,            MachineInput.Color},
                {Keys.F,            MachineInput.ShowFrameStats},
                {Keys.M,            MachineInput.Mute},
                {Keys.R,            MachineInput.Reset},
                {Keys.S,            MachineInput.Select}
            };

            // this may not be complete or 100% accurate--more effort required
            var keyToKeysMapping = new Dictionary<DirectX.Key, Keys>
            {
                {DirectX.Key.A,             Keys.A},
                {DirectX.Key.Add,           Keys.Add},
                {DirectX.Key.Apps,          Keys.Apps},
                {DirectX.Key.B,             Keys.B},
                {DirectX.Key.Back,          Keys.Back},
                {DirectX.Key.C,             Keys.C},
                {DirectX.Key.CapsLock,      Keys.CapsLock},
                {DirectX.Key.D,             Keys.D},
                {DirectX.Key.D0,            Keys.D0},
                {DirectX.Key.D1,            Keys.D1},
                {DirectX.Key.D2,            Keys.D2},
                {DirectX.Key.D3,            Keys.D3},
                {DirectX.Key.D4,            Keys.D4},
                {DirectX.Key.D5,            Keys.D5},
                {DirectX.Key.D6,            Keys.D6},
                {DirectX.Key.D7,            Keys.D7},
                {DirectX.Key.D8,            Keys.D8},
                {DirectX.Key.D9,            Keys.D9},
                {DirectX.Key.Decimal,       Keys.Decimal},
                {DirectX.Key.Delete,        Keys.Delete},
                {DirectX.Key.Divide,        Keys.Divide},
                {DirectX.Key.Down,          Keys.Down},
                {DirectX.Key.E,             Keys.E},
                {DirectX.Key.End,           Keys.End},
                {DirectX.Key.Return,        Keys.Enter},
                {DirectX.Key.Escape,        Keys.Escape},
                {DirectX.Key.F,             Keys.F},
                {DirectX.Key.F1,            Keys.F1},
                {DirectX.Key.F2,            Keys.F2},
                {DirectX.Key.F3,            Keys.F3},
                {DirectX.Key.F4,            Keys.F4},
                {DirectX.Key.F5,            Keys.F5},
                {DirectX.Key.F6,            Keys.F6},
                {DirectX.Key.F7,            Keys.F7},
                {DirectX.Key.F8,            Keys.F8},
                {DirectX.Key.F9,            Keys.F9},
                {DirectX.Key.F10,           Keys.F10},
                {DirectX.Key.F11,           Keys.F11},
                {DirectX.Key.F12,           Keys.F12},
                {DirectX.Key.F13,           Keys.F13},
                {DirectX.Key.F14,           Keys.F14},
                {DirectX.Key.F15,           Keys.F15},
                {DirectX.Key.G,             Keys.G},
                {DirectX.Key.H,             Keys.H},
                {DirectX.Key.Home,          Keys.Home},
                {DirectX.Key.I,             Keys.I},
                {DirectX.Key.Insert,        Keys.Insert},
                {DirectX.Key.J,             Keys.J},
                {DirectX.Key.K,             Keys.K},
                {DirectX.Key.L,             Keys.L},
                {DirectX.Key.LeftControl,   Keys.LControlKey},
                {DirectX.Key.Left,          Keys.Left},
                {DirectX.Key.LeftAlt,       Keys.LMenu},
                {DirectX.Key.LeftShift,     Keys.LShiftKey},
                {DirectX.Key.LeftWindows,   Keys.LWin},
                {DirectX.Key.M,             Keys.M},
                {DirectX.Key.Multiply,      Keys.Multiply},
                {DirectX.Key.N,             Keys.N},
                {DirectX.Key.Numlock,       Keys.NumLock},
                {DirectX.Key.NumPad0,       Keys.NumPad0},
                {DirectX.Key.NumPad1,       Keys.NumPad1},
                {DirectX.Key.NumPad2,       Keys.NumPad2},
                {DirectX.Key.NumPad3,       Keys.NumPad3},
                {DirectX.Key.NumPad4,       Keys.NumPad4},
                {DirectX.Key.NumPad5,       Keys.NumPad5},
                {DirectX.Key.NumPad6,       Keys.NumPad6},
                {DirectX.Key.NumPad7,       Keys.NumPad7},
                {DirectX.Key.NumPad8,       Keys.NumPad8},
                {DirectX.Key.NumPad9,       Keys.NumPad9},
                {DirectX.Key.O,             Keys.O},
                {DirectX.Key.P,             Keys.P},
                {DirectX.Key.PageDown,      Keys.PageDown},
                {DirectX.Key.PageUp,        Keys.PageUp},
                {DirectX.Key.Pause,         Keys.Pause},
                {DirectX.Key.Q,             Keys.Q},
                {DirectX.Key.R,             Keys.R},
                {DirectX.Key.RightControl,  Keys.RControlKey},
                {DirectX.Key.Right,         Keys.Right},
                {DirectX.Key.RightShift,    Keys.RShiftKey},
                {DirectX.Key.RightWindows,  Keys.RWin},
                {DirectX.Key.S,             Keys.S},
                {DirectX.Key.Scroll,        Keys.Scroll},
                {DirectX.Key.Space,         Keys.Space},
                {DirectX.Key.Subtract,      Keys.Subtract},
                {DirectX.Key.T,             Keys.T},
                {DirectX.Key.Tab,           Keys.Tab},
                {DirectX.Key.U,             Keys.U},
                {DirectX.Key.Up,            Keys.Up},
                {DirectX.Key.V,             Keys.V},
                {DirectX.Key.W,             Keys.W},
                {DirectX.Key.X,             Keys.X},
                {DirectX.Key.Y,             Keys.Y},
                {DirectX.Key.Z,             Keys.Z},
            };

            foreach (var keyVal in H.UpdateKeyBindingsFromGlobalSettings(HostBase.CreateDefaultKeyBindings()))
            {
                Keys key;
                if (!keyToKeysMapping.TryGetValue(keyVal.Key, out key))
                {
                    _logger.WriteLine("HostGdi.InitializeKeyBindings: unable to map DirectX {0} enum to equivalent WinFroms enum", keyVal.Key);
                    continue;
                }

                var keyValValue = keyVal.Value;
                var priorKey = key;
                foreach (var keysVal in _keyBindings.Where(keysVal => keysVal.Value.Equals(keyValValue)))
                {
                    priorKey = keysVal.Key;
                    break;
                }
                if (priorKey.Equals(key))
                    continue;

                _keyBindings.Remove(priorKey);
                _keyBindings.Add(key, keyVal.Value);
            }
        }

        void OnMouseClick(MouseEventArgs e, bool down)
        {
            if (!H.Paused && e.Button == MouseButtons.Left)
            {
                H.RaiseInput(MachineInput.Fire, down);
            }
        }

        void OnMouseMove_(MouseEventArgs e)
        {
            H.RaiseLightGunInput(e.X, e.Y);
            H.RaisePaddleInput(Width, e.X);
        }

        void OnKeyPress(KeyEventArgs e, bool down)
        {
            e.Handled = true;

            MachineInput hostInput;
            if (!_keyBindings.TryGetValue(e.KeyCode, out hostInput)) return;

            switch (hostInput)
            {
                case MachineInput.ShowFrameStats:
                    if (!down) break;
                    double rmTicks = 0.0, wTicks = 0.0, avgRectPerFrame = 0.0;
                    for (var i = 0; i < FRAME_SAMPLES; i++)
                    {
                        rmTicks += _runMachineTicks[i];
                        wTicks += _waitTicks[i];
                        avgRectPerFrame += _rectangles[i];
                    }
                    rmTicks = rmTicks * 1000 / Stopwatch.Frequency / FRAME_SAMPLES;
                    wTicks = wTicks * 1000 / Stopwatch.Frequency / FRAME_SAMPLES;
                    avgRectPerFrame /= FRAME_SAMPLES;
                    H.PostedMsg = string.Format("{0:0.0} {1:0.0} {2:0.0} {3:0} {4:0.0} RPF", rmTicks, wTicks, rmTicks + wTicks, _usedAudioBuffers, avgRectPerFrame);
                    break;
                case MachineInput.PanLeft:
                    if (!down) break;
                    _rectangulator.OffsetLeft++;
                    _rectangulator.UpdateTransformationParameters();
                    _reqRefresh = true;
                    break;
                case MachineInput.PanRight:
                    if (!down) break;
                    _rectangulator.OffsetLeft--;
                    _rectangulator.UpdateTransformationParameters();
                    _reqRefresh = true;
                    break;
                case MachineInput.PanUp:
                    if (!down) break;
                    _rectangulator.ClipTop++;
                    _rectangulator.UpdateTransformationParameters();
                    _reqRefresh = true;
                    break;
                case MachineInput.PanDown:
                    if (!down) break;
                    _rectangulator.ClipTop--;
                    _rectangulator.UpdateTransformationParameters();
                    _reqRefresh = true;
                    break;
                default:
                    H.RaiseInput(_keyBindings[e.KeyCode], down);
                    break;
            }
        }

        bool _RenderedTextMsg;

        void ShowPostedMsg()
        {
            if (H.PostedMsg.Length > 0)
            {
                ClearTextMsg();
                ShowTextMsg();
                _RenderedTextMsg = true;
            }
            else if (_RenderedTextMsg)
            {
                ClearTextMsg();
            }
        }

        void ShowTextMsg()
        {
            _textBrush.Color = Color.White;
            _rGraphics = CreateGraphics();
            _rGraphics.TextRenderingHint = TextRenderingHint.SystemDefault;
            _rGraphics.DrawString(H.PostedMsg, _textFont, _textBrush, 0, 0);
            _RenderedTextMsg = true;
        }

        void ClearTextMsg()
        {
            _textBrush.Color = Color.Black;
            _rGraphics.FillRectangle(_textBrush, 0, 0, ClientSize.Width, 30);
            _RenderedTextMsg = false;
        }

        void OnLayout(object sender, LayoutEventArgs e)
        {
            _rGraphics = CreateGraphics();
            _rGraphics.CompositingMode = CompositingMode.SourceCopy;
            _rGraphics.CompositingQuality = CompositingQuality.Invalid;
            _rGraphics.SmoothingMode = SmoothingMode.None;
            _rGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            _rectangulator.ViewPortSize = ClientSize;
            _rectangulator.UpdateTransformationParameters();

            _reqRefresh = true;
        }

        void OnClosing(object sender,  CancelEventArgs e)
        {
            if (!H.Ended)
            {
                // if game is running, veto close request while arranging for game loop termination
                // this facilitates cleaning things up nicely
                H.RaiseInput(MachineInput.End, true);
                e.Cancel = true;
            }
        }

        void OnPaint(object sender, PaintEventArgs e)
        {
            _reqRefresh = true;
        }

        void SubmitFrameToRectangulator()
        {
            _rectangulator.StartFrame();
            for (var scanline = 0; scanline < _frameBuffer.Scanlines; scanline++)
            {
                for (int x = 0, s = scanline * _frameBuffer.VideoBufferElementVisiblePitch; x < _frameBuffer.VideoBufferElementVisiblePitch; x++, s++)
                {
                    var bufferElement = _frameBuffer.VideoBuffer[s];
                    for (var offset = 0; offset < BufferElement.SIZE; offset++)
                    {
                        _scanlineBuffer[(x << BufferElement.SHIFT) | offset] = bufferElement[offset];
                    }
                }
                // TODO: Rectangulator should be enhanced to just consume the entire VideoFrameBuffer
                _rectangulator.InputScanline(_scanlineBuffer, scanline, 0, _frameBuffer.VisiblePitch);
            }
            _rectangulator.EndFrame();
        }

        void OnUpdateRect(DisplRect r)
        {
            _sBrush.Color = Color.FromArgb(r.Argb);
            _rGraphics.FillRectangle(_sBrush, r.Rectangle);
            _frameRectangleCount++;
        }

        private void InitializeComponent()
        {
            var resources = new ComponentResourceManager(typeof(HostGdiForm));
            SuspendLayout();
            // 
            // HostGdiForm
            // 
            ClientSize = new Size(284, 264);
            Icon = ((Icon)(resources.GetObject("$this.Icon")));
            Name = "HostGdiForm";
            ResumeLayout(false);

        }
    }

    public class HostGdi : HostBase
    {
        readonly MachineBase _m;
        readonly ILogger _logger;

        public HostGdi(MachineBase m, ILogger logger) : base(m, logger)
        {
            _m = m;
            _logger = logger;
        }

        public override void Run()
        {
            base.Run();
            using (var f = new HostGdiForm(this, _m, _logger))
            {
                f.Run();
            }
        }
    }
}