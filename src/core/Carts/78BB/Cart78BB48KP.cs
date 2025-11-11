namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge - 2x48K w/Pokey@4000
/// </summary>
public sealed class Cart78BB48KP : Cart78BB
{
    // Address Space    Cart/Device
    // 0x4000:0x000f    0x0000:0x000f Pokey writable
    // 0x4000:0xc000    0x0000:0xc000 ROM CPU readable
    // 0x4000:0xc000    0xc000:0xc000 ROM Maria readable

    PokeySound _pokeySound = PokeySound.Default;

    #region IDevice Members

    const int
        ROM_SHIFT = 17,
        ROM_SIZE  = 1 << ROM_SHIFT // 128 KB, 0x20000
        ;

    public override void Reset()
    {
        base.Reset();
        _pokeySound.Reset();
    }

    public override byte this[ushort addr]
    {
        get => ROM[(M.Mem.MariaRead << (ROM_SHIFT - 1)) | addr];
        set
        {
            if ((addr & 0xf000) == 0x4000)
            {
                _pokeySound.Update(addr, value);
            }
        }
    }

    #endregion

    public override void Attach(MachineBase m)
    {
        base.Attach(m);
        _pokeySound = new(M);
    }

    public override void StartFrame()
        => _pokeySound.StartFrame();

    public override void EndFrame()
        => _pokeySound.EndFrame();

    public Cart78BB48KP(byte[] romBytes)
    {
        LoadRom(romBytes, ROM_SIZE);
    }

    #region Serialization Members

    public Cart78BB48KP(DeserializationContext input, MachineBase m) : base(input)
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