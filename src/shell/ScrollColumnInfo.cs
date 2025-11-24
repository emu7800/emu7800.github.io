// © Mike Murphy

using EMU7800.Shell;

namespace EMU7800.Shell;

public record ScrollColumnInfo
{
    public RectF CollectionRect { get; set; }
    public float ScrollYAcceleration { get; set; }
    public float ScrollYTopMostBoundary { get; set; }
    public float ScrollYBottomMostBoundary { get; set; }
    public GameProgramInfoViewItemCollectionEx GameProgramInfoViewItemCollection { get; init; } = new();
}