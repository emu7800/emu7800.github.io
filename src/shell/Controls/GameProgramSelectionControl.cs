// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Core;
using EMU7800.Services.Dto;
using EMU7800.Win32.Interop;
using System;
using System.Linq;

namespace EMU7800.D2D.Shell
{
    public sealed class GameProgramSelectionControl : ControlBase
    {
        static readonly EventHandler<GameProgramSelectedEventArgs> DefaultEventHandler = (s, o) => {};

        #region Fields

        StaticBitmap _playRest          = StaticBitmap.Default;
        StaticBitmap _playRestInverted  = StaticBitmap.Default;
        StaticBitmap _pauseRest         = StaticBitmap.Default;
        StaticBitmap _pauseRestInverted = StaticBitmap.Default;

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

        readonly D2D_SIZE_F _itemSize = new(ITEM_WIDTH, ITEM_HEIGHT);
        readonly D2D_SIZE_F _iconSize = new(ICON_WIDTH, ICON_HEIGHT);

        float _scrollXLeftMostBoundary, _scrollXRightMostBoundary, _scrollXAcceleration;

        bool _itemDown;
        int _isMouseDownByPointerId = -1;
        bool _mouseCollectionInIcon;
        int _mouseCollectionIndex, _mouseCollectionItemIndex;

        int _focusedCollectionIndex = -1, _focusedCollectionItemIndex = -1;
        int _focusCandidateCollectionIndex, _focusCandidateCollectionItemIndex;

        ScrollColumnInfo[] _scrollColumnInfoSet = Array.Empty<ScrollColumnInfo>();
        D2D_RECT_F _gameProgramViewItemCollectionsRect;
        D2D_RECT_F _clipRect;

        bool _reinitializeAtNextUpdate, _initializeAtNextUpdate = true;

#if PROFILE
        // On TVPC, GetCursorPos() returns incorrect results when mouse wheel is turning after directinput API has been started once.
        // This displays the mouse position when the wheel is turned.
        int _lastWheelX, _lastWheelY;
#endif

        #endregion

        public event EventHandler<GameProgramSelectedEventArgs> Selected = DefaultEventHandler;

        public void BindTo(GameProgramInfoViewItemCollection[] gameProgramInfoViewItemCollection)
        {
            _scrollColumnInfoSet = Enumerable.Range(0, gameProgramInfoViewItemCollection.Length)
                .Select(i => ToScrollColumnInfo(gameProgramInfoViewItemCollection[i]))
                    .ToArray();

            _reinitializeAtNextUpdate = true;
        }

        public override void SizeChanged()
        {
            _reinitializeAtNextUpdate = true;
        }

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

            var mouseOutsideWindow = (y < Location.Y + GapForCollectionTitle
                || y > Location.Y + Size.Height
                    || x < Location.X
                        || x > Location.X + Size.Width);

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
            if (down)
                return;
            switch (key)
            {
                case KeyboardKey.Enter:
                    RaiseSelected();
                    break;
                case KeyboardKey.Left:
                case KeyboardKey.Right:
                case KeyboardKey.Up:
                case KeyboardKey.Down:
                    HandleKeyPressed(key);
                    break;
            }
        }

        public override void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
        {
            base.ControllerButtonChanged(controllerNo, input, down);
            if (down)
                return;
            switch (input)
            {
                case MachineInput.Start:
                    RaiseSelected();
                    break;
                case MachineInput.Left:
                    HandleKeyPressed(KeyboardKey.Left);
                    break;
                case MachineInput.Right:
                    HandleKeyPressed(KeyboardKey.Right);
                    break;
                case MachineInput.Up:
                    HandleKeyPressed(KeyboardKey.Up);
                    break;
                case MachineInput.Down:
                    HandleKeyPressed(KeyboardKey.Down);
                    break;
            }
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
                var effectiveItemWidth = (Size.Width > ITEM_WIDTH + 20) ? ITEM_WIDTH : 0.75 * Size.Width;
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
        }

