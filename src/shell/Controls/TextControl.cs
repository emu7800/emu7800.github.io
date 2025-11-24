// © Mike Murphy

namespace EMU7800.Shell;

public sealed class TextControl : ControlBase
{
    #region Fields

    TextLayout _textLayout = TextLayout.Default;
    int _isMouseDownByPointerId = -1;
    RectF _bounds;
    int _startY, _maxStartY;
    float _scrollbarLength, _scrollbarY;

    #endregion

    public string Text
    {
        get => field ?? string.Empty;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public string TextFontFamilyName
    {
        get => field ?? Styles.NormalFontFamily;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public int TextFontSize
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
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
        _bounds = new(Location, Size);
    }

    public override void SizeChanged()
    {
        base.SizeChanged();
        _bounds = new(Location, Size);
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
        if (_textLayout == TextLayout.Default)
            return;

        if (_startY > 0)
            _startY = 0;
        else if (_startY < _maxStartY)
            _startY = _maxStartY;

        _scrollbarLength = Size.Height / _textLayout.Height * Size.Height;
        _scrollbarY = _maxStartY != 0
            ? (Size.Height - _scrollbarLength) * (1.0f + ((float)_startY / _maxStartY - 1.0f))
            : -1;
    }

    public override void Render()
    {
        if (_textLayout == TextLayout.Default)
        {
            _textLayout = new TextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, int.MaxValue);
            _maxStartY = (int)(Size.Height - _textLayout.Height);
            if (_maxStartY >= 0)
                _maxStartY = 0;
        }

        GraphicsDevice.PushAxisAlignedClip(_bounds, AntiAliasMode.PerPrimitive);
        GraphicsDevice.Draw(_textLayout, new(Location.X, Location.Y + _startY), SolidColorBrush.White);
        GraphicsDevice.PopAxisAlignedClip();

        if (_scrollbarY >= 0.0f)
            GraphicsDevice.DrawLine(
                new(Location.X + Size.Width + 1, Location.Y + _scrollbarY),
                new(Location.X + Size.Width + 1, Location.Y + _scrollbarY + _scrollbarLength),
                1.0f,
                SolidColorBrush.White);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayout);
        base.DisposeResources();
    }

    #endregion
}