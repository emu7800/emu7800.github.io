namespace EMU7800.Core;

/// <summary>
/// Atari standard 32KB bankswitched carts
/// </summary>
public sealed class CartA32K : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x0ff4-0x0ffc
    // Bank2: 0x1000:0x1000
    // Bank3: 0x2000:0x1000
    // Bank4: 0x3000:0x1000
    // Bank5: 0x4000:0x1000
    // Bank6: 0x5000:0x1000
    // Bank7: 0x6000:0x1000
    // Bank8: 0x7000:0x1000
    //
    ushort BankBaseAddr;

    #region IDevice Members

    public override void Reset()
    {
        BankBaseAddr = GetBankBaseAddr(7);
    }

    public override byte this[ushort addr]
    {
        get
        {
            addr &= 0xfff;
            UpdateBank(addr);
            return ROM[BankBaseAddr + addr];
        }
        set
        {
            addr &= 0xfff;
            UpdateBank(addr);
        }
    }

    #endregion

    public CartA32K(byte[] romBytes)
    {
        LoadRom(romBytes, 0x8000);
        BankBaseAddr = GetBankBaseAddr(7);
    }

    void UpdateBank(ushort addr)
    {
        if (addr is >= 0x0ff4 and < 0x0ffc)
        {
            BankBaseAddr = GetBankBaseAddr(addr - 0xff4);
        }
    }

    static ushort GetBankBaseAddr(int bankNo)
      => (ushort)(bankNo << 12);  // Multiply by 4096

    #region Serialization Members

    public CartA32K(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x8000), 0x8000);
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