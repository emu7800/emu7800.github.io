using System;

namespace EMU7800.Core
{
    public class FrameBuffer
    {
        public static readonly FrameBuffer Default = new FrameBuffer(0, 0);

        /// <summary>
        /// Number of visible pixels on a single horizontal line.
        /// </summary>
        public int VisiblePitch { get; }

        /// <summary>
        /// Number of <see cref="BufferElement"/>s that represent <c>VisiblePitch</c>.
        /// </summary>
        public int VideoBufferElementVisiblePitch { get; }

        /// <summary>
        /// Number of visible scan lines.
        /// </summary>
        public int Scanlines { get; }

        /// <summary>
        /// The number of bytes contained by <c>VideoBuffer</c>.
        /// </summary>
        public int VideoBufferByteLength { get; }

        /// <summary>
        /// The number of <see cref="BufferElement"/>s contained by <c>VideoBuffer</c>
        /// </summary>
        public int VideoBufferElementLength { get; }

        /// <summary>
        /// The number of bytes contained by <c>SoundBuffer</c>.
        /// </summary>
        public int SoundBufferByteLength { get; }

        /// <summary>
        /// The number of <see cref="BufferElement"/>s contained by <c>SoundBuffer</c>
        /// </summary>
        public int SoundBufferElementLength { get; }

        /// <summary>
        /// The buffer containing computed pixel data.
        /// </summary>
        public BufferElement[] VideoBuffer { get; }

        /// <summary>
        /// The buffer containing computed PCM audio data.
        /// </summary>
        public BufferElement[] SoundBuffer { get; }

        #region Constructors

        internal FrameBuffer(int visiblePitch, int scanLines)
        {
            if (visiblePitch < 0)
                throw new ArgumentException("visiblePitch must be non-negative.");
            if (scanLines < 0)
                throw new ArgumentException("scanLines must be non-negative.");

            VisiblePitch = visiblePitch;
            VideoBufferElementVisiblePitch = VisiblePitch >> BufferElement.SHIFT;
            Scanlines = scanLines;
            VideoBufferByteLength = VisiblePitch * Scanlines;
            VideoBufferElementLength = VideoBufferElementVisiblePitch * Scanlines;
            SoundBufferByteLength = Scanlines << 1;
            SoundBufferElementLength = SoundBufferByteLength >> BufferElement.SHIFT;

            VideoBuffer = new BufferElement[VideoBufferElementLength + (64 >> BufferElement.SHIFT)];
            SoundBuffer = new BufferElement[SoundBufferElementLength + (64 >> BufferElement.SHIFT)];
        }

        #endregion
    }
}
