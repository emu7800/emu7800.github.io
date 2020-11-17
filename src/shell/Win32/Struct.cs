// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public static class Struct
    {
        public static PointF ToPointF(float x, float y)
        {
            var point = new PointF { X = x, Y = y };
            return point;
        }

        public static SizeF ToSizeF(float width, float height)
        {
            var size = new SizeF { Width = width, Height = height };
            return size;
        }

        public static SizeU ToSizeU(uint width, uint height)
        {
            var size = new SizeU { Width = width, Height = height };
            return size;
        }

        public static RectF ToRectF(PointF point, SizeF size)
        {
            var rect = new RectF { Left = point.X, Top = point.Y, Right = point.X + size.Width, Bottom = point.Y + size.Height };
            return rect;
        }

        public static PointF ToLocation(RectF rect)
        {
            var point = new PointF { X = rect.Left, Y = rect.Top };
            return point;
        }

        public static SizeF ToSize(RectF rect)
        {
            var size = new SizeF { Width = rect.Right - rect.Left, Height = rect.Bottom - rect.Top };
            return size;
        }

        public static PointF ToRightOf(ControlBase control, int dx, int dy)
        {
            var point = ToPointF(control.Location.X + control.Size.Width + dx, control.Location.Y + dy);
            return point;
        }

        public static PointF ToBottomOf(ControlBase control, int dx, int dy)
        {
            var point = ToPointF(control.Location.X + dx, control.Location.Y + control.Size.Height + dy);
            return point;
        }
    }
}
