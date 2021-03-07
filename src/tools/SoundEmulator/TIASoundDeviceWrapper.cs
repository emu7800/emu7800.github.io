using EMU7800.Core;

namespace EMU7800.SoundEmulator
{
    internal sealed class TIASoundDeviceWrapper : IDevice
    {
        readonly TIASound _tiaSound;

        public void Reset()
            => _tiaSound.Reset();

        public byte this[ushort addr]
        {
            get => 0;
            set => _tiaSound.Update((ushort)(addr & 0x1f), value);
        }

        public void StartFrame()
            => _tiaSound.StartFrame();

        public void EndFrame()
            => _tiaSound.EndFrame();

        public TIASoundDeviceWrapper(MachineBase m)
            => _tiaSound = new TIASound(m, 57);
    }
}