        public override void Render()
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
                for (var j = 0; j < gpivic.GameProgramInfoViewItemSet.Length; j++)
                {
                    var itemRect = ToItemRect(i, j);
                    TranslateRect(ref itemRect, i);

                    if (j == 0)
                    {
                        if (gpivic.NameTextLayout == TextLayout.Default)
                            gpivic.NameTextLayout = new TextLayout(Styles.LargeFontFamily, Styles.LargeFontSize,
                                gpivic.Name, ITEM_WIDTH, ITEM_HEIGHT
                                );
                        GraphicsDevice.Draw(
                            gpivic.NameTextLayout,
                            new(itemRect.Left, Location.Y + ITEM_HEIGHT / 2 - (float)gpivic.NameTextLayout.Height),
                            D2DSolidColorBrush.White
                            );
                    }

                    if (itemRect.Right < Location.X || itemRect.Left > Location.X + Size.Width
                    ||  itemRect.Top > Location.Y + GapForCollectionTitle + Size.Height)
                        break;
                    if (itemRect.Bottom < Location.Y)
                        continue;

                    GraphicsDevice.PushAxisAlignedClip(_clipRect, D2DAntiAliasMode.Aliased);

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

                    var gpivi = gpivic.GameProgramInfoViewItemSet[j];

                    if (gpivi.TitleTextLayout == TextLayout.Default)
                        gpivi.TitleTextLayout = new TextLayout(Styles.NormalFontFamily, Styles.NormalFontSize,
                            gpivi.Title, ITEM_WIDTH - 25, ITEM_HEIGHT
                            );
                    if (gpivi.SubTitleTextLayout == TextLayout.Default)
                        gpivi.SubTitleTextLayout = new TextLayout(Styles.SmallFontFamily, Styles.SmallFontSize,
                            gpivi.SubTitle, ITEM_WIDTH - 25, ITEM_HEIGHT
                            );

                    var itemRectHeight = itemRect.Bottom - itemRect.Top;
                    var totalTextHeight = gpivi.TitleTextLayout.Height + gpivi.SubTitleTextLayout.Height;
                    var textYStart = itemRect.Top + itemRectHeight / 2 - (float)totalTextHeight / 2;
                    D2D_POINT_2F textTitleLocation = new(itemRect.Left + 64, textYStart);
                    D2D_POINT_2F textSubTitleLocation = new(itemRect.Left + 64, textYStart + (float)gpivi.TitleTextLayout.Height);

                    if (i == _focusedCollectionIndex && j == _focusedCollectionItemIndex)
                    {
                        if (_itemDown)
                        {
                            var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRestInverted : _playRestInverted;
                            GraphicsDevice.FillRectangle(iconRect, D2DSolidColorBrush.White);
                            GraphicsDevice.Draw(bitmap, iconRect);
                            GraphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                            GraphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                        }
                        else
                        {
                            var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                            GraphicsDevice.Draw(bitmap, iconRect);
                            GraphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                            GraphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                            GraphicsDevice.DrawRectangle(iconRect, 5.0f, D2DSolidColorBrush.White);
                        }
                    }
                    else
                    {
                        var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                        GraphicsDevice.Draw(bitmap, iconRect);
                        GraphicsDevice.Draw(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                        GraphicsDevice.Draw(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                        GraphicsDevice.DrawRectangle(iconRect, 1.0f, D2DSolidColorBrush.White);
                    }

                    GraphicsDevice.PopAxisAlignedClip();
                }
            }
        }

