// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto
{
    public record GameProgramInfo
    {
        public string Title { get; init; } = string.Empty;
        public string Manufacturer { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Qualifier { get; init; } = string.Empty;
        public string Year { get; init; } = string.Empty;
        public string ModelNo { get; init; } = string.Empty;
        public string Rarity { get; init; } = string.Empty;
        public CartType CartType { get; init; } = CartType.Unknown;
        public MachineType MachineType { get; init; } = MachineType.Unknown;
        public Controller LController { get; init; } = Controller.None;
        public Controller RController { get; init; } = Controller.None;
        public string MD5 { get; init; } = string.Empty;
        public string HelpUri { get; init; } = string.Empty;
    }
}
