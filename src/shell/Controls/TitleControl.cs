// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Win32.Interop;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell;

public sealed class TitleControl : ControlBase
{
    readonly D2D_SIZE_F _size = new(301, 100);

    StaticBitmap _appicon = StaticBitmap.Default;
    D2D_SIZE_F _appiconSize = new(64, 64);
    D2D_RECT_F _appiconRect;
    TextLayout _titleTextLayout = TextLayout.Default;
    TextLayout _subTitleTextLayout = TextLayout.Default;
    D2D_POINT_2F _titleTextLocation, _subTitleTextLocation;

    public TitleControl()
    {
        Size = _size;
    }

    #region ControlBase Overrides

    public override void LocationChanged()
    {
        base.LocationChanged();
        _appiconRect = new(Location, _appiconSize);
        _titleTextLocation = new(Location.X + _appiconSize.Width + 8, Location.Y - 4);
        _subTitleTextLocation = new(Location.X + 25, Location.Y + 70);
    }

    public override void Render()
    {
        GraphicsDevice.Draw(_appicon, _appiconRect);
        GraphicsDevice.Draw(_titleTextLayout, _titleTextLocation, D2DSolidColorBrush.White);
        GraphicsDevice.Draw(_subTitleTextLayout, _subTitleTextLocation, D2DSolidColorBrush.White);
    }

    protected override async void CreateResources()
    {
        // As a convention, GraphicsDevice draw operations accept null objects without throwing to accommodate
        // late arriving artifacts that can occur with async methods such as this.
        // It turns out this is the most holistically-elegant means to accommodate async operations.
        base.CreateResources();
        _titleTextLayout = new TextLayout(
            Styles.TitleFontFamily,
            Styles.TitleFontSize,
            "EMU7800",
            300, 64
            );
        _subTitleTextLayout = new TextLayout(
            Styles.SubTitleFontFamily,
            Styles.SubTitleFontSize,
            "Classic Atari 2600 and 7800 games",
            300, 64
            );
        _appicon = await CreateStaticBitmapAsync(Asset.appicon_128x128);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _appicon);
        SafeDispose(ref _titleTextLayout);
        SafeDispose(ref _subTitleTextLayout);
        base.DisposeResources();
    }

    #endregion

    static async Task<StaticBitmap> CreateStaticBitmapAsync(Asset asset)
    {
        var bytes = await AssetService.GetAssetBytesAsync(asset);
        return new StaticBitmap(bytes);
    }
}