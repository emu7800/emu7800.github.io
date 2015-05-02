using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Line : DrawableShape
    {
        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();
            var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
            Canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, Paint);
        }
        public Line(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}