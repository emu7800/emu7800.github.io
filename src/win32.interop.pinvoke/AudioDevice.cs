namespace EMU7800.Win32.Interop
{
    public static class AudioDevice
    {
        static int Frequency, BufferPayloadSizeInBytes, QueueLength;

        public static bool IsOpened { get; private set; }
        public static bool IsClosed => !IsOpened;

        public static uint WaveOutVolume
        {
            get => (uint)WinmmNativeMethods.GetVolume();
            set => WinmmNativeMethods.SetVolume(value);
        }

        public static uint ToVolume(int left, int right)
            => (((uint)left) & 0xffff) | ((((uint)right) & 0xffff) << 16);

        public static int CountBuffersQueued()
            => IsOpened? WinmmNativeMethods.GetBuffersQueued() : -1;

        public static void SubmitBuffer(byte[] buffer)
        {
            if (buffer.Length < BufferPayloadSizeInBytes)
                throw new System.ApplicationException("Bad SubmitBuffer request: buffer length is not at least " + BufferPayloadSizeInBytes);

            if (!IsOpened)
            {
                var ec = WinmmNativeMethods.Open(Frequency, BufferPayloadSizeInBytes, QueueLength);
                IsOpened = ec == 0;
            }

            if (IsOpened)
            {
                WinmmNativeMethods.Enqueue(buffer);
            }
        }

        public static void Close()
        {
            if (IsOpened)
            {
                WinmmNativeMethods.Close();
                IsOpened = false;
            }
        }

        public static void Configure(int frequency, int bufferSizeInBytes, int queueLength)
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

            Frequency = frequency;
            BufferPayloadSizeInBytes = bufferSizeInBytes;
            QueueLength = queueLength;
        }
    }
}
