using System.Runtime.Intrinsics.Arm;

namespace EMU7800.Core;

/// <summary>
/// Atari 7800 non-bankswitched 32KB cartridge w/Pokey at $0450
/// </summary>
public sealed class Cart7832PL : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    //                            0x0450:0x045f Pokey
    // 0x0000:0x8000              0x8000:0x8000
    //
    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT = 15, // 32 KB rom size
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
        get => (addr & 0xfff0) switch
        {
            0x0450 => _pokeySound.Read(addr),
            _      => ROM[addr & ROM_MASK]
        };
        set
        {
            switch (addr & 0xfff0)
            {
                case 0x0450:
                    _pokeySound.Update(addr, value);
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
        => "EMU7800.Core." + nameof(Cart7832PL);

    public override bool Map()
    {
        M?.Mem.Map(0x0440, 0x40, this);
        M?.Mem.Map(0x4000, 0xc000, this);
        return true;
    }

    #region Constructors

    public Cart7832PL(byte[] romBytes)
        => LoadRom(romBytes, ROM_SIZE);

    #endregion

    #region Serialization Members

    public Cart7832PL(DeserializationContext input, MachineBase m) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(ROM_SIZE), ROM_SIZE);
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