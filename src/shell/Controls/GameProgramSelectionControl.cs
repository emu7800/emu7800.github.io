// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Shell;

public sealed class GameProgramSelectionControl : ControlBase
{
    static readonly EventHandler<GameProgramSelectedEventArgs> DefaultEventHandler = (_, _) => {};

    #region Fields

    StaticBitmap _playRest          = StaticBitmap.Empty;
    StaticBitmap _playRestInverted  = StaticBitmap.Empty;
    StaticBitmap _pauseRest         = StaticBitmap.Empty;
    StaticBitmap _pauseRestInverted = StaticBitmap.Empty;

    const float
        EPILSON               = 1e-6f,
        ITEM_WIDTH            = 300f,
        ITEM_HEIGHT           = 75f,
        ICON_WIDTH            = 48f,
        ICON_HEIGHT           = 48f,
        ICON_DX               = 0,
        ICON_DY               = ITEM_HEIGHT / 2 - ICON_HEIGHT / 2,
        AccelerationUnit      = 500f,  // in pixels/sec/sec
        DragCoefficient       = 5f,
        GapForCollectionTitle = 50f,
        GapBetweenCollections = 50f;

    readonly SizeF _itemSize = new(ITEM_WIDTH, ITEM_HEIGHT);
    readonly SizeF _iconSize = new(ICON_WIDTH, ICON_HEIGHT);

    float _scrollXLeftMostBoundary, _scrollXRightMostBoundary, _scrollXAcceleration;

    bool _itemDown;
    int _isMouseDownByPointerId = -1;
    bool _mouseCollectionInIcon;
    int _mouseCollectionIndex, _mouseCollectionItemIndex;

    int _focusedCollectionIndex = -1, _focusedCollectionItemIndex = -1;
    int _focusCandidateCollectionIndex, _focusCandidateCollectionItemIndex;

    readonly ScrollColumnInfo[] _scrollColumnInfoSet;
    RectF _gameProgramViewItemCollectionsRect;
    RectF _clipRect;

    bool _reinitializeAtNextUpdate, _initializeAtNextUpdate = true;

    KeyboardKey _keyRepeatKey = KeyboardKey.None;
    long _keyRepeatTicks;

#if PROFILE
    // On TVPC, GetCursorPos() returns incorrect results when mouse wheel is turning after directinput API has been started once.
    // This displays the mouse position when the wheel is turned.
    int _lastWheelX, _lastWheelY;
#endif

    #endregion

    public event EventHandler<GameProgramSelectedEventArgs> Selected = DefaultEventHandler;

    #region ControlBase Overrides

    public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        if (down && _isMouseDownByPointerId >= 0
            || !down && _isMouseDownByPointerId < 0
                || !down && _isMouseDownByPointerId != pointerId)
            return;

        _isMouseDownByPointerId = down ? pointerId : -1;

