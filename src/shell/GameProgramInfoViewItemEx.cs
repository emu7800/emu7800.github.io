// © Mike Murphy

using EMU7800.Services.Dto;

namespace EMU7800.Shell;

public class GameProgramInfoViewItemEx
{
    public string Title { get; set; } = string.Empty;
    public TextLayout TitleTextLayout { get; set; } = TextLayout.Default;
    public string SubTitle { get; set; } = string.Empty;
    public TextLayout SubTitleTextLayout { get; set; } = TextLayout.Default;
    public ImportedGameProgramInfo ImportedGameProgramInfo { get; set; } = new(new());
}