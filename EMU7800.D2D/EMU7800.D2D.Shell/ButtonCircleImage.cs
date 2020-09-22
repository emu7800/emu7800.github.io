// © Mike Murphy

using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public class ButtonCircleImage : ButtonBase
    {
        #region Fields

        readonly Asset _image, _imageInverted;
        readonly D2DSolidColorBrush _mouseOverColor;
        StaticBitmap _circleBitmap = StaticBitmapDefault;
        StaticBitmap _circleInvertedBitmap = StaticBitmapDefault;
        StaticBitmap _imageBitmap = StaticBitmapDefault;
        StaticBitmap _imageInvertedBitmap = StaticBitmapDefault;

        #endregion

        protected ButtonCircleImage(Asset image, Asset imageInverted, D2DSolidColorBrush mouseOverColor)
        {
            _image = image;
            _imageInverted = imageInverted;
            _mouseOverColor = mouseOverColor;
            Size = Struct.ToSizeF(48f, 48f);
        }

        #region ControlBase Overrides

        public override void Render(GraphicsDevice gd)
        {
            var rect = Struct.ToRectF(Location, Size);

            if (IsPressed)
            {
                gd.FillEllipse(rect, D2DSolidColorBrush.White);
                gd.DrawBitmap(_circleInvertedBitmap, rect);
                gd.DrawBitmap(_imageInvertedBitmap, rect);
            }
            else if (IsMouseOver)
            {
                gd.FillEllipse(rect, _mouseOverColor);
                gd.DrawBitmap(_circleBitmap, rect);
                gd.DrawBitmap(_imageBitmap, rect);
            }
            else
            {
                gd.DrawBitmap(_circleBitmap, rect);
                gd.DrawBitmap(_imageBitmap, rect);
            }
        }

        protected async override void CreateResources(GraphicsDevice gd)
        {
            base.CreateResources(gd);
            _circleBitmap = await CreateStaticBitmapAsync(gd, Asset.appbar_basecircle_rest);
            _circleInvertedBitmap = await CreateStaticBitmapAsync(gd, Asset.appbar_basecircle_rest_inverted);
            _imageBitmap = await CreateStaticBitmapAsync(gd, _image);
            _imageInvertedBitmap = await CreateStaticBitmapAsync(gd, _imageInverted);
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

        static async Task<StaticBitmap> CreateStaticBitmapAsync(GraphicsDevice gd, Asset asset)
        {
            var (_, bytes) = await AssetService.GetAssetBytesAsync(asset);
            return gd.CreateStaticBitmap(bytes);
        }

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