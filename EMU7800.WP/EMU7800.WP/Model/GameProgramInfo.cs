using System;

using EMU7800.Core;

namespace EMU7800.WP.Model
{
    public class GameProgramInfo
    {
        public GameProgramId Id { get; private set; }

        public MachineType MachineType { get; private set; }

        public CartType CartType { get; private set; }

        public Func<byte[]> RomBytes { get; private set; }

        public ControllerInfo Controller { get; private set; }

        public string Title { get; private set; }

        public string Manufacturer { get; set; }

        public string Year { get; set; }

        public int LeftOffset { get; set; }

        public int ClipStart { get; set; }

        #region Constructors

        public GameProgramInfo(GameProgramId id, CartType cartType, Func<byte[]> romBytes, Controller controller, string title, string manufacturer)
        {
            if (romBytes == null)
                throw new ArgumentNullException("romBytes");

            Id = id;
            MachineType = ToMachineType(cartType);
            CartType = cartType;
            RomBytes = romBytes;
            Controller = ControllerInfo.ToControllerInfo(controller);
            Title = title ?? string.Empty;
            Manufacturer = manufacturer ?? string.Empty;
        }

        #endregion

        #region Helpers

        static MachineType ToMachineType(CartType cartType)
        {
            switch (cartType)
            {
                case CartType.None:
                    return MachineType.None;
                case CartType.A7808:
                case CartType.A7816:
                case CartType.A7832:
                case CartType.A7832P:
                case CartType.A7848:
                case CartType.A78SG:
                case CartType.A78SGP:
                case CartType.A78SGR:
                case CartType.A78S9:
                case CartType.A78S4:
                case CartType.A78S4R:
                case CartType.A78AB:
                case CartType.A78AC:
                    return MachineType.A7800NTSC;
                default:
                    return MachineType.A2600NTSC;
            }
        }

        #endregion
    }
}
