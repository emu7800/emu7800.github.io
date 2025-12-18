// � Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto;

public record MachineStateInfo(
    MachineBase Machine,
    GameProgramInfo GameProgramInfo,
    bool SoundOff,
    int CurrentPlayerNo,
    int InterpolationMode)
{
    public readonly static MachineStateInfo Default = new(MachineBase.Default, GameProgramInfo.Default, true, 0, 0);
}