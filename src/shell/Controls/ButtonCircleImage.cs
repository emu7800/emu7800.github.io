// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Win32.Interop;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell
{
    public class ButtonCircleImage : ButtonBase
    {
        #region Fields

        readonly Asset _image, _imageInverted;
        readonly D2DSolidColorBrush _mouseOverColor;
        StaticBitmap _circleBitmap = StaticBitmap.Default;
        StaticBitmap _circleInvertedBitmap = StaticBitmap.Default;
        StaticBitmap _imageBitmap = StaticBitmap.Default;
        StaticBitmap _imageInvertedBitmap = StaticBitmap.Default;

        #endregion

        protected ButtonCircleImage(Asset image, Asset imageInverted, D2DSolidColorBrush mouseOverColor)
        {
            _image = image;
            _imageInverted = imageInverted;
            _mouseOverColor = mouseOverColor;
            Size = new(48f, 48f);
        }

        #region ControlBase Overrides

        public override void Render()
        {
            D2D_RECT_F rect = new(Location, Size);

            if (IsPressed)
            {
                GraphicsDevice.FillEllipse(rect, D2DSolidColorBrush.White);
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

        protected async override void CreateResources()
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
            => new StaticBitmap(await AssetService.GetAssetBytesAsync(asset));

        #endregion
    }

    public class BackButton : ButtonCircleImage
    {
        public BackButton() : base(Asset.appbar_back_rest, Asset.appbar_back_rest_inverted, D2DSolidColorBrush.Red)
        {
        }
    }

    public class NextButton : ButtonCircleImage
    {
        public NextButton() : base(Asset.appbar_next_rest, Asset.appbar_next_rest_inverted, D2DSolidColorBrush.Green)
        {
        }
    }

    public class QuestionMarkButton : ButtonCircleImage
    {
        public QuestionMarkButton() : base(Asset.appbar_questionmark_rest, Asset.appbar_questionmark_rest_inverted, D2DSolidColorBrush.Blue)
        {
        }
    }

    public class SearchButton : ButtonCircleImage
    {
        public SearchButton() : base(Asset.appbar_feature_search_rest, Asset.appbar_feature_search_rest_inverted, D2DSolidColorBrush.Blue)
        {
        }
    }

    public class SettingsButton : ButtonCircleImage
    {
        public SettingsButton() : base(Asset.appbar_feature_settings_rest, Asset.appbar_feature_settings_rest_inverted, D2DSolidColorBrush.Blue)
        {
        }
    }

    public class CheckButton : ButtonCircleImage
    {
        public CheckButton() : base(Asset.appbar_check_rest, Asset.appbar_check_rest_inverted, D2DSolidColorBrush.Green)
        {
        }
    }

    public class CancelButton : ButtonCircleImage
    {
        public CancelButton() : base(Asset.appbar_cancel_rest, Asset.appbar_cancel_rest_inverted, D2DSolidColorBrush.Red)
        {
        }
    }

    public class PlusButton : ButtonCircleImage
    {
        public PlusButton() : base(Asset.appbar_add_rest, Asset.appbar_add_rest_inverted, D2DSolidColorBrush.Green)
        {
        }
    }

    public class MinusButton : ButtonCircleImage
    {
        public MinusButton() : base(Asset.appbar_minus_rest, Asset.appbar_minus_rest_inverted, D2DSolidColorBrush.Red)
        {
        }
    }
}