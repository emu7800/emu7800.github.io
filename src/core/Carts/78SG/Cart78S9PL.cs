namespace EMU7800.Core;

/// <summary>
/// Atari 7800 SuperGame S9 bankswitched cartridge w/Pokey at $0450
/// </summary>
public sealed class Cart78S9PL : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000      0x0450:0x045f  Pokey
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-8 (1 on startup)
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank8
    // Bank4: 0x10000:0x4000
    // Bank5: 0x14000:0x4000
    // Bank6: 0x18000:0x4000
    // Bank7: 0x1c000:0x4000
    // Bank8: 0x20000:0x4000
    //
    readonly int[] Bank = [0, 0, 1, 8];
    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT = 14,   // 16 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
        ;

    public override byte this[ushort addr]
    {
        get => (addr & 0xfff0) switch
        {
            0x0450 => _pokeySound.Read(addr),
            _      => ROM[(Bank[addr >> ROM_SHIFT] << ROM_SHIFT) | (addr & ROM_MASK)]
        };
        set
        {
            switch (addr & 0xfff0)
            {
                case 0x0450:
                    _pokeySound.Update(addr, value);
                    break;
                default:
                    if ((addr >> ROM_SHIFT) == 2)
                    {
                        Bank[2] = (value & 7) + 1;
                    }
                    break;
            }
        }
    }

    #endregion

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        _pokeySound = new PokeySound(m);
    }

    public override void StartFrame()
        => _pokeySound.StartFrame();

    public override void EndFrame()
        => _pokeySound.EndFrame();

    public override string ToString()
        => "EMU7800.Core." + nameof(Cart78S9PL);

    public override bool Map()
    {
        M?.Mem.Map(0x0440, 0x40, this);
        M?.Mem.Map(0x4000, 0xc000, this);
        return true;
    }

    public Cart78S9PL(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE * 9);

    #region Serialization Members

    public Cart78S9PL(DeserializationContext input, MachineBase m) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        Bank = input.ReadIntegers(4);
        _pokeySound = input.ReadOptionalPokeySound(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(Bank);
        output.WriteOptional(_pokeySound);
    }

    #endregion
}