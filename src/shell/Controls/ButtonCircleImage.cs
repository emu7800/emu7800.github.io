// © Mike Murphy

using EMU7800.Assets;
using System.Threading.Tasks;

namespace EMU7800.Shell;

public class ButtonCircleImage : ButtonBase
{
    #region Fields

    readonly Asset _image, _imageInverted;
    readonly SolidColorBrush _mouseOverColor;
    StaticBitmap _circleBitmap = StaticBitmap.Empty;
    StaticBitmap _circleInvertedBitmap = StaticBitmap.Empty;
    StaticBitmap _imageBitmap = StaticBitmap.Empty;
    StaticBitmap _imageInvertedBitmap = StaticBitmap.Empty;

    #endregion

    protected ButtonCircleImage(Asset image, Asset imageInverted, SolidColorBrush mouseOverColor)
    {
        _image = image;
        _imageInverted = imageInverted;
        _mouseOverColor = mouseOverColor;
        Size = new(48f, 48f);
    }

    #region ControlBase Overrides

    public override void Render()
    {
        RectF rect = new(Location, Size);

        if (IsPressed)
        {
            GraphicsDevice.FillEllipse(rect, SolidColorBrush.White);
            GraphicsDevice.Draw(_circleInvertedBitmap, rect);
            GraphicsDevice.Draw(_imageInvertedBitmap, rect);
        }
        else if (IsMouseOver)
        {
            GraphicsDevice.FillEllipse(rect, _mouseOverColor);
            GraphicsDevice.Draw(_circleBitmap, rect);
            GraphicsDevice.Draw(_imageBitmap, rect);
        }
        else
        {
            GraphicsDevice.Draw(_circleBitmap, rect);
            GraphicsDevice.Draw(_imageBitmap, rect);
        }
    }

    protected override async void CreateResources()
    {
        base.CreateResources();
        _circleBitmap = await CreateStaticBitmapAsync(Asset.appbar_basecircle_rest);
        _circleInvertedBitmap = await CreateStaticBitmapAsync(Asset.appbar_basecircle_rest_inverted);
        _imageBitmap = await CreateStaticBitmapAsync(_image);
        _imageInvertedBitmap = await CreateStaticBitmapAsync(_imageInverted);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _circleBitmap);
        SafeDispose(ref _circleInvertedBitmap);
        SafeDispose(ref _imageBitmap);
        SafeDispose(ref _imageInvertedBitmap);
        base.DisposeResources();
    }

    #endregion

    #region Helpers

    static async Task<StaticBitmap> CreateStaticBitmapAsync(Asset asset)
    {
        var bytes = await AssetService.GetAssetBytesAsync(asset);
        return GraphicsDevice.CreateStaticBitmap(bytes.Span);
    }

    #endregion
}

public class BackButton : ButtonCircleImage
{
    public BackButton() : base(Asset.appbar_back_rest, Asset.appbar_back_rest_inverted, SolidColorBrush.Red)
    {
    }
}

public class NextButton : ButtonCircleImage
{
    public NextButton() : base(Asset.appbar_next_rest, Asset.appbar_next_rest_inverted, SolidColorBrush.Green)
    {
    }
}

public class QuestionMarkButton : ButtonCircleImage
{
    public QuestionMarkButton() : base(Asset.appbar_questionmark_rest, Asset.appbar_questionmark_rest_inverted, SolidColorBrush.Blue)
    {
    }
}

public class SearchButton : ButtonCircleImage
{
    public SearchButton() : base(Asset.appbar_feature_search_rest, Asset.appbar_feature_search_rest_inverted, SolidColorBrush.Blue)
    {
    }
}

public class SettingsButton : ButtonCircleImage
{
    public SettingsButton() : base(Asset.appbar_feature_settings_rest, Asset.appbar_feature_settings_rest_inverted, SolidColorBrush.Blue)
    {
    }
}

public class CheckButton : ButtonCircleImage
{
    public CheckButton() : base(Asset.appbar_check_rest, Asset.appbar_check_rest_inverted, SolidColorBrush.Green)
    {
    }
}

public class CancelButton : ButtonCircleImage
{
    public CancelButton() : base(Asset.appbar_cancel_rest, Asset.appbar_cancel_rest_inverted, SolidColorBrush.Red)
    {
    }
}

public class PlusButton : ButtonCircleImage
{
    public PlusButton() : base(Asset.appbar_add_rest, Asset.appbar_add_rest_inverted, SolidColorBrush.Green)
    {
    }
}

public class MinusButton : ButtonCircleImage
{
    public MinusButton() : base(Asset.appbar_minus_rest, Asset.appbar_minus_rest_inverted, SolidColorBrush.Red)
    {
    }
}