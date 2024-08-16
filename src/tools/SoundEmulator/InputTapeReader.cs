using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace EMU7800.SoundEmulator;

public class InputTapeReader
{
    readonly Queue<byte[]> _queue = new();

    public int Load(string fileName)
    {
        _queue.Clear();

        using (var sr = new StreamReader(fileName))
        {
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                    break;
                var parsedLine = ParseLine(line);
                if (parsedLine.Length > 0)
                {
                    _queue.Enqueue(parsedLine);
                }
            }
        }

        return _queue.Count;
    }

    public byte[] Dequeue()
        => _queue.Count > 0 ? _queue.Dequeue() : [];

    static byte[] ParseLine(string line)
    {
        var trimmedLine = line.Trim();
        if (trimmedLine.Length == 0 || trimmedLine[0] == ';')
        {
            return [];
        }

        var parsedLine = new byte[16];

        var splitTrimmedLine = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < parsedLine.Length && i < splitTrimmedLine.Length; i++)
        {
            if (byte.TryParse(splitTrimmedLine[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var byteVal))
            {
                parsedLine[i] = byteVal;
            }
        }

        return parsedLine;
    }
}
