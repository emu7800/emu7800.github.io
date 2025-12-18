// © Mike Murphy

namespace EMU7800.Shell;

public record GameProgramInfoViewItemCollectionEx(string Name, GameProgramInfoViewItemEx[] GameProgramInfoViewItems)
{
    public TextLayout NameTextLayout { get; set; } = TextLayout.Empty;
}
