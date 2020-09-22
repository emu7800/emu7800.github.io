// © Mike Murphy

using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public class ButtonTouchControl : ButtonBase
    {
        #region Fields

        readonly AssetService _assetService = new AssetService();
        readonly Asset _image;
        readonly D2DSolidColorBrush _mouseOverColor;
        StaticBitmap _circleBitmap = StaticBitmapDefault;
        StaticBitmap _imageBitmap = StaticBitmapDefault;

        #endregion

        public bool ExpandBoundingRectangleHorizontally { get; set; }
        public bool ExpandBoundingRectangleVertically { get; set; }

        protected ButtonTouchControl(Asset image, D2DSolidColorBrush mouseOverColor)
        {
            _image = image;
            _mouseOverColor = mouseOverColor;
            Size = Struct.ToSizeF(48f, 48f);
        }

        #region ControlBase Overrides

        public override void Render(GraphicsDevice gd)
        {
            var rect = Struct.ToRectF(Location, Size);
            if (IsPressed)
            {
                gd.FillEllipse(rect, _mouseOverColor);
            }
            gd.DrawBitmap(_circleBitmap, rect);
            gd.DrawBitmap(_imageBitmap, rect);
        }

        protected async override void CreateResources(GraphicsDevice gd)
        {
            base.CreateResources(gd);
            _circleBitmap = await CreateStaticBitmapAsync(gd, Asset.appbar_basecircle_rest);
            _imageBitmap = await CreateStaticBitmapAsync(gd, _image);
        }

        protected override void DisposeResources()
        {
            SafeDispose(ref _circleBitmap);
            SafeDispose(ref _imageBitmap);
            base.DisposeResources();
        }

        #endregion

        protected override RectF ComputeBoundingRectangle()
        {
            var rect = Struct.ToRectF(Location, Size);
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

        async Task<StaticBitmap> CreateStaticBitmapAsync(GraphicsDevice gd, Asset asset)
        {
            var bytesResult = await _assetService.GetAssetBytesAsync(asset);
            return gd.CreateStaticBitmap(bytesResult.Value.Bytes);
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
}