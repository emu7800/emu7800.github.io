// © Mike Murphy

using EMU7800.D2D.Interop;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public class GameProgramInfoViewItemEx
    {
        public string Title { get; set; }
        public TextLayout TitleTextLayout { get; set; }
        public string SubTitle { get; set; }
        public TextLayout SubTitleTextLayout { get; set; }
        public ImportedGameProgramInfo ImportedGameProgramInfo { get; set; }
    }
}
