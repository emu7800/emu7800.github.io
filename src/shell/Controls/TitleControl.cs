// © Mike Murphy

using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public sealed class TitleControl : ControlBase
    {
        readonly SizeF _size = Struct.ToSizeF(301, 100);

        StaticBitmap _appicon = StaticBitmapDefault;
        SizeF _appiconSize = Struct.ToSizeF(64, 64);
        RectF _appiconRect;
        TextLayout _titleTextLayout = TextLayoutDefault;
        TextLayout _subTitleTextLayout = TextLayoutDefault;
        PointF _titleTextLocation, _subTitleTextLocation;

        public TitleControl()
        {
            Size = _size;
        }

        #region ControlBase Overrides

        public override void LocationChanged()
        {
            base.LocationChanged();
            _appiconRect = Struct.ToRectF(
                Location,
                _appiconSize
                );
            _titleTextLocation = Struct.ToPointF(Location.X + _appiconSize.Width + 8, Location.Y - 4);
            _subTitleTextLocation = Struct.ToPointF(Location.X + 25, Location.Y + 70);
        }

        public override void Render(GraphicsDevice gd)
        {
            gd.DrawBitmap(_appicon, _appiconRect);
            gd.DrawText(_titleTextLayout, _titleTextLocation, D2DSolidColorBrush.White);
            gd.DrawText(_subTitleTextLayout, _subTitleTextLocation, D2DSolidColorBrush.White);
        }

        protected async override void CreateResources(GraphicsDevice gd)
        {
            // As a convention, GraphicsDevice draw operations accept null objects without throwing to accommodate
            // late arriving artifacts that can occur with async methods such as this.
            // It turns out this is the most holistically-elegant means to accommodate async operations.
            base.CreateResources(gd);
            _titleTextLayout = gd.CreateTextLayout(
                Styles.TitleFontFamily,
                Styles.TitleFontSize,
                "EMU7800",
                300, 64
                );
            _subTitleTextLayout = gd.CreateTextLayout(
                Styles.SubTitleFontFamily,
                Styles.SubTitleFontSize,
                "Classic Atari 2600 and 7800 games",
                300, 64
                );
            _appicon = await CreateStaticBitmapAsync(gd, Asset.appicon_128x128);
        }

        protected override void DisposeResources()
        {
            SafeDispose(ref _appicon);
            SafeDispose(ref _titleTextLayout);
            SafeDispose(ref _subTitleTextLayout);
            base.DisposeResources();
        }

        #endregion

        static async Task<StaticBitmap> CreateStaticBitmapAsync(GraphicsDevice gd, Asset asset)
        {
            var bytes = await AssetService.GetAssetBytesAsync(asset);
            return gd.CreateStaticBitmap(bytes);
        }
    }
}
