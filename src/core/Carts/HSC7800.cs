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
    readonly NVRAM2k NVRAM;
    readonly Cart Cart = Default;

    #region IDevice Members

    const int
        ROM_SHIFT = 12, // 4 KB rom size
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        NVRAM.Reset();
        Cart.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x1000 => NVRAM[addr],
            0x3000 => ROM[addr & ROM_MASK],
            _      => Cart[addr]
        };
        set => NVRAM[addr] = value;
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
        NVRAM = new NVRAM2k("HSC.bin");
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
        NVRAM = input.ReadNVRAM2k();
        Cart = input.ReadCart(M);
    }

    public override void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(NVRAM);
        output.Write(Cart);
    }

    #endregion
}