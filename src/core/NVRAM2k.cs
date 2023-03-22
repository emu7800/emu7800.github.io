/*
 * NVRAM2k.cs
 *
 * Implements a non-volatile 2KB memory device.
 *
 * Copyright © 2023 Mike Murphy
 *
 */
namespace EMU7800.Core;

public sealed class NVRAM2k : IDevice
{
    public static readonly NVRAM2k Default = new(string.Empty);

    readonly byte[] NVRAM = new byte[NVRAM_SIZE];
    readonly string _fileName;

    #region IDevice

    const int
        NVRAM_SHIFT = 11, // 2 KB size
        NVRAM_SIZE  = 1 << NVRAM_SHIFT,
        NVRAM_MASK  = NVRAM_SIZE - 1
        ;

    public byte this[ushort addr]
    {
        get => NVRAM[addr & NVRAM_MASK];
        set => NVRAM[addr & NVRAM_MASK] = value;
    }

    public void Reset()
    {
    }

    #endregion

    #region Constructors

    public NVRAM2k(string fileName)
    {
        _fileName = fileName;
        LoadNVRAM();
    }

    #endregion

    #region Serialization Members

    public NVRAM2k(DeserializationContext input)
    {
        input.CheckVersion(1);
        _fileName = input.ReadString();
        LoadNVRAM();
    }

    public void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(_fileName);
        SaveNVRAM();
    }

    #endregion

    #region Helpers

    void LoadNVRAM()
    {
        if (string.IsNullOrWhiteSpace(_fileName))
            return;

        var dir = ToNVRAMDir();
        var path = System.IO.Path.Combine(dir, _fileName);
        try
        {
            using var fs = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            using var br = new System.IO.BinaryReader(fs, System.Text.Encoding.UTF8, leaveOpen: true);
            var bytes = br.ReadBytes(NVRAM_SIZE);
            System.Buffer.BlockCopy(bytes, 0, NVRAM, 0, NVRAM_SIZE);
        }
        catch
        {
        }
    }

    void SaveNVRAM()
    {
        if (string.IsNullOrWhiteSpace(_fileName))
            return;

        var dir = ToNVRAMDir();
        var path = System.IO.Path.Combine(dir, _fileName);
        try
        {
            System.IO.Directory.CreateDirectory(dir);
            using var fs = System.IO.File.Open(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            using var bw = new System.IO.BinaryWriter(fs, System.Text.Encoding.UTF8, leaveOpen: true);
            bw.Write(NVRAM);
            bw.Flush();
        }
        catch
        {
        }
    }

    static string ToNVRAMDir()
        => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800", "nvram");

    #endregion
}
