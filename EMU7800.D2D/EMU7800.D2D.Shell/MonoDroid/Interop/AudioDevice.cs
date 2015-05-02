using Android.Media;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EMU7800.D2D.Interop
{
    public sealed class AudioDevice : IDisposable
    {
        #region Fields

        readonly object _locker = new object();
        readonly int _frequency, _bufferSizeInBytes, _queueLength;
        readonly Queue<byte[]> _queue;
        readonly Queue<byte[]> _freeQueue;
        readonly Thread _playbackThread;
        bool _disposed;

        #endregion

        public int BuffersQueued
        {
            get; private set;
        }

        public int SubmitBuffer(byte[] buffer)
        {
            if (_disposed)
                return BuffersQueued = -1;

            lock (_locker)
            {
                if (_freeQueue.Count > 0)
                {
                    var buf = _freeQueue.Dequeue();
                    Buffer.BlockCopy(buffer, 0, buf, 0, Math.Min(buf.Length, buffer.Length));
                    _queue.Enqueue(buf);
                    BuffersQueued = _queue.Count;
                    Monitor.Pulse(_locker);
                }
            }

            return BuffersQueued;
        }

        #region IDisposable Members

        ~AudioDevice()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;
                try { _playbackThread.Join(2000); } catch {}
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Constructors

        public AudioDevice(int frequency, int bufferSizeInBytes, int queueLength)
        {
            if (frequency < 0)
                frequency = 0;

            if (bufferSizeInBytes < 0)
                bufferSizeInBytes = 0;
            else if (bufferSizeInBytes > 0x400)
                bufferSizeInBytes = 0x400;

            if (queueLength < 0)
                queueLength = 0;
            else if (queueLength > 0x10)
                queueLength = 0x10;

            _frequency = frequency;
            _bufferSizeInBytes = bufferSizeInBytes;
            _queueLength = queueLength;

            _queue     = new Queue<byte[]>(_queueLength);
            _freeQueue = new Queue<byte[]>(_queueLength);
            for (var i = 0; i < _queueLength; i++)
            {
                _freeQueue.Enqueue(new byte[_bufferSizeInBytes]);
            }

            _playbackThread = new Thread(DoPlayback);
            _playbackThread.Start();
        }

        #endregion

        #region Helpers

        void DoPlayback(object state)
        {
            var bufferSize = AudioTrack.GetMinBufferSize(_frequency, ChannelOut.Mono, Encoding.Pcm8bit);
            using (var audioTrack = new AudioTrack(Stream.Music, _frequency, ChannelOut.Mono, Encoding.Pcm8bit, bufferSize, AudioTrackMode.Stream))
            {
                DoPlaybackLoop(audioTrack);
                audioTrack.Stop();
            }
        }

        void DoPlaybackLoop(AudioTrack audioTrack)
        {
            var hasStarted = false;

            byte[] buf = null;

            while (!_disposed)
            {
                lock (_locker)
                {
                    if (buf != null)
                    {
                        _freeQueue.Enqueue(buf);
                        buf = null;
                    }

                    while (_queue.Count == 0)
                        Monitor.Wait(_locker);

                    buf = _queue.Dequeue();
                    BuffersQueued = _queue.Count;
                }

                if (!hasStarted)
                {
                    audioTrack.Play();
                    hasStarted = true;
                }
                audioTrack.Write(buf, 0, buf.Length);
            }
        }

        #endregion
    }
}