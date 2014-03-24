namespace EMU7800.SoundEmulator
{
    public static class Constants
    {
        public const int
            TIA_AUDC0     = 0x15, // Write: audio control 0 (D3-0)
            TIA_AUDC1     = 0x16, // Write: audio control 1 (D4-0)
            TIA_AUDF0     = 0x17, // Write: audio frequency 0 (D4-0)
            TIA_AUDF1     = 0x18, // Write: audio frequency 1 (D3-0)
            TIA_AUDV0     = 0x19, // Write: audio volume 0 (D3-0)
            TIA_AUDV1     = 0x1a, // Write: audio volume 1 (D3-0)
            POKEY_AUDF1   = 0x00, // write reg: channel 1 frequency
            POKEY_AUDC1   = 0x01, // write reg: channel 1 generator
            POKEY_AUDF2   = 0x02, // write reg: channel 2 frequency
            POKEY_AUDC2   = 0x03, // write reg: channel 2 generator
            POKEY_AUDF3   = 0x04, // write reg: channel 3 frequency
            POKEY_AUDC3   = 0x05, // write reg: channel 3 generator
            POKEY_AUDF4   = 0x06, // write reg: channel 4 frequency
            POKEY_AUDC4   = 0x07, // write reg: channel 4 generator
            POKEY_AUDCTL  = 0x08; // write reg: control over audio channels
    }
}