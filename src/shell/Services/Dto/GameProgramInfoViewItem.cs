// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto
{
    public record GameProgramInfoViewItem
    {
        public string Title { get; }
        public string SubTitle { get; }
        public ImportedGameProgramInfo ImportedGameProgramInfo { get; }

        public GameProgramInfoViewItem(GameProgramInfo gpi, string subTitle, string romPath)
        {
            Title    = gpi.Title;
            SubTitle = subTitle;
            ImportedGameProgramInfo = new(gpi, romPath);
        }

        public GameProgramInfoViewItem(ImportedGameProgramInfo igpi, string subTitle)
        {
            Title    = igpi.GameProgramInfo.Title;
            SubTitle = subTitle;
            ImportedGameProgramInfo = igpi;
        }

        public GameProgramInfoViewItem(MachineType machineType, CartType cartType, Controller lcontroller, Controller rcontroller, string romPath)
        {
            Title    = string.Empty;
            SubTitle = string.Empty;
            ImportedGameProgramInfo = new(new()
            {
                MachineType = machineType,
                CartType    = cartType,
                LController = lcontroller,
                RController = rcontroller
            }, romPath);
        }
    }
}
