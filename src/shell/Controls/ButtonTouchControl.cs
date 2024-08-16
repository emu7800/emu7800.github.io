// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Win32.Interop;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell;

public class ButtonTouchControl : ButtonBase
{
    #region Fields

    readonly Asset _image;
    readonly D2DSolidColorBrush _mouseOverColor;
    StaticBitmap _circleBitmap = StaticBitmap.Default;
    StaticBitmap _imageBitmap = StaticBitmap.Default;

    #endregion

    public bool ExpandBoundingRectangleHorizontally { get; set; }
    public bool ExpandBoundingRectangleVertically { get; set; }

    protected ButtonTouchControl(Asset image, D2DSolidColorBrush mouseOverColor)
    {
        _image = image;
        _mouseOverColor = mouseOverColor;
        Size = new(48f, 48f);
    }

    #region ControlBase Overrides

    public override void Render()
    {
        D2D_RECT_F rect = new(Location, Size);
        if (IsPressed)
        {
            GraphicsDevice.FillEllipse(rect, _mouseOverColor);
        }
        GraphicsDevice.Draw(_circleBitmap, rect);
        GraphicsDevice.Draw(_imageBitmap, rect);
    }

    protected override async void CreateResources()
    {
        base.CreateResources();
        _circleBitmap = await CreateStaticBitmapAsync(Asset.appbar_basecircle_rest);
        _imageBitmap = await CreateStaticBitmapAsync(_image);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _circleBitmap);
        SafeDispose(ref _imageBitmap);
        base.DisposeResources();
    }

    #endregion

    protected override D2D_RECT_F ComputeBoundingRectangle()
    {
        D2D_RECT_F rect = new(Location, Size);
        if (ExpandBoundingRectangleHorizontally)
        {
            rect.Left -= Size.Width;
            rect.Right += Size.Width;
        }
        if (ExpandBoundingRectangleVertically)
        {
            rect.Top -= Size.Height;
            rect.Bottom += Size.Height;
        }
        return rect;
    }

    #region Helpers

    static async Task<StaticBitmap> CreateStaticBitmapAsync(Asset asset)
    {
        var bytes = await AssetService.GetAssetBytesAsync(asset);
        return new StaticBitmap(bytes);
    }

    #endregion
}

public class LeftButton : ButtonTouchControl
{
    public LeftButton() : base(Asset.appbar_transport_playleft_rest, D2DSolidColorBrush.Green)
    {
    }
}

public class RightButton : ButtonTouchControl
{
    public RightButton() : base(Asset.appbar_transport_play_rest, D2DSolidColorBrush.Green)
    {
    }
}

public class UpButton : ButtonTouchControl
{
    public UpButton() : base(Asset.appbar_transport_playup_rest, D2DSolidColorBrush.Green)
    {
    }
}

public class DownButton : ButtonTouchControl
{
    public DownButton() : base(Asset.appbar_transport_playdown_rest, D2DSolidColorBrush.Green)
    {
    }
}

public class FireButton : ButtonTouchControl
{
    public FireButton() : base(Asset.appbar_cancel_rest, D2DSolidColorBrush.Red)
    {
    }
}