namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge - 2x32K w/RAM@4000 w/Pokey@800
/// </summary>
public sealed class Cart78BB32KRPL : Cart78BB
{
    // Address Space    Cart/Device
    // 0x0800:0x000f    0x0000:0x000f Pokey
    // 0x4000:0x4000    0x0000:0x4000 RAM CPU readable
    // 0x4000:0x4000    0x4000:0x4000 RAM Maria readable
    // 0x8000:0x8000    0x0000:0x8000 ROM CPU readable
    // 0x8000:0x8000    0x8000:0x8000 ROM Maria readable
    // 0xC000:0x4000    0x4000:0x4000 RAM CPU writable

    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT     = 17,
        ROM_SIZE      = 1 << ROM_SHIFT,      // 128 KB, 0x20000
        RAMBANK_SHIFT = 14,
        RAMBANK_SIZE  = 1 << RAMBANK_SHIFT,  //  16 KB, 0x4000
        RAMBANK_MASK  = RAMBANK_SIZE - 1
        ;

    public override void Reset()
    {
        base.Reset();
        _pokeySound.Reset();
    }

    public override byte this[ushort addr]
    {
        get
        {
            if ((addr & 0xff00) == 0x0800)
            {
                return _pokeySound.Read(addr);
            }
            if (addr >= 0x4000 && addr < 0x8000)
            {
                return RAM[(M.Mem.MariaRead << RAMBANK_SHIFT) | (addr & RAMBANK_MASK)];
            }
            return ROM[(M.Mem.MariaRead << (ROM_SHIFT - 1)) | addr];
        }
        set
        {
            if ((addr & 0xff00) == 0x0800)
            {
                _pokeySound.Update(addr, value);
            }
            else if (addr >= 0x4000 && addr < 0x8000)
            {
                RAM[0 << RAMBANK_SHIFT | (addr & RAMBANK_MASK)] = value;
            }
            else if (addr >= 0xc000)
            {
                RAM[1 << RAMBANK_SHIFT | (addr & RAMBANK_MASK)] = value;
            }
        }
    }

    #endregion

    public override string ToString()
        => GetType().FullName ?? string.Empty;

    public override bool Map()
    {
        M?.Mem.Map(0x0800, 0x0f, this);
        M?.Mem.Map(0x4000, 0xc000, this);
        return true;
    }

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        _pokeySound = new PokeySound(M);
    }

    public override void StartFrame()
        => _pokeySound.StartFrame();

    public override void EndFrame()
        => _pokeySound.EndFrame();

    public Cart78BB32KRPL(byte[] romBytes)
    {
        LoadRom(romBytes, ROM_SIZE);
        InitRam(RAMBANK_SIZE << 1);
    }

    #region Serialization Members

    public Cart78BB32KRPL(DeserializationContext input, MachineBase m) : base(input)
    {
        _ = input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        LoadRam(input.ReadBytes());
        _pokeySound = input.ReadOptionalPokeySound(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(RAM);
        output.WriteOptional(_pokeySound);
    }

    #endregion
}