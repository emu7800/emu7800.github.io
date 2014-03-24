// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto
{
    public class MachineStateInfo
    {
        public int FramesPerSecond { get; set; }
        public bool SoundOff { get; set; }
        public int CurrentPlayerNo { get; set; }
        public int InterpolationMode { get; set; }
        public MachineBase Machine { get; set; }
        public GameProgramInfo GameProgramInfo { get; set; }
    }
}