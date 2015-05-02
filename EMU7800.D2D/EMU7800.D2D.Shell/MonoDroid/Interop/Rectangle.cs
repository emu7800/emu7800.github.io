using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Rectangle : DrawableShape
    {
        protected override void RefreshBitmap(Canvas canvas, Paint paint)
        {
            base.RefreshBitmap(canvas, paint);
            var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
            canvas.DrawRect(rect, paint);
        }
        public Rectangle(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}