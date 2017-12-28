using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EMU7800.Core;

namespace EMU7800.SL.Model
{
    public class GameProgramRepository
    {
        #region Fields

        static readonly IList<GameProgramInfo> GameProgramCollection;

        #endregion

        public IEnumerable<GameProgramInfo> GetAllGamePrograms()
        {
            return new ReadOnlyCollection<GameProgramInfo>(GameProgramCollection);
        }

        #region Constructors

        static GameProgramRepository()
        {
            var collection = new List<GameProgramInfo>
            {
                new GameProgramInfo(GameProgramId.ActionMan, CartType.A4K, Resources.actionmn, Controller.Joystick, Controller.Joystick, "Atari 2600 - Action Man") { LeftOffset = 3, ClipStart = 30 },
                new GameProgramInfo(GameProgramId.Adventure, CartType.A4K, Resources.ADVNTURE, Controller.Joystick, Controller.Joystick, "Atari 2600 - Adventure"),
                new GameProgramInfo(GameProgramId.Asteroids, CartType.A8K, Resources.ASTEROID, Controller.Joystick, Controller.Joystick, "Atari 2600 - Asteroids"),
                new GameProgramInfo(GameProgramId.Berzerk, CartType.A4K, Resources.Berzerk, Controller.Joystick, Controller.Joystick, "Atari 2600 - Berzerk"),
                new GameProgramInfo(GameProgramId.Breakout, CartType.A2K, Resources.Breakout, Controller.Joystick, Controller.Joystick, "Atari 2600 - Breakout"),
                //new GameProgramInfo(GameProgramId.ChopperCommand, CartType.A4K, Resources.Choprcmd, Controller.Joystick, Controller.Joystick, "Activision 2600 - Chopper Command") { LeftOffset = 4},
                new GameProgramInfo(GameProgramId.Combat, CartType.A2K, Resources.Combat, Controller.Joystick, Controller.Joystick, "Atari 2600 - Combat"),
                new GameProgramInfo(GameProgramId.CosmicArk, CartType.A4K, Resources.Cosmcark, Controller.Joystick, Controller.Joystick, "Imagic 2600 - Cosmic Ark"),
                new GameProgramInfo(GameProgramId.DragonFire, CartType.A4K, Resources.Drgnfire, Controller.Joystick, Controller.Joystick, "Imagic 2600 - Dragon Fire"),
                new GameProgramInfo(GameProgramId.Frogger, CartType.A4K, Resources.Frogger, Controller.Joystick, Controller.Joystick, "Atari 2600 - Frogger"),
                new GameProgramInfo(GameProgramId.MissleCommand, CartType.A4K, Resources.MISSCOMM, Controller.Joystick, Controller.Joystick, "Atari 2600 - Missle Command"),
                new GameProgramInfo(GameProgramId.Oystron, CartType.A4K, Resources.OYSTR29, Controller.Joystick, Controller.Joystick, "Homebrew 2600 - Oystron"),
                new GameProgramInfo(GameProgramId.Pacman, CartType.A4K, Resources.Pacman, Controller.Joystick, Controller.Joystick, "Atari 2600 - Pacman"),
                new GameProgramInfo(GameProgramId.Asteroids78, CartType.A7816, Resources.Asteroids, Controller.Joystick, Controller.Joystick, "Atari 7800 - Asteroids"),
                new GameProgramInfo(GameProgramId.DarkChambers, CartType.A78SG, Resources.Darkchambers, Controller.Joystick, Controller.Joystick, "Atari 7800 - Dark Chambers"),
                new GameProgramInfo(GameProgramId.DigDug78, CartType.A7816, Resources.Digdug, Controller.Joystick, Controller.Joystick, "Atari 7800 - Dig Dug"),
                new GameProgramInfo(GameProgramId.DonkeyKong78, CartType.A7848, Resources.Donkeykong, Controller.Joystick, Controller.Joystick, "Atari 7800 - Donkey Kong"),
                new GameProgramInfo(GameProgramId.DonkeyKongJr78, CartType.A7848, Resources.Donkeykongjr, Controller.Joystick, Controller.Joystick, "Atari 7800 - Donkey Kong Jr."),
                new GameProgramInfo(GameProgramId.ImpossibleMission, CartType.A78SGR, Resources.Impossiblemission, Controller.Joystick, Controller.Joystick, "Atari 7800 - Impossible Mission"),
                new GameProgramInfo(GameProgramId.Joust, CartType.A7832, Resources.Joust, Controller.Joystick, Controller.Joystick, "Atari 7800 - Joust"),
                new GameProgramInfo(GameProgramId.MarioBros, CartType.A7848, Resources.Mariobros, Controller.Joystick, Controller.Joystick, "Atari 7800 - Mario Bros."),
                new GameProgramInfo(GameProgramId.MsPacMan78, CartType.A7816, Resources.Mspacman, Controller.Joystick, Controller.Joystick, "Atari 7800 - Ms. Pac Man"),
                new GameProgramInfo(GameProgramId.Robotron2084, CartType.A7832, Resources.Robotron2084, Controller.Joystick, Controller.Joystick, "Atari 7800 - Robotron 2084"),
                new GameProgramInfo(GameProgramId.SummerGames, CartType.A78SGR, Resources.Summergames, Controller.Joystick, Controller.Joystick, "Atari 7800 - Summer Games"),
                new GameProgramInfo(GameProgramId.WinterGames, CartType.A78SGR, Resources.Wintergames, Controller.Joystick, Controller.Joystick, "Atari 7800 - Winter Games"),
                new GameProgramInfo(GameProgramId.Galaga, CartType.A7832, Resources.Galaga, Controller.Joystick, Controller.Joystick, "Atari 7800 - Galaga"),
                new GameProgramInfo(GameProgramId.PacManCollection, CartType.A7832, Resources.PACCOLL, Controller.ProLineJoystick, Controller.ProLineJoystick, "Homebrew 7800 - PacMan Collection"),
                new GameProgramInfo(GameProgramId.DemonAttack, CartType.A4K, Resources.Demonatk, Controller.Joystick, Controller.Joystick, "Imagic 2600 - Demon Attack"),
                new GameProgramInfo(GameProgramId.SpaceInvaders, CartType.A4K, Resources.SPCINVAD, Controller.Joystick, Controller.Joystick, "Atari 2600 - Space Invaders"),
                new GameProgramInfo(GameProgramId.SpaceInvaders78, CartType.A7816, Resources.SI7800, Controller.Joystick, Controller.Joystick, "Homebrew 7800 - Space Invaders"),
            };

            var query = from g in collection
                        orderby g.Title ascending
                        select g;
            GameProgramCollection = query.ToList();
        }

        #endregion
    }
}
