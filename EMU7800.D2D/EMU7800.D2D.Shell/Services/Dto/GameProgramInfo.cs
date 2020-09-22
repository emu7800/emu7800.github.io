// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto
{
    public class GameProgramInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Qualifier { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string ModelNo { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public CartType CartType { get; set; } = CartType.Unknown;
        public MachineType MachineType { get; set; } = MachineType.Unknown;
        public Controller LController { get; set; } = Controller.None;
        public Controller RController { get; set; } = Controller.None;
        public string MD5 { get; set; } = string.Empty;
        public string HelpUri { get; set; } = string.Empty;
    }
}
