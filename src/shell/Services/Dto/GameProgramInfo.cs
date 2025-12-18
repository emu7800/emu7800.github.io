// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto;

public record GameProgramInfo(
    string Title,
    string Manufacturer,
    string Author,
    string Qualifier,
    string Year,
    string ModelNo,
    string Rarity,
    CartType CartType,
    MachineType MachineType,
    Controller LController,
    Controller RController,
    string MD5,
    string HelpUri)
{
    public readonly static GameProgramInfo Default = new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        CartType.Unknown,
        MachineType.Unknown,
        Controller.None,
        Controller.None,
        string.Empty,
        string.Empty);
}
