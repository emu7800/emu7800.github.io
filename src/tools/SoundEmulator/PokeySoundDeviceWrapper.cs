using EMU7800.Core;

namespace EMU7800.SoundEmulator
{
    internal sealed class PokeySoundDeviceWrapper : IDevice
    {
        readonly PokeySound _pokeySound;

        public void Reset()
        {
            _pokeySound.Reset();
        }

        public byte this[ushort addr]
        {
            get { return 0; }
            set
            {
                addr &= 0xf;
                _pokeySound.Update(addr, value);
            }
        }

        public void StartFrame()
        {
            _pokeySound.StartFrame();
        }

        public void EndFrame()
        {
            _pokeySound.EndFrame();
        }

        public PokeySoundDeviceWrapper(MachineBase m)
        {
            _pokeySound = new PokeySound(m);
        }
    }
}