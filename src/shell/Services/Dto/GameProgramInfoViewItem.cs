// © Mike Murphy

namespace EMU7800.Services.Dto
{
    public record GameProgramInfoViewItem
    {
        public string Title { get; init; } = string.Empty;
        public string SubTitle { get; init; } = string.Empty;
        public ImportedGameProgramInfo ImportedGameProgramInfo { get; init; } = new();
    }
}
