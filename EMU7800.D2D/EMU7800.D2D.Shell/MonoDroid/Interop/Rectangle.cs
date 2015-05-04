// © Mike Murphy

using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Rectangle : DrawableShape
    {
        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();
            using (var paint = new Paint())
            {
                paint.SetStyle(Style);
                paint.StrokeWidth = StrokeWidth;
                paint.Color = ToColor(Brush);
                var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
                Canvas.DrawRect(rect, paint);
            }
        }
        public Rectangle(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}