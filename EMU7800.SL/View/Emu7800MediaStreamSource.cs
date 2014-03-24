using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using EMU7800.Core;
using EMU7800.SL.ViewModel;

namespace EMU7800.SL.View
{
    public class Emu7800MediaStreamSource : MediaStreamSource
    {
        #region Fields

        MediaStreamDescription _videoDescription, _audioDescription;
        readonly IDictionary<MediaSampleAttributeKeys, string> _emptyMediaSampleAttributes = new Dictionary<MediaSampleAttributeKeys, string>(0);

        MediaElement _hostingMediaElement;

        readonly MachineToStreamAdapter _machineToStreamAdapter;

        #endregion

        #region Constructors

        private Emu7800MediaStreamSource()
        {
        }

        public Emu7800MediaStreamSource(MachineToStreamAdapter machineToStreamAdapter)
        {
            if (machineToStreamAdapter == null)
                throw new ArgumentNullException("machineToStreamAdapter");

            _machineToStreamAdapter = machineToStreamAdapter;

            // milliseconds? (for latency reduction)
            AudioBufferLength = 30;
        }

        #endregion

        #region Public Members

        public double BlendFactor
        {
            get { return _machineToStreamAdapter.BlendFactor; }
            set { _machineToStreamAdapter.BlendFactor = value; }
        }

        public string PostedMessage
        {
            get { return _machineToStreamAdapter.PostedMessage; }
            set { _machineToStreamAdapter.PostedMessage = value; }
        }

        public int LeftOffset
        {
            get { return _machineToStreamAdapter.LeftOffset; }
            set { _machineToStreamAdapter.LeftOffset = value; }
        }

        public int ClipStart
        {
            get { return _machineToStreamAdapter.ClipStart; }
            set { _machineToStreamAdapter.ClipStart = value; }
        }

        public double RenderedFramesPerSecond
        {
            get { return (_hostingMediaElement != null) ? _hostingMediaElement.RenderedFramesPerSecond : 0.0; }
        }

        public double DroppedFramesPerSecond
        {
            get { return (_hostingMediaElement != null) ? _hostingMediaElement.DroppedFramesPerSecond : 0.0; }
        }

        public void SetHostingMediaElement(MediaElement mediaElement)
        {
            _hostingMediaElement = mediaElement;
        }

        public void RaiseInput(int playerNo, MachineInput machineInput, bool down)
        {
            _machineToStreamAdapter.RaiseInput(playerNo, machineInput, down);
        }

        #endregion

        #region MediaStreamSource Overrides

        protected override void OpenMediaAsync()
        {
            var sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            sourceAttributes[MediaSourceAttributesKeys.Duration] = "0";
            sourceAttributes[MediaSourceAttributesKeys.CanSeek] = bool.FalseString;

            var videoStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            videoStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            videoStreamAttributes[MediaStreamAttributeKeys.Height] = MachineToStreamAdapter.FrameHeight.ToString();
            videoStreamAttributes[MediaStreamAttributeKeys.Width] = MachineToStreamAdapter.FrameWidth.ToString();
            _videoDescription = new MediaStreamDescription(MediaStreamType.Video, videoStreamAttributes);

            var audioStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            audioStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = ToAudioCodec(_machineToStreamAdapter.SoundSampleFrequency);
            _audioDescription = new MediaStreamDescription(MediaStreamType.Audio, audioStreamAttributes);

            var availableStreams = new List<MediaStreamDescription> {_videoDescription, _audioDescription};

            ReportOpenMediaCompleted(sourceAttributes, availableStreams);
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            switch (mediaStreamType)
            {
                case MediaStreamType.Audio:
                    _machineToStreamAdapter.ComputeNextFrame();
                    ReportGetSampleCompleted(GetAudioSample());
                    break;
                case MediaStreamType.Video:
                    ReportGetSampleCompleted(GetVideoSample());
                    break;
            }
        }

        protected override void SeekAsync(long seekToTime)
        {
            _machineToStreamAdapter.CurrentMediaTimestamp = seekToTime;
            ReportSeekCompleted(seekToTime);
        }

        protected override void CloseMedia()
        {
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
        }

        #endregion

        #region Helpers

        MediaStreamSample GetVideoSample()
        {
            _machineToStreamAdapter.RenderNextVideoSample();
            return new MediaStreamSample(_videoDescription,
                _machineToStreamAdapter.FrameStream,
                _machineToStreamAdapter.FrameStreamOffset,
                _machineToStreamAdapter.FrameStreamSampleSize,
                _machineToStreamAdapter.CurrentMediaTimestamp,
                _emptyMediaSampleAttributes);
        }

        MediaStreamSample GetAudioSample()
        {
            return new MediaStreamSample(_audioDescription,
                _machineToStreamAdapter.AudioStream,
                _machineToStreamAdapter.AudioStreamOffset,
                _machineToStreamAdapter.AudioStreamSampleSize,
                _machineToStreamAdapter.CurrentMediaTimestamp,
                _emptyMediaSampleAttributes);
        }

        // X4: FormatTag: 1=WAVE_FORMAT_PCM
        // X4: Channels: 1
        // X8: SamplesPerSec: 31440
        // X8: AvgBytesPerSec: SamplesPerSec * channels * (bitsPerSample / 8) = 31440 * 1 * 8 / 8 = 31440
        // X4: BlockAlign: channels * (bitsPerSample / 8) = 1 * 8 / 8 = 1
        // X4: BitsPerSample: 8
        // X4: Size: 0
        static string ToAudioCodec(int samplesPerSecond)
        {
            return string.Format("01000100{0:X2}{1:X2}{2:X2}{3:X2}{0:X2}{1:X2}{2:X2}{3:X2}010008000000",
                                 ( samplesPerSecond        & 0xFF),
                                 ((samplesPerSecond >>  8) & 0xFF),
                                 ((samplesPerSecond >> 16) & 0xFF),
                                 ((samplesPerSecond >> 24) & 0xFF));
        }

        #endregion
    }
}
