// © Mike Murphy

namespace EMU7800.D2D.Shell
{
    public interface IFrameRenderer
    {
        void UpdateDynamicBitmapData(uint[] palette);
        void OnDynamicBitmapDataDelivered();
    }
}