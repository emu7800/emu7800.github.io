namespace EMU7800.Core;

/// <summary>
/// Atari 7800 SuperGame bankswitched cartridge w/Pokey at 4000
/// </summary>
public sealed class Cart78SGP : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank0: 0x00000:0x4000
    // Bank1: 0x04000:0x4000      0x4000:0x4000  Pokey
    // Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-7 (0 on startup)
    // Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank7
    // Bank4: 0x10000:0x4000
    // Bank5: 0x14000:0x4000
    // Bank6: 0x18000:0x4000
    // Bank7: 0x1c000:0x4000
    //
    readonly int[] _bank = new[] { 0, 0, 0, 7 };
    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT = 14,   // 16 KB, 0x4000
        ROM_SIZE  = 1 << ROM_SHIFT,
        ROM_MASK  = ROM_SIZE - 1
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
            var bankNo = addr >> ROM_SHIFT;
            return bankNo switch
            {
                1 => _pokeySound.Read(addr),
                _ => ROM[(_bank[bankNo] << ROM_SHIFT) | (addr & ROM_MASK)],
            };
        }
        set
        {
            var bankNo = addr >> ROM_SHIFT;
            switch (bankNo)
            {
                case 1:
                    _pokeySound.Update(addr, value);
                    break;
                case 2:
                    _bank[2] = value & 7;
                    break;
            }
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core." + nameof(Cart78SGP);

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        _pokeySound = new PokeySound(M);
    }

    public override void StartFrame()
        => _pokeySound.StartFrame();

    public override void EndFrame()
        => _pokeySound.EndFrame();

    public Cart78SGP(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE * 8);

    #region Serialization Members

    public Cart78SGP(DeserializationContext input, MachineBase m) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadBytes());
        _bank = input.ReadIntegers(4);
        _pokeySound = input.ReadOptionalPokeySound(m);
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);
        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(_bank);
        output.WriteOptional(_pokeySound);
    }

    #endregion
}