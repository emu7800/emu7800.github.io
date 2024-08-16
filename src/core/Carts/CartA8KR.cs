namespace EMU7800.Core;

/// <summary>
/// Atari standard 8KB bankswitched carts with 128 bytes of RAM
/// </summary>
public sealed class CartA8KR : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
    // Bank2: 0x1000:0x1000
    //                            Shadows ROM
    //                            0x1000:0x0080  RAM write port
    //                            0x1080:0x0080  RAM read port
    //
    ushort BankBaseAddr;
    readonly byte[] RAM;

    #region IDevice Members

    public override void Reset()
    {
        BankBaseAddr = GetBankBaseAddr(1);
    }

    public override byte this[ushort addr]
    {
        get
        {
            addr &= 0xfff;
            if (addr is < 0x100 and >= 0x80)
            {
                return RAM[addr & 0x7f];
            }
            UpdateBank(addr);
            return ROM[BankBaseAddr + addr];
        }
        set
        {
            addr &= 0xfff;
            if (addr < 0x80)
            {
                RAM[addr & 0x7f] = value;
                return;
            }
            UpdateBank(addr);
        }
    }

    #endregion

    public override string ToString()
        => "EMU7800.Core.CartA8KR";

    public CartA8KR(byte[] romBytes)
    {
        LoadRom(romBytes, 0x2000);
        BankBaseAddr = GetBankBaseAddr(1);
        RAM = new byte[0x80];
    }

    void UpdateBank(ushort addr)
    {
        if (addr is >= 0xff8 and <= 0xff9)
        {
            BankBaseAddr = GetBankBaseAddr(addr - 0xff8);
        }
    }

    static ushort GetBankBaseAddr(int bankNo)
      => (ushort)(bankNo << 12);  // Multiply by 4096

    #region Serialization Members

    public CartA8KR(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x2000), 0x2000);
        RAM = input.ReadExpectedBytes(0x80);
        BankBaseAddr = input.ReadUInt16();
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);

        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(RAM);
        output.Write(BankBaseAddr);
    }

    #endregion
}