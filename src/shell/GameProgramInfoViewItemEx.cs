// © Mike Murphy

using EMU7800.D2D.Interop;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public class GameProgramInfoViewItemEx
    {
        public string Title { get; set; } = string.Empty;
        public TextLayout TitleTextLayout { get; set; } = ControlBase.TextLayoutDefault;
        public string SubTitle { get; set; } = string.Empty;
        public TextLayout SubTitleTextLayout { get; set; } = ControlBase.TextLayoutDefault;
        public ImportedGameProgramInfo ImportedGameProgramInfo { get; set; } = new();
    }
}
