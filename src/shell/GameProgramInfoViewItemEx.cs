// © Mike Murphy

using EMU7800.Services.Dto;

namespace EMU7800.Shell;

public record GameProgramInfoViewItemEx(string Title, string SubTitle, ImportedGameProgramInfo ImportedGameProgramInfo)
{
    public TextLayout TitleTextLayout { get; set;} = TextLayout.Empty;
    public TextLayout SubTitleTextLayout { get; set; } = TextLayout.Empty;
}
