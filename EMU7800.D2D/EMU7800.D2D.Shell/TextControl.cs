﻿// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class TextControl : ControlBase
    {
        #region Fields

        TextLayout _textLayout = TextLayoutDefault;
        int _isMouseDownByPointerId;
        RectF _bounds;
        int _startY, _maxStartY;
        float _scrollbarLength, _scrollbarY;

        string _text = string.Empty;
        string _textFontFamily = Styles.NormalFontFamily;
        int _textFontSize = Styles.NormalFontSize;

        #endregion

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                SafeDispose(ref _textLayout);
            }
        }

        public string TextFontFamilyName
        {
            get => _textFontFamily;
            set
            {
                if (_textFontFamily == value)
                    return;
                _textFontFamily = value;
                SafeDispose(ref _textLayout);
            }
        }

        public int TextFontSize
        {
            get => _textFontSize;
            set
            {
                if (_textFontSize == value)
                    return;
                _textFontSize = value;
                SafeDispose(ref _textLayout);
            }
        }

        public void ScrollUpByIncrement(int increment)
        {
            _startY += 10 * increment;
        }

        public void ScrollDownByIncrement(int increment)
        {
            _startY -= 10 * increment;
        }

        #region ControlBase Overrides

        public override void LocationChanged()
        {
            base.LocationChanged();
            _bounds = Struct.ToRectF(Location, Size);
        }

        public override void SizeChanged()
        {
            base.SizeChanged();
            _bounds = Struct.ToRectF(Location, Size);
            SafeDispose(ref _textLayout);
        }

        public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
        {
            if (down && _isMouseDownByPointerId >= 0
                || !down && _isMouseDownByPointerId < 0
                    || !down && _isMouseDownByPointerId != pointerId)
                return;

            _isMouseDownByPointerId = down ? pointerId : -1;
        }

        public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
        {
            if (_isMouseDownByPointerId < 0)
                return;
            if (!IsInBounds(x, y, _bounds))
                return;
            if (_isMouseDownByPointerId == pointerId)
                _startY += dy;
        }

        public override void MouseWheelChanged(int pointerId, int x, int y, int delta)
        {
            if (_isMouseDownByPointerId >= 0)
                return;
            _startY += delta / 10;
        }

        public override void Update(TimerDevice td)
        {
            if (_textLayout == TextLayoutDefault)
                return;

            if (_startY > 0)
                _startY = 0;
            else if (_startY < _maxStartY)
                _startY = _maxStartY;

            _scrollbarLength = (Size.Height / (float)_textLayout.Height) * Size.Height;
            _scrollbarY = (_maxStartY != 0)
                ? (Size.Height - _scrollbarLength) * (1.0f + ((float)_startY / _maxStartY - 1.0f))
                : -1;
        }

        public override void Render(GraphicsDevice gd)
        {
            if (_textLayout == TextLayoutDefault)
            {
                _textLayout = gd.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, int.MaxValue);
                _maxStartY = (int)(Size.Height - _textLayout.Height);
                if (_maxStartY >= 0)
                    _maxStartY = 0;
            }

            gd.PushAxisAlignedClip(_bounds, D2DAntiAliasMode.PerPrimitive);
            gd.DrawText(_textLayout, Struct.ToPointF(Location.X, Location.Y + _startY), D2DSolidColorBrush.White);
            gd.PopAxisAlignedClip();

            if (_scrollbarY >= 0.0f)
                gd.DrawLine(
                    Struct.ToPointF(Location.X + Size.Width + 1, Location.Y + _scrollbarY),
                    Struct.ToPointF(Location.X + Size.Width + 1, Location.Y + _scrollbarY + _scrollbarLength),
                    1.0f,
                    D2DSolidColorBrush.White);
        }

        protected override void DisposeResources()
        {
            SafeDispose(ref _textLayout);
            base.DisposeResources();
        }

        #endregion
    }
}
