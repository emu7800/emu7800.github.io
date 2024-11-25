﻿namespace EMU7800.Core;

/// <summary>
/// Atari standard 8KB bankswitched carts
/// </summary>
public sealed class CartA8K : Cart
{
    //
    // Cart Format                Mapping to ROM Address Space
    // Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
    // Bank2: 0x1000:0x1000
    //
    ushort BankBaseAddr;

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

    public override string ToString()
        => "EMU7800.Core.CartA8K";

    public CartA8K(byte[] romBytes)
    {
        LoadRom(romBytes, 0x2000);
        BankBaseAddr = GetBankBaseAddr(1);
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

    public CartA8K(DeserializationContext input) : base(input)
    {
        input.CheckVersion(1);
        LoadRom(input.ReadExpectedBytes(0x2000), 0x2000);
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