// � Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto;

public record MachineStateInfo
{
    public static readonly MachineStateInfo Default = new();

    public int FramesPerSecond { get; init; }
    public bool SoundOff { get; init; }
    public int CurrentPlayerNo { get; init; }
    public int InterpolationMode { get; init; }
    public MachineBase Machine { get; init; } = MachineBase.Default;
    public GameProgramInfo GameProgramInfo { get; init; } = new();
}