using EMU7800.Core;

namespace EMU7800.SoundEmulator;

internal sealed class TIASoundDeviceWrapper(MachineBase m) : IDevice
{
    readonly TIASound _tiaSound = new(m, 57);

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
}