// © Mike Murphy

using System;
using System.Linq;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public sealed class GameProgramSelectionControl : ControlBase
    {
        #region Fields

        readonly AssetService _assetService = new AssetService();

        StaticBitmap _playRest, _playRestInverted, _pauseRest, _pauseRestInverted;

        const float
            EPILSON               = 1e-6f,
            ITEM_WIDTH            = 300f,
            ITEM_HEIGHT           = 75f,
            ICON_WIDTH            = 48f,
            ICON_HEIGHT           = 48f,
            ICON_DX               = 0,
            ICON_DY               = ITEM_HEIGHT / 2 - ICON_HEIGHT / 2,
            AccelerationUnit      = 200f,  // in pixels/sec/sec
            DragCoefficient       = 5f,
            GapForCollectionTitle = 50f,
            GapBetweenCollections = 50f;

        readonly SizeF _itemSize = Struct.ToSizeF(ITEM_WIDTH, ITEM_HEIGHT);
        readonly SizeF _iconSize = Struct.ToSizeF(ICON_WIDTH, ICON_HEIGHT);

        float _scrollXLeftMostBoundary, _scrollXRightMostBoundary, _scrollXAcceleration;

        bool _itemDown;
        uint? _isMouseDownByPointerId;
        bool _mouseCollectionInIcon;
        int _mouseCollectionIndex, _mouseCollectionItemIndex;

        int _focusedCollectionIndex = -1, _focusedCollectionItemIndex = -1;
        int _focusCandidateCollectionIndex, _focusCandidateCollectionItemIndex;

        ScrollColumnInfo[] _scrollColumnInfoSet = new ScrollColumnInfo[0];
        RectF _gameProgramViewItemCollectionsRect;
        RectF _clipRect;

        bool _reinitializeAtNextUpdate, _initializeAtNextUpdate = true;

#if PROFILE
        // On TVPC, GetCursorPos() returns incorrect results when mouse wheel is turning after directinput API has been started once.
        // This displays the mouse position when the wheel is turned.
        int _lastWheelX, _lastWheelY;
#endif

        #endregion

        public event EventHandler<GameProgramSelectedEventArgs> Selected;

        public void BindTo(GameProgramInfoViewItemCollection[] gameProgramInfoViewItemCollection)
        {
            if (gameProgramInfoViewItemCollection == null)
                return;

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

        public override void MouseButtonChanged(uint pointerId, int x, int y, bool down)
        {
            if (down && _isMouseDownByPointerId.HasValue
                || !down && !_isMouseDownByPointerId.HasValue
                    || !down && _isMouseDownByPointerId.Value != pointerId)
                return;

            _isMouseDownByPointerId = down ? pointerId : (uint?)null;

            if (_isMouseDownByPointerId.HasValue)
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

        public override void MouseMoved(uint pointerId, int x, int y, int dx, int dy)
        {
            if (_scrollColumnInfoSet.Length == 0 || !_isMouseDownByPointerId.HasValue)
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

        public override void MouseWheelChanged(uint pointerId, int x, int y, int delta)
        {
#if PROFILE
            _lastWheelX = x;
            _lastWheelY = y;
#endif
            if (_scrollColumnInfoSet.Length == 0 || _isMouseDownByPointerId.HasValue)
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
                    var h = Struct.ToSize(rect).Height;
                    rect.Top = gpvic.ScrollYTopMostBoundary;
                    rect.Bottom = gpvic.ScrollYTopMostBoundary + h;
                }
                else if (rect.Top > gpvic.ScrollYBottomMostBoundary)
                {
                    var h = Struct.ToSize(rect).Height;
                    rect.Top = gpvic.ScrollYBottomMostBoundary;
                    rect.Bottom = gpvic.ScrollYBottomMostBoundary + h;
                }
                gpvic.CollectionRect = rect;
            }
        }

        public override void Render(GraphicsDevice gd)
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
                        if (gpivic.NameTextLayout == null)
                            gpivic.NameTextLayout = gd.CreateTextLayout(Styles.LargeFontFamily, Styles.LargeFontSize,
                                gpivic.Name, ITEM_WIDTH, ITEM_HEIGHT
                                );
                        gd.DrawText(gpivic.NameTextLayout,
                            Struct.ToPointF(itemRect.Left, Location.Y + ITEM_HEIGHT / 2 - (float)gpivic.NameTextLayout.Height),
                            D2DSolidColorBrush.White
                            );
                    }

                    if (itemRect.Right < Location.X || itemRect.Left > Location.X + Size.Width
                    ||  itemRect.Top > Location.Y + GapForCollectionTitle + Size.Height)
                        break;
                    if (itemRect.Bottom < Location.Y)
                        continue;

                    gd.PushAxisAlignedClip(_clipRect, D2DAntiAliasMode.Aliased);

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

                    if (gpivi.TitleTextLayout == null)
                        gpivi.TitleTextLayout = gd.CreateTextLayout(Styles.NormalFontFamily, Styles.NormalFontSize,
                            gpivi.Title, ITEM_WIDTH - 25, ITEM_HEIGHT
                            );
                    if (gpivi.SubTitleTextLayout == null)
                        gpivi.SubTitleTextLayout = gd.CreateTextLayout(Styles.SmallFontFamily, Styles.SmallFontSize,
                            gpivi.SubTitle, ITEM_WIDTH - 25, ITEM_HEIGHT
                            );

                    var itemRectHeight = itemRect.Bottom - itemRect.Top;
                    var totalTextHeight = gpivi.TitleTextLayout.Height + gpivi.SubTitleTextLayout.Height;
                    var textYStart = itemRect.Top + itemRectHeight / 2 - (float)totalTextHeight / 2;
                    var textTitleLocation = Struct.ToPointF(itemRect.Left + 64, textYStart);
                    var textSubTitleLocation = Struct.ToPointF(itemRect.Left + 64, textYStart + (float)gpivi.TitleTextLayout.Height);

                    if (i == _focusedCollectionIndex && j == _focusedCollectionItemIndex)
                    {
                        if (_itemDown)
                        {
                            var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRestInverted : _playRestInverted;
                            gd.FillRectangle(iconRect, D2DSolidColorBrush.White);
                            gd.DrawBitmap(bitmap, iconRect);
                            gd.DrawText(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                            gd.DrawText(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                        }
                        else
                        {
                            var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                            gd.DrawBitmap(bitmap, iconRect);
                            gd.DrawText(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                            gd.DrawText(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                            gd.DrawRectangle(iconRect, 5.0f, D2DSolidColorBrush.White);
                        }
                    }
                    else
                    {
                        var bitmap = gpivi.ImportedGameProgramInfo.PersistedStateExists ? _pauseRest : _playRest;
                        gd.DrawBitmap(bitmap, iconRect);
                        gd.DrawText(gpivi.TitleTextLayout, textTitleLocation, D2DSolidColorBrush.White);
                        gd.DrawText(gpivi.SubTitleTextLayout, textSubTitleLocation, D2DSolidColorBrush.Gray);
                        gd.DrawRectangle(iconRect, 1.0f, D2DSolidColorBrush.White);
                    }

                    gd.PopAxisAlignedClip();
                }
            }
        }

        protected async override void CreateResources(GraphicsDevice gd)
        {
            base.CreateResources(gd);

            var playRest = await _assetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest);
            var playRestInverted = await _assetService.GetAssetBytesAsync(Asset.appbar_transport_play_rest_inverted);
            var pauseRest = await _assetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest);
            var pauseRestInverted = await _assetService.GetAssetBytesAsync(Asset.appbar_transport_pause_rest_inverted);

            _playRest = gd.CreateStaticBitmap(playRest);
            _playRestInverted = gd.CreateStaticBitmap(playRestInverted);
            _pauseRest = gd.CreateStaticBitmap(pauseRest);
            _pauseRestInverted = gd.CreateStaticBitmap(pauseRestInverted);
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
                if (ntl != null)
                    ntl.Dispose();
                sci.GameProgramInfoViewItemCollection.NameTextLayout = null;

                foreach (var gpivi in sci.GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet)
                {
                    var ttl = gpivi.TitleTextLayout;
                    var sttl = gpivi.SubTitleTextLayout;
                    if (ttl != null)
                        ttl.Dispose();
                    if (sttl != null)
                        sttl.Dispose();
                    gpivi.TitleTextLayout = null;
                    gpivi.SubTitleTextLayout = null;
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
                        var pt = Add(Struct.ToLocation(iconRect), Struct.ToPointF(ITEM_WIDTH / 2 - ITEM_WIDTH, ITEM_HEIGHT / 2));
                        var t = ToCollectionIndexes(pt);
                        _focusedCollectionIndex = t.Item1;
                        _focusedCollectionItemIndex = t.Item2;
                    }
                    break;
                case KeyboardKey.Right:
                    if (_focusedCollectionIndex < _scrollColumnInfoSet.Length - 1)
                    {
                        var pt = Add(Struct.ToLocation(iconRect), Struct.ToPointF(ITEM_WIDTH / 2 + ITEM_WIDTH, ITEM_HEIGHT / 2));
                        var t = ToCollectionIndexes(pt);
                        _focusedCollectionIndex = t.Item1;
                        _focusedCollectionItemIndex = t.Item2;
                    }
                    break;
                case KeyboardKey.Up:
                    if (_focusedCollectionItemIndex > 0)
                    {
                        var pt = Add(Struct.ToLocation(iconRect), Struct.ToPointF(ITEM_WIDTH / 2, ITEM_HEIGHT / 2 - ITEM_HEIGHT));
                        var t = ToCollectionIndexes(pt);
                        _focusedCollectionIndex = t.Item1;
                        _focusedCollectionItemIndex = t.Item2;
                    }
                    break;
                case KeyboardKey.Down:
                    if (_focusedCollectionItemIndex < _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet.Length - 1)
                    {
                        var pt = Add(Struct.ToLocation(iconRect), Struct.ToPointF(ITEM_WIDTH / 2, ITEM_HEIGHT / 2 + ITEM_HEIGHT));
                        var t = ToCollectionIndexes(pt);
                        _focusedCollectionIndex = t.Item1;
                        _focusedCollectionItemIndex = t.Item2;
                    }
                    break;
            }
        }

        void RaiseSelected()
        {
            if (Selected == null || !IsFocusSet)
                return;
            var gpivi = _scrollColumnInfoSet[_focusedCollectionIndex].GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet[_focusedCollectionItemIndex];
            var e = new GameProgramSelectedEventArgs { GameProgramInfoViewItem = ToGameProgramInfoViewItem(gpivi) };
            Selected(this, e);
        }

        void ComputeClipRect()
        {
            var location = Struct.ToPointF(Location.X, Location.Y + GapForCollectionTitle);
            var size     = Struct.ToSizeF(Size.Width, Size.Height - GapForCollectionTitle);
            _clipRect = Struct.ToRectF(location, size);
        }

        void InitializeViewItemQuantities(bool includeCollectionRects)
        {
            if (_scrollColumnInfoSet == null)
                return;
            var maxLen = 0;
            for (var i = 0; i < _scrollColumnInfoSet.Length; i++)
            {
                var scrollColumnInfo = _scrollColumnInfoSet[i];
                var clen = scrollColumnInfo.GameProgramInfoViewItemCollection.GameProgramInfoViewItemSet.Length;
                scrollColumnInfo.ScrollYBottomMostBoundary = 5f;
                if (includeCollectionRects)
                {
                    scrollColumnInfo.CollectionRect = Struct.ToRectF(
                        Struct.ToPointF(0, scrollColumnInfo.ScrollYBottomMostBoundary),
                        Struct.ToSizeF(ITEM_WIDTH, clen * ITEM_HEIGHT)
                        );
                }
                scrollColumnInfo.ScrollYTopMostBoundary = (Struct.ToSize(scrollColumnInfo.CollectionRect).Height > (Size.Height - GapForCollectionTitle))
                    ? Location.Y + Size.Height - 2 * GapForCollectionTitle - Struct.ToSize(scrollColumnInfo.CollectionRect).Height
                    : scrollColumnInfo.ScrollYBottomMostBoundary;
                if (clen > maxLen)
                    maxLen = clen;
            }
            if (includeCollectionRects)
            {
                var size = Struct.ToSizeF((ITEM_WIDTH + GapBetweenCollections) * _scrollColumnInfoSet.Length, maxLen * ITEM_HEIGHT);
                _gameProgramViewItemCollectionsRect = Struct.ToRectF(Struct.ToPointF(Location.X + 10, Location.Y + GapForCollectionTitle), size);
            }
            _scrollXLeftMostBoundary = -(Location.X + Struct.ToSize(_gameProgramViewItemCollectionsRect).Width - Size.Width);
            _scrollXRightMostBoundary = 10;
        }

        RectF ToItemRect(int i, int j)
        {
            var x = i * (ITEM_WIDTH + GapBetweenCollections);
            var y = j * ITEM_HEIGHT;
            var location = Struct.ToPointF(x, y);
            var rect = Struct.ToRectF(location, _itemSize);
            return rect;
        }

        RectF ToIconRect(int i, int j)
        {
            var x = i * (ITEM_WIDTH + GapBetweenCollections);
            var y = j * ITEM_HEIGHT;
            var location = Struct.ToPointF(x + ICON_DX, y + ICON_DY);
            var rect = Struct.ToRectF(location, _iconSize);
            return rect;
        }

        void TranslateRect(ref RectF sourceRect, int collectionIndex)
        {
            var gpvic = _scrollColumnInfoSet[collectionIndex];
            var translationVector = Add(Struct.ToLocation(_gameProgramViewItemCollectionsRect), Struct.ToLocation(gpvic.CollectionRect));
            sourceRect.Left   += translationVector.X;
            sourceRect.Top    += translationVector.Y;
            sourceRect.Right  += translationVector.X;
            sourceRect.Bottom += translationVector.Y;
        }

        static PointF Add(PointF a, PointF b)
        {
            var pt = Struct.ToPointF(a.X + b.X, a.Y + b.Y);
            return pt;
        }

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
            var t = ToCollectionIndexes(Struct.ToPointF(x, y));
            _mouseCollectionIndex = t.Item1;
            _mouseCollectionItemIndex = t.Item2;
            _mouseCollectionInIcon = t.Item3;
        }

        // return tuple: (collection index, collection item index, is inside icon rectangle?)
        Tuple<int, int, bool> ToCollectionIndexes(PointF mousePointerLocation)
        {
            var pt = Struct.ToPointF(mousePointerLocation.X, mousePointerLocation.Y);
            Sub(ref pt, Struct.ToLocation(_gameProgramViewItemCollectionsRect));
            if (pt.X < 0)
                return new Tuple<int, int, bool>(-1, -1, false);

            var i = (int)(pt.X / (ITEM_WIDTH + GapBetweenCollections));
            if (i < 0 || i >= _scrollColumnInfoSet.Length)
                return new Tuple<int, int, bool>(-1, -1, false);

            var scrollColumnInfo = _scrollColumnInfoSet[i];
            var gpvic = scrollColumnInfo.GameProgramInfoViewItemCollection;
            Sub(ref pt, Struct.ToLocation(scrollColumnInfo.CollectionRect));
            var j = (int)(pt.Y / ITEM_HEIGHT);
            if (j < 0 || j >= gpvic.GameProgramInfoViewItemSet.Length)
                return new Tuple<int, int, bool>(i, -1, false);

            Sub(ref pt, i * (ITEM_WIDTH + GapBetweenCollections), j * ITEM_HEIGHT);
            Sub(ref pt, Struct.ToPointF(ICON_DX, ICON_DY));

            var insideIconRect = pt.X < ICON_WIDTH && pt.Y < ICON_HEIGHT && pt.X >= 0 && pt.Y >= 0;
            return new Tuple<int, int, bool>(i, j, insideIconRect);
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
        {
            var sci = new ScrollColumnInfo { GameProgramInfoViewItemCollection = ToGameProgramInfoViewItemCollectionEx(gpvic) };
            return sci;
        }

        static GameProgramInfoViewItemCollectionEx ToGameProgramInfoViewItemCollectionEx(GameProgramInfoViewItemCollection gpvic)
        {
            var gpvic2 = new GameProgramInfoViewItemCollectionEx
            {
                Name = gpvic.Name,
                GameProgramInfoViewItemSet = gpvic.GameProgramInfoViewItemSet.Select(ToGameProgramInfoViewItemEx).ToArray(),
            };
            return gpvic2;
        }

        static GameProgramInfoViewItemEx ToGameProgramInfoViewItemEx(GameProgramInfoViewItem gpvi)
        {
            var gpvi2 = new GameProgramInfoViewItemEx
            {
                SubTitle = gpvi.SubTitle,
                Title = gpvi.Title,
                ImportedGameProgramInfo = gpvi.ImportedGameProgramInfo
            };
            return gpvi2;
        }

        static GameProgramInfoViewItem ToGameProgramInfoViewItem(GameProgramInfoViewItemEx gpivi)
        {
            var gpivi2 = new GameProgramInfoViewItem
            {
                Title = gpivi.Title,
                SubTitle = gpivi.SubTitle,
                ImportedGameProgramInfo = gpivi.ImportedGameProgramInfo
            };
            return gpivi2;
        }

        #endregion
    }
}
