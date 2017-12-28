using System;
using System.IO;
using EMU7800.Core;
using EMU7800.SL.Model;

namespace EMU7800.SL.ViewModel
{
    public class MachineToStreamAdapter
    {
        #region Fields

        public const int
            FrameWidth = 320,
            FrameHeight = 240,
            FramePixelSize = 4;

        const int
            NumberOfHundredNanosecondsPerSecond = 10000000,
            FramePitch = FrameWidth * FramePixelSize;

        readonly FrameBuffer _machineFrameBuffer;
        readonly byte[] _frameBuffer;
        readonly bool _doublePixelWidth;
        readonly int _frameDuration;

        readonly byte[] _audioFrameBuffer;

        readonly MemoryStream _frameStream, _audioStream;

        readonly FontRenderer _fontRenderer;
        byte _fontColor;
        string _postedMessageText;
        int _postedMessageExpirationCounter;
        double _blendFactor, _blendFactorComplement;

        readonly MachineBase _machine;

        #endregion

        #region Constructors

        private MachineToStreamAdapter()
        {
        }

        public MachineToStreamAdapter(GameProgramInfo gameProgramInfo) : this(ToMachine(gameProgramInfo))
        {
            LeftOffset = gameProgramInfo.LeftOffset;
            ClipStart = gameProgramInfo.ClipStart;
        }

        public MachineToStreamAdapter(MachineBase machine)
        {
            if (machine == null)
                throw new ArgumentNullException("machine");

            _machine = machine;

            BlendFactor = 0.5;

            _machineFrameBuffer = _machine.CreateFrameBuffer();

            _frameBuffer = new byte[FrameHeight * FrameWidth * FramePixelSize];
            for (var i = 0; i < _frameBuffer.Length; i += FramePixelSize)
            {
                _frameBuffer[i + 3] = 0xFF;
            }

            _frameDuration = NumberOfHundredNanosecondsPerSecond / _machine.FrameHZ;

            _audioFrameBuffer = new byte[_machineFrameBuffer.SoundBufferByteLength];

            _doublePixelWidth = _machineFrameBuffer.VisiblePitch == 160;

            _frameStream = new MemoryStream(_frameBuffer.Length * 2 * _machine.FrameHZ);
            _audioStream = new MemoryStream(_machineFrameBuffer.SoundBufferByteLength * 2 * _machine.FrameHZ);

            FrameStreamOffset = -_frameBuffer.Length;
            AudioStreamOffset = -_machineFrameBuffer.SoundBufferByteLength;
            CurrentMediaTimestamp = -_frameDuration;

            _fontRenderer = new FontRenderer();
        }

        #endregion

        #region Public Members

        public long CurrentMediaTimestamp { get; set; }

        public MemoryStream FrameStream
        {
            get { return _frameStream; }
        }

        public int FrameStreamOffset { get; private set; }

        public int FrameStreamSampleSize
        {
            get { return _frameBuffer.Length; }
        }

        public MemoryStream AudioStream
        {
            get { return _audioStream; }
        }

        public int AudioStreamOffset { get; private set; }

        public int AudioStreamSampleSize
        {
            get { return _machineFrameBuffer.SoundBufferByteLength; }
        }

        public int SoundSampleFrequency
        {
            get { return _machine.SoundSampleFrequency; }
        }

        public string PostedMessage
        {
            get { return _postedMessageText ?? string.Empty; }
            set
            {
                _postedMessageExpirationCounter = 120;
                _postedMessageText = value;
            }
        }

