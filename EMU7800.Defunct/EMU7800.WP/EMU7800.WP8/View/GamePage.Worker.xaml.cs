using EMU7800.Core;
using EMU7800.WP8.Interop;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EMU7800.WP.View
{
    public partial class GamePage
    {
        void StartWorker()
        {
            // Collect the screenwidth/height for the input handler here as these values should be correct by now
            _inputHandler.ScreenWidth = (int)ActualWidth;
            _inputHandler.ScreenHeight = (int)ActualHeight;

            _stopRequested = false;

            _workerThread = new Thread(RunWorker);
            _workerThread.Start();
        }

        void StopWorker()
        {
            if (_workerThread == null)
                return;
            _stopRequested = true;
            _workerThread.Join(5000);
            _workerThread = null;
        }

        void RunWorker()
        {
            var audioBytes = new byte[_frameBuffer.SoundBufferByteLength];
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var ticksPerFrame = Stopwatch.Frequency / _framesPerSecond;
            AudioDevice audioDevice = null;

            while (!_stopRequested)
            {
                var startTick = stopwatch.ElapsedTicks;
                var endTick = startTick + ticksPerFrame;

                _inputHandler.Update();

                if (_calibrationNeeded)
                {
                    _calibrationNeeded = false;
                    _calibrating = !_soundOff;
                    if (_calibrating)
                    {
                        _frameDurationBucketSamples = 0;
                        for (var i = 0; i < _frameDurationBuckets.Length; i++)
                        {
                            _frameDurationBuckets[i] = 0;
                        }
                        Dispatcher.BeginInvoke(() => runCalibrating.Text = "(Calibrating...)");
                    }
                }

                if (_calibrating && _frameDurationBucketSamples > 200)
                {
                    Dispatcher.BeginInvoke(() => runCalibrating.Text = string.Empty);
                    _calibrating = false;

                    var frameDurationSamplesNeeded = (int)(_frameDurationBucketSamples * 95 / 100);
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

                    if (_framesPerSecond > _proposedFrameRate)
                        _frameRateChangeNeeded = true;
                }

                if (_frameRateChangeNeeded)
                {
                    _frameRateChangeNeeded = false;
                    if (audioDevice != null)
                    {
                        audioDevice.Dispose();
                        audioDevice = null;
                    }
                    if (_proposedFrameRate > _framesPerSecond)
                        _calibrationNeeded = true;

                    _framesPerSecond = _proposedFrameRate;
                    ticksPerFrame = Stopwatch.Frequency / _framesPerSecond;
                    Dispatcher.BeginInvoke(() => sliderFps.Value = _framesPerSecond);
                }

                if (!_soundOff && audioDevice == null)
                {
                    var soundFrequency = _frameBuffer.SoundBufferByteLength * _framesPerSecond;
                    audioDevice = new AudioDevice(soundFrequency, _frameBuffer.SoundBufferByteLength, 8);
                }

                var buffersQueued = (audioDevice != null) ? audioDevice.BuffersQueued : -1;
                long adjustment = 0;
                if (buffersQueued < 0 || _soundOff || _paused)
                    adjustment = 0;
                else if (buffersQueued < 2)
                    adjustment = -(ticksPerFrame >> 1);
                else if (buffersQueued > 4)
                    adjustment = ticksPerFrame >> 1;
                endTick += adjustment;

                if (_powerOn)
                {
                    if (!_paused)
                    {
                        _machine.ComputeNextFrame(_frameBuffer);
                        _frameRenderer.Update(_frameBuffer);
                        _frameRenderer.Draw(_frameBuffer);
                        if (_mogaController.IsConnected)
                        {
                            _frameRenderer.DrawMogaConnectedFeedback();
                        }
                        else
                        {
                            switch (_gameProgramInfo.Controller.ControllerType)
                            {
                                case Controller.Joystick:
                                case Controller.ProLineJoystick:
                                    _frameRenderer.DrawDPadControllerFeedback(_interop.IsDPadLeft, _interop.IsDPadUp, _interop.IsDPadRight, _interop.IsDPadDown);
                                    _frameRenderer.DrawFireButtonControllerFeedback(_interop.IsFire1, _interop.IsFire2);
                                    break;
                            }
                        }
                        _interop.SubmitFrameBuffer(_frameRenderer.TextureData);
                    }
                }
                else
                {
                    ComputeSnowSoundFrame(_frameBuffer);
                    _frameRenderer.Update(null);
                    _frameRenderer.Draw(null);
                    _interop.SubmitFrameBuffer(_frameRenderer.TextureData);
                }

                if (!_soundOff && !_paused && audioDevice != null)
                {
                    UpdateAudioBytes(_frameBuffer, audioBytes);
                    audioDevice.SubmitBuffer(audioBytes);
                }

                var elaspedTicks = stopwatch.ElapsedTicks;
                var frameMilliseconds = (uint)((elaspedTicks - startTick) / _stopwatchFrequencyInMilliseconds);
                if (!_soundOff && frameMilliseconds < _frameDurationBuckets.Length)
                {
                    _frameDurationBuckets[frameMilliseconds]++;
                    _frameDurationBucketSamples++;
                }

                while (stopwatch.ElapsedTicks < endTick)
                {
                    Task.Yield();
                }
            }

            if (audioDevice != null)
                audioDevice.Dispose();
        }
    }
}
