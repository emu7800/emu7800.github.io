using EMU7800.Core;

namespace EMU7800.SoundEmulator
{
    internal sealed class MachineSoundEmulator : MachineBase
    {
        const ushort
            TIA_BASE   = 0x0000,
            POKEY_BASE = 0x4000;

        readonly TIASoundDeviceWrapper _tiaSoundDevice;
        readonly PokeySoundDeviceWrapper _pokeySoundDevice;

        public void PokeTia(byte tiaRegister, byte value)
        {
            Mem[(ushort)(TIA_BASE + tiaRegister)] = value;
        }

        public void PokePokey(byte pokeyRegister, byte value)
        {
            Mem[(ushort)(POKEY_BASE + pokeyRegister)] = value;
        }

        public override void ComputeNextFrame(FrameBuffer frameBuffer)
        {
            base.ComputeNextFrame(frameBuffer);
            _tiaSoundDevice.StartFrame();
            _pokeySoundDevice.StartFrame();
            _tiaSoundDevice.EndFrame();
            _pokeySoundDevice.EndFrame();
        }

        MachineSoundEmulator(int freq, int scanlines) : base(null, scanlines, 0, 0, freq, null, 0)
        {
            _tiaSoundDevice = new TIASoundDeviceWrapper(this);
            _pokeySoundDevice = new PokeySoundDeviceWrapper(this);

            Mem = new AddressSpace(this, 16, 6);
            CPU = new M6502(this, 4) { Jammed = true };
            Mem.Map(TIA_BASE, 0x40, _tiaSoundDevice);
            Mem.Map(POKEY_BASE, 0x0040, _pokeySoundDevice);
        }

        public static MachineSoundEmulator CreateForNTSC()
        {
            var m = new MachineSoundEmulator(31440, 262);
            return m;
        }

        public static MachineSoundEmulator CreateForPAL()
        {
            var m = new MachineSoundEmulator(31200, 312);
            return m;
        }
    }
}
