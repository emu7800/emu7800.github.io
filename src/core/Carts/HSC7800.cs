/*
 * HSC7800.cs
 *
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
 *
 *   2KB NVRAM         $1000-$17ff
 *   4KB ROM           $3000-$3fff
 *
 */
namespace EMU7800.Core;

public sealed class HSC7800 : Cart
{
    readonly byte[] NVRAM = new byte[NVRAM_SIZE];
    readonly Cart Cart = Default;

    #region IDevice Members

    const int
        ROM_SHIFT   = 12, // 4 KB rom size
        ROM_SIZE    = 1 << ROM_SHIFT,
        ROM_MASK    = ROM_SIZE - 1,
        NVRAM_SHIFT = 11, // 2 KB nvram size
        NVRAM_SIZE  = 1 << NVRAM_SHIFT,
        NVRAM_MASK  = NVRAM_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        Cart.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x1000 => NVRAM[addr & NVRAM_MASK],
            0x3000 => ROM[addr & ROM_MASK],
            _      => Cart[addr]
        };
        set
        {
            NVRAM[addr & NVRAM_MASK] = value;
        }
    }

    #endregion

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        Cart.Attach(m);
    }

    public override void StartFrame()
        => Cart.StartFrame();

    public override void EndFrame()
        => Cart.EndFrame();

    public override string ToString()
        => "EMU7800.Core." + nameof(HSC7800);

    public override bool Map()
    {
        M?.Mem.Map(0x1000, 0x800, this);
        M?.Mem.Map(0x3000, 0x1000, this);
        if (M != null && !M.Mem.Map(Cart))
        {
            M?.Mem.Map(0x4000, 0xc000, Cart);
        }
        return true;
    }

    #region Constructors

    HSC7800()
    {
        ROM = new byte[ROM_SIZE];
        LoadNVRAM(GetHSCNVRAMPath());
    }

    public HSC7800(byte[] hscRom, Cart cart) : this()
    {
        LoadRom(hscRom, ROM_SIZE);
        Cart = cart;
    }

    #endregion

    #region Serialization Members

    public HSC7800(DeserializationContext input) : this()
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Cart = input.ReadCart(M);
    }

    public override void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(Cart);
        SaveNVRAM(GetHSCNVRAMPath());
    }

    #endregion

    void LoadNVRAM(string path)
    {
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

    void SaveNVRAM(string path)
    {
        try
        {
            using var fs = System.IO.File.Open(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            using var bw = new System.IO.BinaryWriter(fs, System.Text.Encoding.UTF8, leaveOpen: true);
            bw.Write(NVRAM);
            bw.Flush();
        }
        catch
        {
        }
    }

    static string GetHSCNVRAMPath()
        => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Saved Games", "EMU7800", "HSCnvram.bin");
}