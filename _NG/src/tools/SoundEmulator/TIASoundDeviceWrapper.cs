using EMU7800.Core;

namespace EMU7800.SoundEmulator
{
    internal sealed class TIASoundDeviceWrapper : IDevice
    {
        readonly TIASound _tiaSound;

        public void Reset()
        {
            _tiaSound.Reset();
        }

        public byte this[ushort addr]
        {
            get { return 0; }
            set
            {
                addr &= 0x1f;
                _tiaSound.Update(addr, value);
            }
        }

        public void StartFrame()
        {
            _tiaSound.StartFrame();
        }

        public void EndFrame()
        {
            _tiaSound.EndFrame();
        }

        public TIASoundDeviceWrapper(MachineBase m)
        {
            _tiaSound = new TIASound(m, 57);
        }
    }
}