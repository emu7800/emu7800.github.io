// © Mike Murphy

namespace EMU7800.Shell;

public record ScrollColumnInfo(GameProgramInfoViewItemCollectionEx GameProgramInfoViewItemCollection)
{
    public RectF CollectionRect { get; set; }
    public float ScrollYAcceleration { get; set; }
    public float ScrollYTopMostBoundary { get; set; }
    public float ScrollYBottomMostBoundary { get; set; }
}