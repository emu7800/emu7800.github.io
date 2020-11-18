// © Mike Murphy

namespace EMU7800.Services.Dto
{
    public enum SpecialBinaryType
    {
        None,
        Bios7800Ntsc,
        Bios7800NtscAlternate,
        Bios7800Pal,
        Hsc7800,
    }

    public record ImportedSpecialBinaryInfo
    {
        public SpecialBinaryType Type { get; init; } = SpecialBinaryType.None;
        public string StorageKey { get; init; } = string.Empty;
    }
}
