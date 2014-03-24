using System;
using System.Collections.Generic;
using System.IO;

namespace EMU7800.SoundEmulator
{
    public class InputTapeReader
    {
        readonly Queue<byte[]> _queue = new Queue<byte[]>();

        public int Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            _queue.Clear();
            using (var sr = new StreamReader(fileName))
            {
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;
                    var parsedLine = ParseLine(line);
                    if (parsedLine != null)
                    {
                        _queue.Enqueue(parsedLine);
                    }
                }
            }

            return _queue.Count;
        }

        public byte[] Dequeue()
        {
            return _queue.Count > 0 ? _queue.Dequeue() : null;
        }

        byte[] ParseLine(string line)
        {
            const int byteCount = 16;
            byte[] parsedLine = null;
            var isDigitMode = false;
            var currentIndex = -1;

            for (var i = 0; i < line.Length && currentIndex < byteCount; i++)
            {
                var ch = line[i];
                var isDigit = (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');

                if (i == 0)
                {
                    if (!isDigit)
                        break;
                    parsedLine = new byte[byteCount];
                }
                if (isDigitMode)
                {
                    if (!isDigit)
                        isDigitMode = false;
                }
                else if (isDigit)
                {
                    isDigitMode = true;
                    currentIndex++;
                }
                if (!isDigitMode || parsedLine == null)
                    continue;

                byte val = 0;
                if (ch >= '0' && ch <= '9')
                    val = (byte)(ch - '0');
                else if (ch >= 'a' && ch <= 'f')
                    val = (byte) ((ch - 'a') + 0xa);
                else if (ch >= 'A' && ch <= 'F')
                    val = (byte)((ch - 'A') + 0xa);

                parsedLine[currentIndex] <<= 4;
                parsedLine[currentIndex] |= val;
            }

            return parsedLine;
        }
    }
}
