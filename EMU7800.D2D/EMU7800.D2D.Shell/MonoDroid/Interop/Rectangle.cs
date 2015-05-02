using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Rectangle : DrawableShape
    {
        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();
            var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
            Canvas.DrawRect(rect, Paint);
        }
        public Rectangle(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}