        public double BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (value < 0.0)
                    _blendFactor = 0.0;
                else if (value > 1.0)
                    _blendFactor = 1.0;
                else
                    _blendFactor = value;
                _blendFactorComplement = 1.0 - _blendFactor;
            }
        }

        public int LeftOffset { get; set; }

        public int ClipStart { get; set; }

        public void RaiseInput(int playerNo, MachineInput machineInput, bool down)
        {
            _machine.InputState.RaiseInput(playerNo, machineInput, down);
        }

        public void ComputeNextFrame()
        {
            CurrentMediaTimestamp += _frameDuration;
            _machine.ComputeNextFrame(_machineFrameBuffer);
            if (_postedMessageExpirationCounter-- > 0)
            {
                _fontRenderer.DrawText(_machineFrameBuffer, PostedMessage, LeftOffset + 2, _machine.FirstScanline + ClipStart + 4, _fontColor++, 0);
            }
            RenderNextAudioSample();
        }

        public void RenderNextVideoSample()
        {
            var tgtY = 0;
            for (var srcY = _machine.FirstScanline + ClipStart; srcY < _machineFrameBuffer.Scanlines; srcY++, tgtY++)
            {
                var tgtIndex = tgtY * FramePitch;
                for (var srcX = 0; srcX < _machineFrameBuffer.VisiblePitch && tgtY < FrameHeight; srcX++)
                {
                    var videoFrameBufferIndex = (srcY * _machineFrameBuffer.VisiblePitch + srcX + LeftOffset) % _machineFrameBuffer.VideoBufferByteLength;
                    var pixel = _machine.Palette[_machineFrameBuffer.VideoBuffer[videoFrameBufferIndex >> BufferElement.SHIFT][videoFrameBufferIndex]];

                    BlendIt(ref _frameBuffer[tgtIndex + 0], (byte)((pixel >> 0) & 0xFF) /* red */);
                    BlendIt(ref _frameBuffer[tgtIndex + 1], (byte)((pixel >> 8) & 0xFF) /* green */);
                    BlendIt(ref _frameBuffer[tgtIndex + 2], (byte)((pixel >> 16) & 0xFF) /* blue */);

                    if (_doublePixelWidth)
                    {
                        _frameBuffer[tgtIndex + 4] = _frameBuffer[tgtIndex];
                        _frameBuffer[tgtIndex + 5] = _frameBuffer[tgtIndex + 1];
                        _frameBuffer[tgtIndex + 6] = _frameBuffer[tgtIndex + 2];
                        tgtIndex += (FramePixelSize << 1);
                    }
                    else
                    {
                        tgtIndex += FramePixelSize;
                    }
                }
            }

            FrameStreamOffset += _frameBuffer.Length;
            if (FrameStreamOffset + _frameBuffer.Length > _frameStream.Length)
            {
                FrameStreamOffset = 0;
                _frameStream.Seek(FrameStreamOffset, SeekOrigin.Begin);
            }
            _frameStream.Write(_frameBuffer, 0, _frameBuffer.Length);
        }

        #endregion

        #region Helpers

        void RenderNextAudioSample()
        {
            AudioStreamOffset += _machineFrameBuffer.SoundBufferByteLength;
            if (AudioStreamOffset + _machineFrameBuffer.SoundBufferByteLength > _audioStream.Length)
            {
                AudioStreamOffset = 0;
                _audioStream.Seek(AudioStreamOffset, SeekOrigin.Begin);
            }

            for (int i = 0, s = 0; i < _machineFrameBuffer.SoundBufferElementLength; i++)
            {
                var be = _machineFrameBuffer.SoundBuffer[i];
                for (var j = 0; j < BufferElement.SIZE; j++, s++)
                    _audioFrameBuffer[s] = be[s];
            }
            _audioStream.Write(_audioFrameBuffer, 0, _audioFrameBuffer.Length);
        }

        void BlendIt(ref byte target, byte input)
        {
            target = (byte)(_blendFactorComplement * target + _blendFactor * input);
        }

        static MachineBase ToMachine(GameProgramInfo gameProgramInfo)
        {
            if (gameProgramInfo == null)
                throw new ArgumentNullException("gameProgramInfo");

            var cart = Cart.Create(gameProgramInfo.RomBytes, gameProgramInfo.CartType);
            var machine = MachineBase.Create(gameProgramInfo.MachineType, cart, null, null,
                                             gameProgramInfo.LeftController.ControllerType,
                                             gameProgramInfo.RightController.ControllerType, null);
            machine.Reset();
            return machine;
        }

        #endregion
    }
}
