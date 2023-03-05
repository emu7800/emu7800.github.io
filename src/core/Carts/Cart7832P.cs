namespace EMU7800.Core;

/// <summary>
/// Atari 7800 non-bankswitched 32KB cartridge w/Pokey
/// </summary>
public sealed class Cart7832P : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    //                            0x4000:0x4000 Pokey
    // 0x0000:0x8000              0x8000:0x8000
    //
    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    public override void Reset()
    {
        base.Reset();
        _pokeySound.Reset();
    }

    public override byte this[ushort addr]
    {
        get => (addr & 0xfff0) == 0x4000 ? _pokeySound.Read(addr) : ROM[addr & 0x7fff];
        set
        {
            if ((addr & 0xfff0) == 0x4000)
                _pokeySound.Update(addr, value);
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
        => "EMU7800.Core.Cart7832P";

    public Cart7832P(byte[] romBytes)
        => LoadRom(romBytes, 0x8000);

    #region Serialization Members

    public Cart7832P(DeserializationContext input, MachineBase m) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x8000), 0x8000);
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