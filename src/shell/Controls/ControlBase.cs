// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public abstract class ControlBase : IDisposable
    {
        public static readonly ControlBase Default = new ControlDefault();
        public static readonly TextLayout TextLayoutDefault = new();
        public static readonly StaticBitmap StaticBitmapDefault = new();
        public static readonly DynamicBitmap DynamicBitmapDefault = new();

        #region Fields

        static int _nextIdToProvision;
        readonly int _id = _nextIdToProvision++;

        PointF _location;
        SizeF _size;

        #endregion

        public PointF Location
        {
            get => _location;
            set
            {
                _location = value;
                LocationChanged();
            }
        }

        public SizeF Size
        {
            get => _size;
            set
            {
                _size = value;
                SizeChanged();
            }
        }

        public bool IsMouseOver
        {
            get => IsMouseOverPointerId >= 0;
        }

        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }

        protected ControlBase()
        {
            IsVisible = true;
            IsEnabled = true;
        }

        #region ControlBase Virtuals

        public virtual void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
        }

        public virtual void MouseMoved(int pointerId, int x, int y, int dx, int dy)
        {
        }

        public virtual void MouseButtonChanged(int pointerId, int x, int y, bool down)
        {
        }

        public virtual void MouseWheelChanged(int pointerId, int x, int y, int delta)
        {
        }

        public virtual void LocationChanged()
        {
        }

        public virtual void SizeChanged()
        {
        }

        public virtual void LoadResources(GraphicsDevice gd)
        {
            DisposeResources();
            CreateResources(gd);
        }

        public virtual void Update(TimerDevice td)
        {
        }

        public virtual void Render(GraphicsDevice gd)
        {
        }

        protected virtual void CreateResources(GraphicsDevice gd)
        {
        }

        protected virtual void DisposeResources()
        {
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object? them)
            => them != null && _id == ((ControlBase)them)._id;

        public override int GetHashCode()
            => _id;

        public override string ToString()
            => $"EMU7800.D2D.Shell.ControlBase: ID={_id}";

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResources();
            }
        }

        #endregion

        protected int IsMouseOverPointerId { get; set; } = -1;

        protected static void SafeDispose(ref StaticBitmap bitmap)
        {
            if (bitmap == StaticBitmapDefault)
                return;
            bitmap.Dispose();
            bitmap = StaticBitmapDefault;
        }

        protected static void SafeDispose(ref DynamicBitmap bitmap)
        {
            if (bitmap == DynamicBitmapDefault)
                return;
            bitmap.Dispose();
            bitmap = DynamicBitmapDefault;
        }

        protected static void SafeDispose(ref TextLayout textLayout)
        {
            if (textLayout == TextLayoutDefault)
                return;
            textLayout.Dispose();
            textLayout = TextLayoutDefault;
        }

        protected static bool IsInBounds(int x, int y, RectF bounds)
        {
            var outOfBounds = x < bounds.Left
                || x > bounds.Right
                    || y < bounds.Top
                        || y > bounds.Bottom;
            return !outOfBounds;
        }

        class ControlDefault : ControlBase
        {
        }
    }
}
