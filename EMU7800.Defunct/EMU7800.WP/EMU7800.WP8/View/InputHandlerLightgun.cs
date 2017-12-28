using EMU7800.Core;
using EMU7800.WP8.Interop;
using System;
using Windows.UI.Core;

namespace EMU7800.WP.View
{
    public class InputHandlerLightgun : InputHandler
    {
        #region Fields

        readonly Direct3DInterop _interop;

        #endregion

        public override void OnPointerPressed(PointerEventArgs args)
        {
            var x = args.CurrentPoint.Position.X;
            var y = args.CurrentPoint.Position.Y;
            RaiseLightgunInput(x, y, true);
        }

        public override void OnPointerReleased(PointerEventArgs args)
        {
            var x = args.CurrentPoint.Position.X;
            var y = args.CurrentPoint.Position.Y;
            RaiseLightgunInput(x, y, false);
        }

        #region Constructors

        public InputHandlerLightgun(MachineBase machine, Direct3DInterop interop) : base(machine)
        {
            if (interop == null)
                throw new ArgumentNullException("interop");
            _interop = interop;
        }

        #endregion

        #region Helpers

        void RaiseLightgunInput(double x, double y, bool down)
        {
            var scaleToDip = 100.0 / _interop.ScaleFactor;

            var tx = x - _interop.DestRectLeft * scaleToDip;
            var ty = y - _interop.DestRectTop * scaleToDip;

            var destWidth  = (_interop.DestRectRight - _interop.DestRectLeft) * scaleToDip;
            var destHeight = (_interop.DestRectBottom - _interop.DestRectTop) * scaleToDip;

            if (tx < 0 || ty < 0 || tx > destWidth || ty > destHeight)
                return;

            var sfx = 320 / destWidth;
            var sfy = 240 / destHeight;

            var scanline = (int)(ty * sfy) + 16;
            var hpos     = (int)(tx * sfx);
            RaiseMachineLightgunInput(scanline, hpos, down);
        }

        #endregion
    }
}
