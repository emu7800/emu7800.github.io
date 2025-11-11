namespace EMU7800.Core;

/// <summary>
/// Atari standard 16KB bankswitched carts with 128 bytes of RAM
/// </summary>
public sealed class CartA16KR : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff9-0x1ff9
    // Bank2: 0x1000:0x1000
    // Bank3: 0x2000:0x1000
    // Bank4: 0x3000:0x1000
    //                            Shadows ROM
    //                            0x1000:0x0080  RAM write port
    //                            0x1080:0x0080  RAM read port
    //
    ushort BankBaseAddr;
    readonly byte[] RAM = new byte[0x80];

    #region IDevice Members

    public override void Reset()
    {
        BankBaseAddr = GetBankBaseAddr(0);
    }

    public override byte this[ushort addr]
    {
        get
        {
            addr &= 0xfff;
            if (addr is < 0x0100 and >= 0x0080)
            {
                return RAM[addr & 0x7f];
            }
            UpdateBank(addr);
            return ROM[BankBaseAddr + addr];
        }
        set
        {
            addr &= 0xfff;
            if (addr < 0x0080)
            {
                RAM[addr & 0x7f] = value;
                return;
            }
            UpdateBank(addr);
        }
    }

    #endregion

    public CartA16KR(byte[] romBytes)
    {
        LoadRom(romBytes, 0x4000);
        BankBaseAddr = GetBankBaseAddr(0);
    }

    void UpdateBank(ushort addr)
    {
        if (addr is >= 0xff6 and <= 0xff9)
        {
            BankBaseAddr = GetBankBaseAddr(addr - 0xff6);
        }
    }

    static ushort GetBankBaseAddr(int bankNo)
      => (ushort)(bankNo << 12);  // Multiply by 4096

    #region Serialization Members

    public CartA16KR(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x4000), 0x4000);
        BankBaseAddr = input.ReadUInt16();
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);

        output.WriteVersion(1);
        output.Write(ROM);
        output.Write(BankBaseAddr);
    }

    #endregion
}