// © Mike Murphy

namespace EMU7800.Services.Dto
{
    public class GameProgramInfoViewItem
    {
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public ImportedGameProgramInfo ImportedGameProgramInfo { get; set; } = new ImportedGameProgramInfo();
    }
}
