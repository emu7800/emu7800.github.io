namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge - 2x128K w/Pokey@4000
/// </summary>
public sealed class Cart78BB128KP : Cart78BB
{
    // Address Space   Cart/Device
    // 0x4000:0x400f    0x0000:0x000f Pokey
    // 0x8000:0x4000   0xac000:0x4000 ROM CPU readable - 16kb bank 0-7 (0 on startup)  - a:{0, 1}, c:{0, 4, 8, C}
    // 0x8000:0x4000   0xbc000:0x4000 ROM Maria readable - 16kb bank 0-7 (0 on startup) - b:{2, 3}, c:{0, 4, 8, C}
    // 0xC000:0x4000   0x1C000:0x4000 ROM CPU readable - 16kb bank 7
    // 0xC000:0x4000   0x3C000:0x4000 ROM Maria readable - 16kb bank 7

    readonly int[] Bank = new[] { 0, 6, 0, 7 };

    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT     = 18,
        ROM_SIZE      = 1 << ROM_SHIFT,     // 256 KB, 0x40000
        ROMBANK_SHIFT = 14,
        ROMBANK_SIZE  = 1 << ROMBANK_SHIFT, //  16 KB, 0x4000
        ROMBANK_MASK  = ROMBANK_SIZE - 1
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
            var bankNo = addr >> ROMBANK_SHIFT;
            return bankNo switch
            {
                1 => _pokeySound.Read(addr),
                _ => ROM[M.Mem.MariaRead << (ROM_SHIFT - 1) | Bank[bankNo] << ROMBANK_SHIFT | addr & ROMBANK_MASK]
            };
        }
        set
        {
            var bankNo = addr >> ROMBANK_SHIFT;
            switch (bankNo)
            {
                case 1:
                    _pokeySound.Update(addr, value);
                    break;
                case 3:
                    Bank[2] = value & 7;
                    break;
            }
        }
    }

    #endregion

    public override string ToString()
        => GetType().FullName ?? string.Empty;

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        _pokeySound = new PokeySound(M);
    }

    public override void StartFrame()
        => _pokeySound.StartFrame();

    public override void EndFrame()
        => _pokeySound.EndFrame();

    public Cart78BB128KP(byte[] romBytes)
    {
        LoadRom(romBytes, ROM_SIZE);
    }

    #region Serialization Members

    public Cart78BB128KP(DeserializationContext input, MachineBase m) : base(input)
    {
        _ = input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        _pokeySound = input.ReadOptionalPokeySound(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.WriteOptional(_pokeySound);
    }

    #endregion
}