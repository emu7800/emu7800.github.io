// © Mike Murphy

namespace EMU7800.Services.Dto
{
    public class ApplicationSettings
    {
        public bool ShowTouchControls { get; set; }
        public int TouchControlSeparation { get; set; }

        public ApplicationSettings ToDeepCopy()
            => new ApplicationSettings
            {
                ShowTouchControls      = this.ShowTouchControls,
                TouchControlSeparation = this.TouchControlSeparation
            };
    }
}