        if (_isMouseDownByPointerId >= 0)
        {
            RefreshMouseCollectionIndexes(x, y);
            if (_mouseCollectionInIcon)
            {
                SetFocus(_mouseCollectionIndex, _mouseCollectionItemIndex);
                _itemDown = true;
            }
        }
        else if (_itemDown)
        {
            RefreshMouseCollectionIndexes(x, y);
            if (_mouseCollectionInIcon)
                RaiseSelected();
            _itemDown = false;
        }
    }

    public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
        if (_scrollColumnInfoSet.Length == 0 || _isMouseDownByPointerId < 0)
            return;

        var ax = 9 * dx;
        AddHorizontalPotentialEnergyToCollections(ax);

        var ay = 7 * dy;
        AddVerticalPotentialEnergyToMouseHoveredCollection(ay, x, y);

        if (ax > 8 || ay > 8)
        {
            ReleaseFocus();
            _itemDown = false;
        }
    }

    public override void MouseWheelChanged(int pointerId, int x, int y, int delta)
    {
#if PROFILE
        _lastWheelX = x;
        _lastWheelY = y;
#endif
        if (_scrollColumnInfoSet.Length == 0 || _isMouseDownByPointerId >= 0)
            return;

        var mouseOutsideWindow = y < Location.Y + GapForCollectionTitle
            || y > Location.Y + Size.Height
                || x < Location.X
                    || x > Location.X + Size.Width;

        if (mouseOutsideWindow)
        {
            var unitDelta = delta / 30;
            var a = unitDelta * AccelerationUnit;
            AddHorizontalPotentialEnergyToCollections(a);
        }
        else
        {
            var unitDelta = delta / 120;
            var a = unitDelta * AccelerationUnit;
            AddVerticalPotentialEnergyToMouseHoveredCollection(a, x, y);
        }

        ReleaseFocus();
        _itemDown = false;
    }

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        switch (key)
        {
            case KeyboardKey.Enter:
                if (!down)
                    RaiseSelected();
                break;
            case KeyboardKey.Left:
            case KeyboardKey.Right:
            case KeyboardKey.Up:
            case KeyboardKey.Down:
                if (down)
                {
                    _keyRepeatKey = key;
                    _keyRepeatTicks = (long)(0.75 / TimerDevice.SecondsPerTick);
                }
                else
                {
                    _keyRepeatKey = KeyboardKey.None;
                    _keyRepeatTicks = 0;
                    HandleKeyPressed(key);
                }
                break;
        }
    }

    public override void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
        base.ControllerButtonChanged(controllerNo, input, down);
        var key = input switch
        {
            MachineInput.Start => KeyboardKey.Enter,
            MachineInput.Left  => KeyboardKey.Left,
            MachineInput.Right => KeyboardKey.Right,
            MachineInput.Up    => KeyboardKey.Up,
            MachineInput.Down  => KeyboardKey.Down,
            _                  => KeyboardKey.None
        };
        KeyboardKeyPressed(key, down);
    }

    public override void Update(TimerDevice td)
    {
        if (_scrollColumnInfoSet.Length == 0)
            return;

        if (_initializeAtNextUpdate || _reinitializeAtNextUpdate)
        {
            ComputeClipRect();
            InitializeViewItemQuantities(_initializeAtNextUpdate);
            _initializeAtNextUpdate = _reinitializeAtNextUpdate = false;
        }

        var focusScrollXVelocity = 0f;
        var focusScrollYVelocity = 0f;
        if (IsFocusSet)
        {
            var iconRect = ToIconRect(_focusedCollectionIndex, _focusedCollectionItemIndex);
            TranslateRect(ref iconRect, _focusedCollectionIndex);

            // prevents left/right jittering when screen is narrow (e.g., in snapped view)
            var effectiveItemWidth = Size.Width > ITEM_WIDTH + 20 ? ITEM_WIDTH : 0.75 * Size.Width;
            if (iconRect.Left < Location.X)
                focusScrollXVelocity = 10 * AccelerationUnit;
            else if (iconRect.Left + effectiveItemWidth > Location.X + Size.Width)
                focusScrollXVelocity = -10*AccelerationUnit;

            if (iconRect.Top < Location.Y + GapForCollectionTitle)
                focusScrollYVelocity = 10 * AccelerationUnit;
            else if (iconRect.Bottom > Location.Y + Size.Height)
                focusScrollYVelocity = -10 * AccelerationUnit;

            focusScrollXVelocity *= td.DeltaInSeconds;
            focusScrollYVelocity *= td.DeltaInSeconds;
        }

        var scrollXVelocity = ExtractHorizontalKineticEnergyFromCollections(td.DeltaInSeconds) + focusScrollXVelocity;
        _gameProgramViewItemCollectionsRect.Left += scrollXVelocity;
        _gameProgramViewItemCollectionsRect.Right += scrollXVelocity;

        var dx = 0f;
        if (_gameProgramViewItemCollectionsRect.Left < _scrollXLeftMostBoundary)
            dx = _scrollXLeftMostBoundary - _gameProgramViewItemCollectionsRect.Left;
        else if (_gameProgramViewItemCollectionsRect.Left > _scrollXRightMostBoundary)
            dx = _scrollXRightMostBoundary - _gameProgramViewItemCollectionsRect.Left;
        _gameProgramViewItemCollectionsRect.Left += dx;
        _gameProgramViewItemCollectionsRect.Right += dx;

        for (var i = 0; i < _scrollColumnInfoSet.Length; i++)
        {
            var scrollYVelocity = ExtractVerticalKineticEnergyFromCollection(i, td.DeltaInSeconds);
            if (i == _focusedCollectionIndex)
                scrollYVelocity += focusScrollYVelocity;
            if (Math.Abs(scrollYVelocity) < EPILSON)
                continue;
            var gpvic = _scrollColumnInfoSet[i];
            var rect = gpvic.CollectionRect;
            rect.Top += scrollYVelocity;
            rect.Bottom += scrollYVelocity;
            if (rect.Top < gpvic.ScrollYTopMostBoundary)
            {
                var h = rect.ToSize().Height;
                rect.Top = gpvic.ScrollYTopMostBoundary;
                rect.Bottom = gpvic.ScrollYTopMostBoundary + h;
            }
            else if (rect.Top > gpvic.ScrollYBottomMostBoundary)
            {
                var h = rect.ToSize().Height;
                rect.Top = gpvic.ScrollYBottomMostBoundary;
                rect.Bottom = gpvic.ScrollYBottomMostBoundary + h;
            }
            gpvic.CollectionRect = rect;
        }

        if (_keyRepeatKey > 0)
        {
            _keyRepeatTicks -= td.DeltaTicks;
            if (_keyRepeatTicks <= 0)
            {
                _keyRepeatTicks += (long)(0.10 / TimerDevice.SecondsPerTick);
                HandleKeyPressed(_keyRepeatKey);
            }
        }
    }

    public override void Render(IGraphicsDeviceDriver graphicsDevice)
    {
#if PROFILE
        var rect = new RectF { Left = _lastWheelX - 5, Right = _lastWheelX + 5, Bottom = _lastWheelY + 5, Top = _lastWheelY - 5 };
        gd.FillEllipse(rect, D2DSolidColorBrush.White);
#endif
        _focusCandidateCollectionIndex = -1;
        _focusCandidateCollectionItemIndex = -1;

        for (var i = 0; i < _scrollColumnInfoSet.Length; i++)
        {
            var gpivic = _scrollColumnInfoSet[i].GameProgramInfoViewItemCollection;
            for (var j = 0; j < gpivic.GameProgramInfoViewItems.Length; j++)
            {
                var itemRect = ToItemRect(i, j);
                TranslateRect(ref itemRect, i);

                if (j == 0)
                {
                    if (gpivic.NameTextLayout == TextLayout.Empty)
                        gpivic.NameTextLayout = graphicsDevice.CreateTextLayout(Styles.LargeFontFamily, Styles.LargeFontSize, gpivic.Name, ITEM_WIDTH, ITEM_HEIGHT, WriteParaAlignment.Near, WriteTextAlignment.Leading, SolidColorBrush.White);
                    graphicsDevice.Draw(
                        gpivic.NameTextLayout,
                        new(itemRect.Left, Location.Y + ITEM_HEIGHT / 2 - gpivic.NameTextLayout.Height));
                }

                if (itemRect.Right < Location.X || itemRect.Left > Location.X + Size.Width
                ||  itemRect.Top > Location.Y + GapForCollectionTitle + Size.Height)
                    break;
                if (itemRect.Bottom < Location.Y)
                    continue;

                graphicsDevice.PushAxisAlignedClip(_clipRect, AntiAliasMode.Aliased);

                var iconRect = ToIconRect(i, j);
                TranslateRect(ref iconRect, i);

                var isIconRectVisible =
                       iconRect.Left   >= Location.X
                    && iconRect.Right  <  Location.X + Size.Width
                    && iconRect.Top    >= Location.Y + GapForCollectionTitle
                    && iconRect.Bottom <  Location.Y + Size.Height;

                if (_focusCandidateCollectionIndex < 0 && isIconRectVisible)
                {
                    _focusCandidateCollectionIndex = i;
                    _focusCandidateCollectionItemIndex = j;
                }

                var gpivi = gpivic.GameProgramInfoViewItems[j];

                if (gpivi.TitleTextLayout == TextLayout.Empty)
                    gpivi.TitleTextLayout = graphicsDevice.CreateTextLayout(Styles.NormalFontFamily, Styles.NormalFontSize, gpivi.Title, ITEM_WIDTH - 25, ITEM_HEIGHT, WriteParaAlignment.Near, WriteTextAlignment.Leading, SolidColorBrush.White);
                if (gpivi.SubTitleTextLayout == TextLayout.Empty)
                    gpivi.SubTitleTextLayout = graphicsDevice.CreateTextLayout(Styles.SmallFontFamily, Styles.SmallFontSize, gpivi.SubTitle, ITEM_WIDTH - 25, ITEM_HEIGHT, WriteParaAlignment.Near, WriteTextAlignment.Leading, SolidColorBrush.Gray);

                var itemRectHeight = itemRect.Bottom - itemRect.Top;
                var totalTextHeight = gpivi.TitleTextLayout.Height + gpivi.SubTitleTextLayout.Height;
                var textYStart = itemRect.Top + itemRectHeight / 2 - totalTextHeight / 2;
                PointF textTitleLocation = new(itemRect.Left + 64, textYStart);
                PointF textSubTitleLocation = new(itemRect.Left + 64, textYStart + gpivi.TitleTextLayout.Height);

                if (i == _focusedCollectionIndex && j == _focusedCollectionItemIndex)
                {
                    if (_itemDown)
                    {
                        var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRestInverted : _playRestInverted;
                        graphicsDevice.FillRectangle(iconRect, SolidColorBrush.White);
                        graphicsDevice.Draw(bitmap, iconRect);
                        graphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation);
                        graphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation);
                    }
                    else
                    {
                        var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                        graphicsDevice.Draw(bitmap, iconRect);
                        graphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation);
                        graphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation);
                        graphicsDevice.DrawRectangle(iconRect, 5.0f, SolidColorBrush.White);
                    }
                }
                else
                {
                    var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                    graphicsDevice.Draw(bitmap, iconRect);
                    graphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation);
                    graphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation);
                    graphicsDevice.DrawRectangle(iconRect, 1.0f, SolidColorBrush.White);
                }

                graphicsDevice.PopAxisAlignedClip();
            }
        }
    }

    protected override async void CreateResources(IGraphicsDeviceDriver graphicsDevice)
    {
        base.CreateResources(graphicsDevice);

        var playRestBytes          = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest);
        var playRestInvertedBytes  = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest_inverted);
        var pauseRestBytes         = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest);
        var pauseRestInvertedBytes = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest_inverted);

        _playRest          = graphicsDevice.CreateStaticBitmap(playRestBytes.Span);
        _playRestInverted  = graphicsDevice.CreateStaticBitmap(playRestInvertedBytes.Span);
        _pauseRest         = graphicsDevice.CreateStaticBitmap(pauseRestBytes.Span);
        _pauseRestInverted = graphicsDevice.CreateStaticBitmap(pauseRestInvertedBytes.Span);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _playRest);
        SafeDispose(ref _playRestInverted);
        SafeDispose(ref _pauseRest);
        SafeDispose(ref _pauseRestInverted);

        foreach (var sci in _scrollColumnInfoSet)
        {
            var ntl = sci.GameProgramInfoViewItemCollection.NameTextLayout;
            if (ntl != TextLayout.Empty)
                ntl.Dispose();
            sci.GameProgramInfoViewItemCollection.NameTextLayout = TextLayout.Empty;

            foreach (var gpivi in sci.GameProgramInfoViewItemCollection.GameProgramInfoViewItems)
            {
                var ttl = gpivi.TitleTextLayout;
                var sttl = gpivi.SubTitleTextLayout;
                if (ttl != TextLayout.Empty)
                    ttl.Dispose();
                if (sttl != TextLayout.Empty)
                    sttl.Dispose();
                gpivi.TitleTextLayout = TextLayout.Empty;
                gpivi.SubTitleTextLayout = TextLayout.Empty;
            }
        }

        base.DisposeResources();
    }

    #endregion

    #region PageBase Overrides

    public override void SizeChanged()
    {
        _reinitializeAtNextUpdate = true;
    }

    #endregion

    #region Constructors

    #pragma warning disable IDE0290 // Use primary constructor

    public GameProgramSelectionControl(List<GameProgramInfoViewItemCollection> gameProgramViewItems)
    {
        _scrollColumnInfoSet = [..gameProgramViewItems.Select(ToScrollColumnInfo)];
        _reinitializeAtNextUpdate = true;
    }

    #endregion

    #region Helpers

    void HandleKeyPressed(KeyboardKey key)
    {
        if (!IsFocusSet)
        {
            SetFocus(_focusCandidateCollectionIndex, _focusCandidateCollectionItemIndex);
            return;
        }
        var iconRect = ToIconRect(_focusedCollectionIndex, _focusedCollectionItemIndex);
        TranslateRect(ref iconRect, _focusedCollectionIndex);
        switch (key)
        {
            case KeyboardKey.Left:
                if (_focusedCollectionIndex > 0)
                {
                    var pt = Add(iconRect.ToLocation(), new(ITEM_WIDTH / 2 - ITEM_WIDTH, ITEM_HEIGHT / 2));
                    var t = ToCollectionIndexes(pt);
                    _focusedCollectionIndex = t.Item1;
                    _focusedCollectionItemIndex = t.Item2;
                }
                break;
            case KeyboardKey.Right:
                if (_focusedCollectionIndex < _scrollColumnInfoSet.Length - 1)
                {
                    var pt = Add(iconRect.ToLocation(), new(ITEM_WIDTH / 2 + ITEM_WIDTH, ITEM_HEIGHT / 2));
                    var t = ToCollectionIndexes(pt);
                    _focusedCollectionIndex = t.Item1;
                    _focusedCollectionItemIndex = t.Item2;
                }
                break;
            case KeyboardKey.Up:
                if (_focusedCollectionItemIndex > 0)
                {
                    var pt = Add(iconRect.ToLocation(), new(ITEM_WIDTH / 2, ITEM_HEIGHT / 2 - ITEM_HEIGHT));
                    var t = ToCollectionIndexes(pt);
                    _focusedCollectionIndex = t.Item1;
                    _focusedCollectionItemIndex = t.Item2;
                }
                break;
            case KeyboardKey.Down:
                if (_focusedCollectionItemIndex < _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItems.Length - 1)
                {
                    var pt = Add(iconRect.ToLocation(), new(ITEM_WIDTH / 2, ITEM_HEIGHT / 2 + ITEM_HEIGHT));
                    var t = ToCollectionIndexes(pt);
                    _focusedCollectionIndex = t.Item1;
                    _focusedCollectionItemIndex = t.Item2;
                }
                break;
        }
    }

    void RaiseSelected()
    {
        if (Selected == DefaultEventHandler || !IsFocusSet)
            return;
        var gpivi = _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItems[_focusedCollectionItemIndex];
        Selected(this, new(ToGameProgramInfoViewItem(gpivi)));
    }

    void ComputeClipRect()
    {
        PointF location = new(Location.X, Location.Y + GapForCollectionTitle);
        SizeF size = new(Size.Width, Size.Height - GapForCollectionTitle);
        _clipRect = new(location, size);
    }

    void InitializeViewItemQuantities(bool includeCollectionRects)
    {
        var maxLen = 0;
        for (var i = 0; i < _scrollColumnInfoSet.Length; i++)
        {
            var scrollColumnInfo = _scrollColumnInfoSet[i];
            var clen = scrollColumnInfo.GameProgramInfoViewItemCollection.GameProgramInfoViewItems.Length;
            scrollColumnInfo.ScrollYBottomMostBoundary = 5f;
            if (includeCollectionRects)
            {
                scrollColumnInfo.CollectionRect = new(
                    new(0, scrollColumnInfo.ScrollYBottomMostBoundary),
                    new(ITEM_WIDTH, clen * ITEM_HEIGHT)
                    );
            }
            scrollColumnInfo.ScrollYTopMostBoundary = scrollColumnInfo.CollectionRect.ToSize().Height > Size.Height - GapForCollectionTitle
                ? Location.Y + Size.Height - 2 * GapForCollectionTitle - scrollColumnInfo.CollectionRect.ToSize().Height
                : scrollColumnInfo.ScrollYBottomMostBoundary;
            if (clen > maxLen)
                maxLen = clen;
        }
        if (includeCollectionRects)
        {
            _gameProgramViewItemCollectionsRect = new(
                new(Location.X + 10, Location.Y + GapForCollectionTitle),
                new((ITEM_WIDTH + GapBetweenCollections) * _scrollColumnInfoSet.Length, maxLen * ITEM_HEIGHT)
                );
        }
        _scrollXLeftMostBoundary = -(Location.X + _gameProgramViewItemCollectionsRect.ToSize().Width - Size.Width);
        _scrollXRightMostBoundary = 10;
        if (_scrollXLeftMostBoundary > _scrollXRightMostBoundary)
            _scrollXLeftMostBoundary = _scrollXRightMostBoundary;
    }

    RectF ToItemRect(int i, int j)
    {
        var x = i * (ITEM_WIDTH + GapBetweenCollections);
        var y = j * ITEM_HEIGHT;
        return new(new(x, y), _itemSize);
    }

    RectF ToIconRect(int i, int j)
    {
        var x = i * (ITEM_WIDTH + GapBetweenCollections);
        var y = j * ITEM_HEIGHT;
        return new(new(x + ICON_DX, y + ICON_DY), _iconSize);
    }

    void TranslateRect(ref RectF sourceRect, int collectionIndex)
    {
        var gpvic = _scrollColumnInfoSet[collectionIndex];
        var translationVector = Add(_gameProgramViewItemCollectionsRect.ToLocation(), gpvic.CollectionRect.ToLocation());
        sourceRect.Left   += translationVector.X;
        sourceRect.Top    += translationVector.Y;
        sourceRect.Right  += translationVector.X;
        sourceRect.Bottom += translationVector.Y;
    }

    static PointF Add(PointF a, PointF b)
        => new(a.X + b.X, a.Y + b.Y);

    float ExtractHorizontalKineticEnergyFromCollections(float timeDeltaInSeconds)
    {
        var scrollXVelocity = timeDeltaInSeconds * _scrollXAcceleration;
        var dragXForce = -1.0f * scrollXVelocity * DragCoefficient;
        if (Math.Abs(dragXForce) > Math.Abs(_scrollXAcceleration))
            _scrollXAcceleration = 0.0f;
        else
            _scrollXAcceleration += dragXForce;
        return scrollXVelocity;
    }

    float ExtractVerticalKineticEnergyFromCollection(int collectionIndex, float timeDeltaInSeconds)
    {
        var gpvic = _scrollColumnInfoSet[collectionIndex];
        var scrollYVelocity = timeDeltaInSeconds * gpvic.ScrollYAcceleration;
        var dragYForce = -1.0f * scrollYVelocity * DragCoefficient;
        if (Math.Abs(dragYForce) > Math.Abs(gpvic.ScrollYAcceleration))
            gpvic.ScrollYAcceleration = 0.0f;
        else
            gpvic.ScrollYAcceleration += dragYForce;
        return scrollYVelocity;
    }

    void AddHorizontalPotentialEnergyToCollections(float acceleration)
    {
        _scrollXAcceleration += acceleration;
    }

    void AddVerticalPotentialEnergyToMouseHoveredCollection(float acceleration, int x, int y)
    {
        RefreshMouseCollectionIndexes(x, y);
        AddVerticalPotentialEnergyToCollection(acceleration, _mouseCollectionIndex);
    }

    void AddVerticalPotentialEnergyToCollection(float acceleration, int collectionIndex)
    {
        if (collectionIndex >= 0)
            _scrollColumnInfoSet[collectionIndex].ScrollYAcceleration += acceleration;
    }

    void RefreshMouseCollectionIndexes(int x, int y)
    {
        var t = ToCollectionIndexes(new(x, y));
        _mouseCollectionIndex = t.Item1;
        _mouseCollectionItemIndex = t.Item2;
        _mouseCollectionInIcon = t.Item3;
    }

    // return tuple: (collection index, collection item index, is inside icon rectangle?)
    (int, int, bool) ToCollectionIndexes(PointF mousePointerLocation)
    {
        PointF pt = new(mousePointerLocation);
        Sub(ref pt, new(_gameProgramViewItemCollectionsRect));
        if (pt.X < 0)
            return (-1, -1, false);

        var i = (int)(pt.X / (ITEM_WIDTH + GapBetweenCollections));
        if (i < 0 || i >= _scrollColumnInfoSet.Length)
            return (-1, -1, false);

        var scrollColumnInfo = _scrollColumnInfoSet[i];
        var gpvic = scrollColumnInfo.GameProgramInfoViewItemCollection;
        Sub(ref pt, scrollColumnInfo.CollectionRect.ToLocation());
        var j = (int)(pt.Y / ITEM_HEIGHT);
        if (j < 0 || j >= gpvic.GameProgramInfoViewItems.Length)
            return (i, -1, false);

        Sub(ref pt, i * (ITEM_WIDTH + GapBetweenCollections), j * ITEM_HEIGHT);
        Sub(ref pt, new(ICON_DX, ICON_DY));

        var insideIconRect = pt is { X: < ICON_WIDTH and >= 0, Y: < ICON_HEIGHT and >= 0 };
        return (i, j, insideIconRect);
    }

    static void Sub(ref PointF a, float x, float y)
    {
        a.X -= x;
        a.Y -= y;
    }

    static void Sub(ref PointF a, PointF b)
    {
        a.X -= b.X;
        a.Y -= b.Y;
    }

    bool IsFocusSet
        => _focusedCollectionIndex >= 0 && _focusedCollectionItemIndex >= 0;

    void SetFocus(int collectionIndex, int collectionItemIndex)
    {
        _focusedCollectionIndex = collectionIndex;
        _focusedCollectionItemIndex = collectionItemIndex;
    }

    void ReleaseFocus()
    {
        _focusedCollectionIndex = _focusedCollectionItemIndex = -1;
    }

    static ScrollColumnInfo ToScrollColumnInfo(GameProgramInfoViewItemCollection gpvic)
        => new(ToGameProgramInfoViewItemCollectionEx(gpvic));

    static GameProgramInfoViewItemCollectionEx ToGameProgramInfoViewItemCollectionEx(GameProgramInfoViewItemCollection gpvic)
        => new(gpvic.Name, [.. gpvic.GameProgramInfoViewItems.Select(ToGameProgramInfoViewItemEx)]);

    static GameProgramInfoViewItemEx ToGameProgramInfoViewItemEx(GameProgramInfoViewItem gpvi)
        => new(gpvi.Title, gpvi.SubTitle, gpvi.ImportedGameProgramInfo);

    static GameProgramInfoViewItem ToGameProgramInfoViewItem(GameProgramInfoViewItemEx gpivi)
        => new(gpivi.ImportedGameProgramInfo, gpivi.SubTitle);

    #endregion
}