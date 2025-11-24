// © Mike Murphy

namespace EMU7800.Shell;

public class GameProgramInfoViewItemCollectionEx
{
    public string Name { get; set; } = string.Empty;
    public TextLayout NameTextLayout { get; set; } = TextLayout.Default;
    public GameProgramInfoViewItemEx[] GameProgramInfoViewItemSet { get; set; } = [];
}
