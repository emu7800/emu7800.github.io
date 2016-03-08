// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public class ScrollColumnInfo
    {
        public RectF CollectionRect { get; set; }
        public float ScrollYAcceleration { get; set; }
        public float ScrollYTopMostBoundary { get; set; }
        public float ScrollYBottomMostBoundary { get; set; }
        public GameProgramInfoViewItemCollectionEx GameProgramInfoViewItemCollection { get; set; }
    }
}