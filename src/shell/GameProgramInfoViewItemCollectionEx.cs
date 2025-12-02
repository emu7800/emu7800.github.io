// © Mike Murphy

namespace EMU7800.Shell;

public class GameProgramInfoViewItemCollectionEx
{
    public string Name { get; set; } = string.Empty;
    public TextLayout NameTextLayout { get; set; } = TextLayout.Empty;
    public GameProgramInfoViewItemEx[] GameProgramInfoViewItemSet { get; set; } = [];
}
