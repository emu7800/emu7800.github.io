// © Mike Murphy

using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public record ScrollColumnInfo
{
    public D2D_RECT_F CollectionRect { get; set; }
    public float ScrollYAcceleration { get; set; }
    public float ScrollYTopMostBoundary { get; set; }
    public float ScrollYBottomMostBoundary { get; set; }
    public GameProgramInfoViewItemCollectionEx GameProgramInfoViewItemCollection { get; init; } = new();
}