// © Mike Murphy

namespace EMU7800.Services.Dto;

public record ApplicationSettings
{
    public bool ShowTouchControls { get; set; }
    public int TouchControlSeparation { get; set; }
}