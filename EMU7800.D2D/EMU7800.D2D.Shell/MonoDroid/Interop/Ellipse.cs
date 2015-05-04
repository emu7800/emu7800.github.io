// © Mike Murphy

using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Ellipse : DrawableShape
    {
        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();
            using (var paint = new Paint())
            {
                paint.SetStyle(Paint.Style.Fill);
                paint.Color = ToColor(Brush);
                var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
                Canvas.DrawOval(rect, paint);
            }
        }
        public Ellipse(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}