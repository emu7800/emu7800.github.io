using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Ellipse : DrawableShape
    {
        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();
            var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
            Canvas.DrawOval(rect, Paint);
        }
        public Ellipse(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}