        protected async override void CreateResources()
        {
            base.CreateResources();

            var playRestBytes          = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest);
            var playRestInvertedBytes  = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest_inverted);
            var pauseRestBytes         = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest);
            var pauseRestInvertedBytes = await AssetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest_inverted);

            _playRest = new StaticBitmap(playRestBytes);
            _playRestInverted = new StaticBitmap(playRestInvertedBytes);
            _pauseRest = new StaticBitmap(pauseRestBytes);
            _pauseRestInverted = new StaticBitmap(pauseRestInvertedBytes);
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
                if (ntl != TextLayout.Default)
                    ntl.Dispose();
                sci.GameProgramInfoViewItemCollection.NameTextLayout = TextLayout.Default;

                foreach (var gpivi in sci.GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet)
                {
                    var ttl = gpivi.TitleTextLayout;
                    var sttl = gpivi.SubTitleTextLayout;
                    if (ttl != TextLayout.Default)
                        ttl.Dispose();
                    if (sttl != TextLayout.Default)
                        sttl.Dispose();
                    gpivi.TitleTextLayout = TextLayout.Default;
                    gpivi.SubTitleTextLayout = TextLayout.Default;
                }
            }

            base.DisposeResources();
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
                    if (_focusedCollectionItemIndex < _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet.Length - 1)
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
            var gpivi = _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet[_focusedCollectionItemIndex];
            Selected(this, new(ToGameProgramInfoViewItem(gpivi)));
        }

        void ComputeClipRect()
        {
            D2D_POINT_2F location = new(Location.X, Location.Y + GapForCollectionTitle);
            D2D_SIZE_F size = new(Size.Width, Size.Height - GapForCollectionTitle);
            _clipRect = new(location, size);
        }

        void InitializeViewItemQuantities(bool includeCollectionRects)
        {
            var maxLen = 0;
            for (var i = 0; i < _scrollColumnInfoSet.Length; i++)
            {
                var scrollColumnInfo = _scrollColumnInfoSet[i];
                var clen = scrollColumnInfo.GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet.Length;
                scrollColumnInfo.ScrollYBottomMostBoundary = 5f;
                if (includeCollectionRects)
                {
                    scrollColumnInfo.CollectionRect = new(
                        new(0, scrollColumnInfo.ScrollYBottomMostBoundary),
                        new(ITEM_WIDTH, clen * ITEM_HEIGHT)
                        );
                }
                scrollColumnInfo.ScrollYTopMostBoundary = (scrollColumnInfo.CollectionRect.ToSize().Height > (Size.Height - GapForCollectionTitle))
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

        D2D_RECT_F ToItemRect(int i, int j)
        {
            var x = i * (ITEM_WIDTH + GapBetweenCollections);
            var y = j * ITEM_HEIGHT;
            return new(new(x, y), _itemSize);
        }

        D2D_RECT_F ToIconRect(int i, int j)
        {
            var x = i * (ITEM_WIDTH + GapBetweenCollections);
            var y = j * ITEM_HEIGHT;
            return new(new(x + ICON_DX, y + ICON_DY), _iconSize);
        }

        void TranslateRect(ref D2D_RECT_F sourceRect, int collectionIndex)
        {
            var gpvic = _scrollColumnInfoSet[collectionIndex];
            var translationVector = Add(_gameProgramViewItemCollectionsRect.ToLocation(), gpvic.CollectionRect.ToLocation());
            sourceRect.Left   += translationVector.X;
            sourceRect.Top    += translationVector.Y;
            sourceRect.Right  += translationVector.X;
            sourceRect.Bottom += translationVector.Y;
        }

        static D2D_POINT_2F Add(D2D_POINT_2F a, D2D_POINT_2F b)
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
        (int, int, bool) ToCollectionIndexes(D2D_POINT_2F mousePointerLocation)
        {
            D2D_POINT_2F pt = new(mousePointerLocation);
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
            if (j < 0 || j >= gpvic.GameProgramInfoViewItemSet.Length)
                return (i, -1, false);

            Sub(ref pt, i * (ITEM_WIDTH + GapBetweenCollections), j * ITEM_HEIGHT);
            Sub(ref pt, new(ICON_DX, ICON_DY));

            var insideIconRect = pt.X < ICON_WIDTH && pt.Y < ICON_HEIGHT && pt.X >= 0 && pt.Y >= 0;
            return (i, j, insideIconRect);
        }

        static void Sub(ref D2D_POINT_2F a, float x, float y)
        {
            a.X -= x;
            a.Y -= y;
        }

        static void Sub(ref D2D_POINT_2F a, D2D_POINT_2F b)
        {
            a.X -= b.X;
            a.Y -= b.Y;
        }

        bool IsFocusSet
        {
            get { return (_focusedCollectionIndex >= 0 && _focusedCollectionItemIndex >= 0); }
        }

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
            => new() { GameProgramInfoViewItemCollection = ToGameProgramInfoViewItemCollectionEx(gpvic) };

        static GameProgramInfoViewItemCollectionEx ToGameProgramInfoViewItemCollectionEx(GameProgramInfoViewItemCollection gpvic)
            => new()
            {
                Name = gpvic.Name,
                GameProgramInfoViewItemSet = gpvic.GameProgramInfoViewItemSet.Select(ToGameProgramInfoViewItemEx).ToArray(),
            };

        static GameProgramInfoViewItemEx ToGameProgramInfoViewItemEx(GameProgramInfoViewItem gpvi)
            => new()
            {
                Title = gpvi.Title,
                SubTitle = gpvi.SubTitle,
                ImportedGameProgramInfo = gpvi.ImportedGameProgramInfo
            };

        static GameProgramInfoViewItem ToGameProgramInfoViewItem(GameProgramInfoViewItemEx gpivi)
            => new(gpivi.ImportedGameProgramInfo, gpivi.SubTitle);

        #endregion
    }
}
