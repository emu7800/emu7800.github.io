namespace EMU7800.Core;

/// <summary>
/// Atari 7800 Bankset Bankswitched cartridge
///
/// The Bankset bankswitching scheme is a 7800 homebrew-era invention.
/// The Bankset scheme was created by Fred Quimby, in collaboration with Mike Saarna.
/// For more details, see http://7800.8bitdev.org/index.php/Bankset_Bankswitching.
/// </summary>
public abstract class Cart78BB : Cart
{
    protected byte[] RAM { get; private set; } = [];

    protected Cart78BB() {}
    protected Cart78BB(DeserializationContext input) : base(input) {}

    protected new void LoadRom(byte[] romBytes, int romSize)
    {
        if (romBytes.Length > romSize || romSize != 0x20000 && romSize != 0x40000)
        {
            throw new Emu7800Exception("Unexpected Cart78BB ROM sizing");
        }
        ROM = new byte[romSize];
        var romBytesHalfSize = romBytes.Length >> 1;
        var romHalfSize = romSize >> 1;
        var offset = romHalfSize - romBytesHalfSize;
        System.Buffer.BlockCopy(romBytes, 0, ROM, offset, romBytesHalfSize);
        System.Buffer.BlockCopy(romBytes, romBytesHalfSize, ROM, romHalfSize | offset, romBytesHalfSize);
    }

    protected new void LoadRom(byte[] romBytes)
    {
        ROM = new byte[romBytes.Length];
        System.Buffer.BlockCopy(romBytes, 0, ROM, 0, romBytes.Length);
    }

    protected void LoadRam(byte[] ramBytes)
    {
        RAM = new byte[ramBytes.Length];
        System.Buffer.BlockCopy(ramBytes, 0, RAM, 0, ramBytes.Length);
    }

    protected void InitRam(int size)
    {
        RAM = new byte[size];
    }
}