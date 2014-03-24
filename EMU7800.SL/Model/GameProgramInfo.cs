using EMU7800.Core;

namespace EMU7800.SL.Model
{
    public enum GameProgramId
    {
        None,
        ActionMan,
        Adventure,
        Asteroids,
        Berzerk,
        Breakout,
        ChopperCommand,
        Combat,
        CosmicArk,
        DragonFire,
        DemonAttack,
        Frogger,
        MissleCommand,
        Oystron,
        Pacman,
        Asteroids78,
        DarkChambers,
        DigDug78,
        DonkeyKong78,
        DonkeyKongJr78,
        Galaga,
        ImpossibleMission,
        Joust,
        MarioBros,
        MsPacMan78,
        Robotron2084,
        SummerGames,
        WinterGames,
        PacManCollection,
        SpaceInvaders,
        SpaceInvaders78,
    }

    public class GameProgramInfo
    {
        public static GameProgramInfo DefaultGameProgram = new GameProgramInfo(GameProgramId.None, CartType.None, null, Controller.None, Controller.None, null);

        public GameProgramId Id { get; private set; }

        public MachineType MachineType { get; private set; }

        public CartType CartType { get; private set; }

        public byte[] RomBytes { get; private set; }

        public ControllerInfo LeftController { get; private set; }

        public ControllerInfo RightController { get; private set; }

        public string Title { get; private set; }

        public string Manufacturer { get; set; }

        public string Year { get; set; }

        public int LeftOffset { get; set; }

        public int ClipStart { get; set; }

        #region Constructors

        public GameProgramInfo(GameProgramId id, CartType cartType, byte[] romBytes, Controller leftController, Controller rightController, string title)
            : this(id, ToMachineType(cartType), cartType, romBytes, ControllerInfo.ToControllerInfo(leftController), ControllerInfo.ToControllerInfo(rightController), title)
        {
        }

        public GameProgramInfo(GameProgramId id, MachineType machineType, CartType cartType, byte[] romBytes, ControllerInfo leftController, ControllerInfo rightController, string title)
        {
            Id = id;
            MachineType = machineType;
            CartType = cartType;
            RomBytes = romBytes ?? new byte[0];
            LeftController = leftController;
            RightController = rightController;
            Title = title ?? string.Empty;
        }

        private GameProgramInfo()
        {
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
