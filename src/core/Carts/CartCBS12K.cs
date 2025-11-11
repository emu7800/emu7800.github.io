namespace EMU7800.Core;

/// <summary>
/// CBS RAM Plus 12KB bankswitched carts with 128 bytes of RAM.
/// </summary>
public sealed class CartCBS12K : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank1: 0x0000:0x1000       Bank1:0x1000:0x1000  Select Segment: 0ff8-0ffa
    // Bank2: 0x1000:0x1000
    // Bank3: 0x2000:0x1000
    //                            Shadows ROM
    //                            0x1000:0x80 RAM write port
    //                            0x1080:0x80 RAM read port
    //
    ushort BankBaseAddr;
    readonly byte[] RAM;

    #region IDevice Members

    public override void Reset()
    {
        BankBaseAddr = GetBankBaseAddr(2);
    }

    public override byte this[ushort addr]
    {
        get
        {
            addr &= 0xfff;
            if (addr is < 0x0200 and >= 0x0100)
            {
                return RAM[addr & 0xff];
            }
            UpdateBank(addr);
            return ROM[BankBaseAddr + addr];
        }
        set
        {
            addr &= 0xfff;
            if (addr < 0x0100)
            {
                RAM[addr & 0xff] = value;
                return;
            }
            UpdateBank(addr);
        }
    }

    #endregion

    public CartCBS12K(byte[] romBytes)
    {
        LoadRom(romBytes, 0x3000);
        BankBaseAddr = GetBankBaseAddr(2);
        RAM = new byte[0x100];
    }

    void UpdateBank(ushort addr)
    {
        if (addr is >= 0xff8 and <= 0xffa)
        {
            BankBaseAddr = GetBankBaseAddr(addr - 0xff8);
        }
    }

    static ushort GetBankBaseAddr(int bankNo)
      => (ushort)(bankNo << 12);  // Multiply by 4096

    #region Serialization Members

    public CartCBS12K(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x3000), 0x3000);
        RAM = input.ReadExpectedBytes(0x100);
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