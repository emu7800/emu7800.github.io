/*
 * XM7800.cs
 *
 * The 7800 eXpansion Module cartridge
 *
 *   POKEY1            $0450-$045F       16 bytes
 *   POKEY2*           $0460-$046F       16 bytes
 *   XCTRL             $0470-$047F        1 byte  XXXPMBBB  X=NA, P=pokey enable, M=ram enable, B=ram bankno
 *   2KB NVRAM         $1000-$17ff
 *   4KB ROM           $3000-$3fff
 *   RAM               $4000-$7FFF    16384 bytes bank
 *
 */
namespace EMU7800.Core;

public sealed class XM7800 : Cart
{
    static readonly byte[] NVRAM = new byte[NVRAM_SIZE];
    readonly byte[] RAM;
    readonly Cart Cart = Default;
    PokeySound Pokey1 = PokeySound.Default;
    PokeySound Pokey2 = PokeySound.Default;

    byte XCTRL;
    int BankNo => XCTRL & 7;
    bool RamEnabled => (XCTRL & 8) != 0;
    bool PokeyEnabled => (XCTRL & 0x10) != 0;
    bool YmEnabled => (XCTRL & 0x84) != 0;

    #region IDevice

    const int
        RAM_BANKSHIFT = 14, // 16 KB bank size
        RAM_BANKSIZE  = 1 << RAM_BANKSHIFT,
        RAM_BANKMASK  = RAM_BANKSIZE - 1,
        ROM_SHIFT     = 12, // 4 KB rom size
        ROM_SIZE      = 1 << ROM_SHIFT,
        ROM_MASK      = ROM_SIZE - 1,
        NVRAM_SHIFT   = 11, // 2 KB nvram size
        NVRAM_SIZE    = 1 << NVRAM_SHIFT,
        NVRAM_MASK    = NVRAM_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        XCTRL = 0;
        Cart.Reset();
        Pokey1.Reset();
        Pokey2.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x0000 => (addr & 0x04f0) switch
            {
                0x0450 => Pokey1.Read(addr),
                0x0460 => YmEnabled ? /* YM2151.Read(addr & 1) */ (byte)0xff : /* Pokey2.Read(addr) */ (byte)0xff,
                0x0470 => XCTRL,
                _      => 0xff,
            },
            0x1000 => NVRAM[addr & NVRAM_MASK],
            0x3000 => ROM[addr & ROM_MASK],
            _      => RamEnabled ? RAM[BankNo << RAM_BANKSHIFT | (addr & RAM_BANKMASK)] : Cart[addr]
        };
        set
        {
            switch (addr & 0xf000)
            {
                case 0x0000:
                    switch (addr & 0x4f0)
                    {
                        case 0x0450:
                            if (PokeyEnabled) { Pokey1.Update(addr, value); }
                            break;
                        case 0x0460:
                            if (YmEnabled) { /* YM2151.Update(addr & 1, value( */ } else if (PokeyEnabled) { /* Pokey2.Update(addr, value) */ }
                            break;
                        case 0x0470:
                            XCTRL = value;
                            break;
                    }
                    break;
                case 0x1000:
                    NVRAM[addr & NVRAM_MASK] = value;
                    break;
                default:
                    if (RamEnabled)
                    {
                        RAM[BankNo << RAM_BANKSHIFT | (addr & RAM_BANKMASK)] = value;
                    }
                    else
                    {
                        Cart[addr] = value;
                    }
                    break;
            }
        }
    }

    #endregion

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        Pokey1 = new PokeySound(m);
        Pokey2 = new PokeySound(m);
    }

    public override void StartFrame()
    {
        if (PokeyEnabled)
        {
            Pokey1.StartFrame();
          //Pokey2.StartFrame();
        }
    }

    public override void EndFrame()
    {
        if (PokeyEnabled)
        {
            Pokey1.EndFrame();
          //Pokey2.EndFrame();
        }
    }

    public override bool Map()
    {
        M?.Mem.Map(0x0440, 0x40, this);
        M?.Mem.Map(0x1000, 0x800, this);
        M?.Mem.Map(0x3000, 0x1000, this);
        M?.Mem.Map(0x4000, 0xc000, this);
        return true;
    }

    #region Constructors

    XM7800()
    {
        ROM = new byte[ROM_SIZE];
        RAM = new byte[RAM_BANKSIZE * 8];
    }

    public XM7800(byte[] hscRom, Cart cart) : this()
    {
        LoadRom(hscRom, ROM_SIZE);
        Cart = cart;
    }

    #endregion

    #region Serialization Members

    public XM7800(DeserializationContext input, MachineBase m) : this()
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        RAM = input.ReadBytes();
        Cart = input.ReadCart(m);
        Pokey1 = input.ReadOptionalPokeySound(m);
        Pokey2 = input.ReadOptionalPokeySound(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(RAM);
        output.Write(Cart);
        output.WriteOptional(Pokey1);
        output.WriteOptional(Pokey2);
    }

    #endregion
}
