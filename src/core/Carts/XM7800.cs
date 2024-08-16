/*
 * XM7800.cs
 *
 * The 7800 eXpansion Module
 *
 *   POKEY1     $0450-$045F    16 bytes
 *   POKEY2     $0460-$046F    16 bytes
 *   YM2151     $0460-$0461     2 bytes
 *   XCTRL      $0470-$047F     1 byte  YXXPMBBB  Y=ym2151 enable, X=NA, P=pokey enable, M=ram enable, B=ram bankno
 *   2KB NVRAM  $1000-$17ff
 *   4KB ROM    $3000-$3fff
 *   RAM        $4000-$7FFF  16KB bank size
 *
 */
namespace EMU7800.Core;

public sealed class XM7800 : Cart
{
    readonly NVRAM2k NVRAM;
    readonly byte[] RAM;
    readonly Cart Cart = Default;

    PokeySound _pokeySound = PokeySound.Default;
    //PokeySound _pokeySound2 = PokeySound.Default;
    YM2151 _ym2151 = YM2151.Default;

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
        ROM_MASK      = ROM_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        NVRAM.Reset();
        XCTRL = 0;
        Cart.Reset();
        _pokeySound.Reset();
        //_pokeySound2.Reset();
        _ym2151.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xf000) switch
        {
            0x0000 => (addr & 0x04f0) switch
            {
                0x0450 => _pokeySound.Read(addr),
                0x0460 => YmEnabled ? _ym2151.Read(addr) : /* _pokeySound2.Read(addr) */ (byte)0xff,
                0x0470 => XCTRL,
                _      => 0xff
            },
            0x1000 => NVRAM[addr],
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
                            if (PokeyEnabled) { _pokeySound.Update(addr, value); }
                            break;
                        case 0x0460:
                            if (YmEnabled) { _ym2151.Update(addr, value); } else if (PokeyEnabled) { /* _pokeySound2.Update(addr, value) */ }
                            break;
                        case 0x0470:
                            XCTRL = value;
                            break;
                    }
                    break;
                case 0x1000:
                    NVRAM[addr] = value;
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
        Cart.Attach(m);
        _pokeySound = new(m);
        //_pokeySound2 = new(m);
        _ym2151 = new(m);
    }

    public override void StartFrame()
    {
        if (PokeyEnabled)
        {
            _pokeySound.StartFrame();
          //_pokeySound2.StartFrame();
        }
        if (YmEnabled)
        {
            _ym2151.StartFrame();
        }
    }

    public override void EndFrame()
    {
        if (PokeyEnabled)
        {
            _pokeySound.EndFrame();
          //_pokeySound2.EndFrame();
        }
        if (YmEnabled)
        {
            _ym2151.EndFrame();
        }
    }

    public override bool Map()
    {
        M.Mem.Map(0x0440, 0x40, this);
        M.Mem.Map(0x1000, 0x800, this);
        M.Mem.Map(0x3000, 0x1000, this);
        if (!M.Mem.Map(Cart))
        {
            M.Mem.Map(0x4000, 0xc000, Cart);
        }
        return true;
    }

    #region Constructors

    XM7800()
    {
        ROM = new byte[ROM_SIZE];
        RAM = new byte[RAM_BANKSIZE * 8];
        NVRAM = new NVRAM2k("XM.bin");
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
        NVRAM = input.ReadNVRAM2k();
        Cart = input.ReadCart(m);
        _pokeySound = input.ReadOptionalPokeySound(m);
        //_pokeySound2 = input.ReadOptionalPokeySound(m);
        _ym2151 = input.ReadOptionalYM2151(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(RAM);
        output.Write(NVRAM);
        output.Write(Cart);
        output.WriteOptional(_pokeySound);
        //output.WriteOptional(_pokeySound2);
        output.WriteOptional(_ym2151);
    }

    #endregion
}