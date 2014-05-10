using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using EMU7800.Core;

namespace EMU7800.WP.Model
{
    public class GameProgramInfoRepository
    {
        #region Game Program Library

        public const string
            ManufacturerAbsolute = "Absolute",
            ManufacturerActivision = "Activision",
            ManufacturerAtari = "Atari",
            ManufacturerColeco = "Coleco",
            ManufacturerFroggo = "Froggo",
            ManufacturerHomebrew = "Homebrew",
            ManufacturerImagic = "Imagic",
            ManufacturerParkerBrothers = "Parker Brothers";

        static readonly IEnumerable<GameProgramInfo> GameProgramCollection = new Collection<GameProgramInfo>
        {
            new GameProgramInfo(GameProgramId.Arkanoid78, CartType.A7832P, () => GetRomData("arkanoid0911"), Controller.Joystick, "Arkanoid", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.AceOfAces, CartType.A78SG, () => GetRomData("Aceofaces"), Controller.ProLineJoystick, "Ace of Aces", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Adventure, CartType.A4K, () => GetRomData("ADVNTURE"), Controller.Joystick, "Adventure", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.AlienBrigade, CartType.A78S9, () => GetRomData("Alienbrigade"), Controller.Lightgun, "Alien Brigade", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.AppleSnaffle, CartType.A78SGP, () => GetRomData("AppleSnaffle_23_07_10.1.30", true), Controller.ProLineJoystick, "Apple Snaffle", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.ArmorAttackII, CartType.A7816, () => GetRomData("ARMORATK"), Controller.ProLineJoystick, "Armor Attack II", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Asteroids, CartType.A8K, () => GetRomData("ASTEROID"), Controller.Joystick, "Asteroids", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Asteroids78, CartType.A7816, () => GetRomData("Asteroids"), Controller.ProLineJoystick, "Asteroids", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.AsteroidsDeluxe, CartType.A7832, () => GetRomData("ASTDLX78"), Controller.ProLineJoystick, "Asteroids Deluxe", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Atlantis, CartType.A4K, () => GetRomData("ATLANTIS"), Controller.Joystick, "Atlantis", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.BallBlazer, CartType.A7832P, () => GetRomData("Ballblazer"), Controller.Joystick, "Ballblazer", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Barnstorming, CartType.A4K, () => GetRomData("Barnstrm"), Controller.Joystick, "Barnstorming", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.BarnyardBlaster, CartType.A78SG, () => GetRomData("Barnyardblaster"), Controller.Lightgun, "Barnyard Blaster", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Basketbrawl, CartType.A78SG, () => GetRomData("Basketbrawl"), Controller.ProLineJoystick, "Basketbrawl", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Beamrider, CartType.A8K, () => GetRomData("Beamride"), Controller.Joystick, "Beamrider", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.BeefDrop, CartType.A7832, () => GetRomData("BeefDrop"), Controller.Joystick, "Beef Drop", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.BentleyBear, CartType.A78S9, () => GetRomData("Bentley Bear - Crystal Quest (SP)", true), Controller.ProLineJoystick, "Bentley Bear", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Berzerk, CartType.A4K, () => GetRomData("Berzerk"), Controller.Joystick, "Berzerk", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.bnQ, CartType.A7848, () => GetRomData("bnQ"), Controller.Joystick, "b*nQ", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Bowling, CartType.A2K, () => GetRomData("Bowling"), Controller.Joystick, "Bowling", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Boxing, CartType.A2K, () => GetRomData("Boxing"), Controller.Joystick, "Boxing", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Breakout, CartType.A2K, () => GetRomData("Breakout"), Controller.Paddles, "Breakout", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Bridge, CartType.A4K, () => GetRomData("Bridge"), Controller.Joystick, "Bridge", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Centipede, CartType.A7816, () => GetRomData("Centipede"), Controller.Joystick, "Centipede", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Checkers, CartType.A2K, () => GetRomData("Checkact"), Controller.Joystick, "Checkers", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Choplifter, CartType.A7832, () => GetRomData("Choplifter"), Controller.ProLineJoystick, "Choplifter!", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.ChopperCommand, CartType.A4K, () => GetRomData("Choprcmd"), Controller.Joystick, "Chopper Command", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.CircusAtari, CartType.A4K, () => GetRomData("Circatri"), Controller.Paddles, "Circus Atari", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.ColorTest, CartType.A7808, () => GetRomData("ColorTest"), Controller.Joystick, "Color Test", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Combat, CartType.A2K, () => GetRomData("Combat"), Controller.Joystick, "Combat", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Commando, CartType.A78SGP, () => GetRomData("Commando"), Controller.ProLineJoystick, "Commando", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.CosmicArk, CartType.A4K, () => GetRomData("Cosmcark"), Controller.Joystick, "Cosmic Ark", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.CosmicCommuter, CartType.A4K, () => GetRomData("Csmcomtr"), Controller.Joystick, "Cosmic Commuter", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Cracked, CartType.A78SG, () => GetRomData("Cracked"), Controller.ProLineJoystick, "Cracked", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Crackpots, CartType.A4K, () => GetRomData("Crackpot"), Controller.Joystick, "Crackpots", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.CrazyBrix, CartType.A7816, () => GetRomData("CrazyBricks"), Controller.ProLineJoystick, "Crazy Brix", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Crossbow, CartType.A78S9, () => GetRomData("Crossbow"), Controller.Lightgun, "Crossbow", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.DarkChambers, CartType.A78SG, () => GetRomData("Darkchambers"), Controller.Joystick, "Dark Chambers", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Decathlon, CartType.DC8K, () => GetRomData("Decathln"), Controller.Joystick, "Decathlon", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.DemonAttack, CartType.A4K, () => GetRomData("Demonatk"), Controller.Joystick, "Demon Attack", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.DesertFalcon, CartType.A7848, () => GetRomData("Desertfalcon"), Controller.ProLineJoystick, "Desert Falcon", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.DigDug78, CartType.A7816, () => GetRomData("Digdug"), Controller.Joystick, "Dig Dug", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Dolphin, CartType.A4K, () => GetRomData("Dolphin"), Controller.Joystick, "Dolphin", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.DonkeyKong, CartType.A4K, () => GetRomData("Dk"), Controller.Joystick, "Donkey Kong", ManufacturerColeco),
            new GameProgramInfo(GameProgramId.DonkeyKong78, CartType.A7848, () => GetRomData("Donkeykong"), Controller.Joystick, "Donkey Kong", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.DonkeyKongXm, CartType.A78S9, () => GetRomData("dkxm_rc8_ntsc", true), Controller.Joystick, "Donkey Kong XM", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.DonkeyKongJr, CartType.A8K, () => GetRomData("Dkjr"), Controller.Joystick, "Donkey Kong Jr.", ManufacturerColeco),
            new GameProgramInfo(GameProgramId.DonkeyKongJr78, CartType.A7848, () => GetRomData("Donkeykongjr"), Controller.Joystick, "Donkey Kong Jr.", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.DoubleDragon, CartType.A16K, () => GetRomData("Dbldragn"), Controller.Joystick, "Double Dragon", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.DoubleDragon78, CartType.A78AC, () => GetRomData("Doubledragon"), Controller.ProLineJoystick, "Double Dragon", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.DragonFire, CartType.A4K, () => GetRomData("Drgnfire"), Controller.Joystick, "Dragonfire", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Dragster, CartType.A2K, () => GetRomData("Dragster"), Controller.Joystick, "Dragster", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Enduro, CartType.A4K, () => GetRomData("Enduro_a"), Controller.Joystick, "Enduro", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.ET, CartType.A8K, () => GetRomData("E_t"), Controller.Joystick, "ET", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.ETFixed, CartType.A8K, () => GetRomData("ET_Fixed_Final"), Controller.Joystick, "ET Fixed", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.F18Hornet, CartType.A78AB, () => GetRomData("F18Hornet"), Controller.ProLineJoystick, "F18 Hornet", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.FailSafe, CartType.A7848, () => GetRomData("FAILSAFE"), Controller.Joystick, "FailSafe", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Fathom, CartType.A8K, () => GetRomData("Fathom"), Controller.Joystick, "Fathom", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.FightNight, CartType.A78SG, () => GetRomData("Fightnight"), Controller.ProLineJoystick, "Fight Night", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Firefighter, CartType.A4K, () => GetRomData("Firefite"), Controller.Joystick, "Fire Fighter", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.FishingDerby, CartType.A2K, () => GetRomData("Fishdrby"), Controller.Joystick, "Fishing Derby", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Foodfight, CartType.A7832, () => GetRomData("Foodfight"), Controller.Joystick, "Food Fight", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Freeway, CartType.A2K, () => GetRomData("Freeway"), Controller.Joystick, "Freeway", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Frogger, CartType.A4K, () => GetRomData("Frogger"), Controller.Joystick, "Frogger", ManufacturerParkerBrothers),
            new GameProgramInfo(GameProgramId.Frogger78, CartType.A7848, () => GetRomData("FROGGER78"), Controller.Joystick, "Frogger", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Frostbite, CartType.A4K, () => GetRomData("Frostbit"), Controller.Joystick, "Frostbite", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Galaga, CartType.A7832, () => GetRomData("Galaga"), Controller.Joystick, "Galaga", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Ghostbusters, CartType.A8K, () => GetRomData("Ghostbst"), Controller.Joystick, "Ghostbusters", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.GrandPrix, CartType.A4K, () => GetRomData("Grandprx"), Controller.Joystick, "Grandprix", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.HarrysHenHouse, CartType.A7832, () => GetRomData("hhh_15_04_09.1.04"), Controller.ProLineJoystick, "Harrys Hen House", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.HatTrick, CartType.A7848, () => GetRomData("Hattrick"), Controller.Joystick, "Hat Trick", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.HauntedHouse, CartType.A4K, () => GetRomData("Haunthse"), Controller.Joystick, "Haunted House", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Hero, CartType.A8K, () => GetRomData("Hero"), Controller.Joystick, "H.E.R.O.", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.IceHockey, CartType.A4K, () => GetRomData("Icehocky"), Controller.Joystick, "Ice Hockey", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.IkariWarriors, CartType.A78SG, () => GetRomData("Ikariwarriors"), Controller.ProLineJoystick, "Ikari Warriors", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.ImpossibleMission, CartType.A78SGR, () => GetRomData("Impossiblemission"), Controller.Joystick, "Impossible Mission", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Jinks, CartType.A78SGR, () => GetRomData("Jinks"), Controller.ProLineJoystick, "Jinks", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Joust, CartType.A7832, () => GetRomData("Joust"), Controller.Joystick, "Joust", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.JrPacMan78, CartType.A7832P, () => GetRomData("jrpac_78_tunnels"), Controller.Joystick, "Jr. Pac Man", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Kabobber, CartType.A4K, () => GetRomData("kabobber"), Controller.Joystick, "Kabobber Prototype", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Kaboom, CartType.A2K, () => GetRomData("Kaboom"), Controller.Paddles, "Kaboom!", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Karateka, CartType.A7848, () => GetRomData("Karateka"), Controller.ProLineJoystick, "Karateka", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.KeystoneKapers, CartType.A4K, () => GetRomData("Keystone"), Controller.Joystick, "Keystone Kapers", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Klax, CartType.A78SG, () => GetRomData("Klax"), Controller.ProLineJoystick, "Klax", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.KungFuMaster, CartType.A8K, () => GetRomData("Kung_fu"), Controller.Joystick, "Kung-Fu Master", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.KungFuMaster78, CartType.A7832, () => GetRomData("Kungfumaster"), Controller.ProLineJoystick, "Kung-Fu Master", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.LaserBlast, CartType.A2K, () => GetRomData("LASRBLST"), Controller.Joystick, "Laser Blast", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.ManGoesDown, CartType.A16K, () => GetRomData("mgd"), Controller.Joystick, "Man Goes Down", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.MarioBros, CartType.A7848, () => GetRomData("Mariobros"), Controller.Joystick, "Mario Brothers", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MatManiaChallenge, CartType.A78SG, () => GetRomData("Matmaniachallenge"), Controller.ProLineJoystick, "Mat Mania Challenge", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Mean18UltimateGolf, CartType.A78SG, () => GetRomData("Mean18"), Controller.ProLineJoystick, "Mean 18 Ultimate Golf", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Meltdown, CartType.A78SG, () => GetRomData("Meltdown"), Controller.Lightgun, "Meltdown", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Megamania, CartType.A4K, () => GetRomData("MEGAMAN"), Controller.Joystick, "Megamania", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.MeteorShower, CartType.A7816, () => GetRomData("MeteorSh"), Controller.Joystick, "Meteor Shower", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.MidnightMagic, CartType.A16K, () => GetRomData("MIDNIGHT"), Controller.Joystick, "Midnight Magic", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MidnightMutants, CartType.A78SG, () => GetRomData("Midnightmutants"), Controller.ProLineJoystick, "Midnight Mutants", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MissleCommand, CartType.A4K, () => GetRomData("MISSCOMM"), Controller.Joystick, "Missle Command", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MoonCresta, CartType.A7832, () => GetRomData("MoonCresta"), Controller.Joystick, "Moon Cresta", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Moonsweeper, CartType.A8K, () => GetRomData("moonswep"), Controller.Joystick, "Moonsweeper", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.MotorPsycho, CartType.A78SG, () => GetRomData("Motorpsycho"), Controller.ProLineJoystick, "Motor Psycho", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MsPacMan78, CartType.A7816, () => GetRomData("Mspacman"), Controller.Joystick, "Ms Pac-Man", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.MsPacMan320, CartType.A7832P, () => GetRomData("MsPac320Pokey", true), Controller.ProLineJoystick, "Ms Pac-Man 320", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.NightDriver, CartType.A2K, () => GetRomData("NIGHTDRV"), Controller.Paddles, "Night Driver", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.NinjaGolf, CartType.A78SG, () => GetRomData("Ninjagolf"), Controller.ProLineJoystick, "Ninja Golf", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.NoEscape, CartType.A4K, () => GetRomData("NOESCAPE"), Controller.Joystick, "No Escape", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Oink, CartType.A4K, () => GetRomData("Oink"), Controller.Joystick, "Oink", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.OneOnOneBasketball, CartType.A7848, () => GetRomData("Oneonone"), Controller.ProLineJoystick, "One on One", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Oystron, CartType.A4K, () => GetRomData("OYSTR29"), Controller.Joystick, "Oystron", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Pacman, CartType.A4K, () => GetRomData("Pacman"), Controller.Joystick, "Pac-Man", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.PacManCollection, CartType.A7832, () => GetRomData("PACCOLL"), Controller.ProLineJoystick, "Pac-Man Collection", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Pacman320, CartType.A7832P, () => GetRomData("Pac320Pokey", true), Controller.ProLineJoystick, "Pac-Man 320", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.PeteRoseBaseball, CartType.A7832, () => GetRomData("Peterosebaseball"), Controller.ProLineJoystick, "Pete Rose Baseball", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.Phoenix, CartType.A8K, () => GetRomData("Phoenix"), Controller.Joystick, "Phoenix", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Pitfall, CartType.A4K, () => GetRomData("Pitfall"), Controller.Joystick, "Pitfall", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Pitfall2, CartType.DPC, () => GetRomData("Pitfall2"), Controller.Joystick, "Pitfall II", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.PlanetSmashers, CartType.A78SG, () => GetRomData("Planetsmashers"), Controller.ProLineJoystick, "Planet Smashers", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.PlaqueAttack, CartType.A4K, () => GetRomData("Plaqattk"), Controller.Joystick, "Plaque Attack", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Plutos, CartType.A78SGR, () => GetRomData("PLUTOS"), Controller.ProLineJoystick, "Plutos", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.PolePositionII, CartType.A7832, () => GetRomData("Poleposition2"), Controller.ProLineJoystick, "Pole Position II", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.PressureCooker, CartType.A8K, () => GetRomData("Pressure"), Controller.Joystick, "Pressure Cooker", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.PrivateEye, CartType.A8K, () => GetRomData("Priveye"), Controller.Joystick, "Private Eye", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Qbert, CartType.A7832, () => GetRomData("Qbert"), Controller.Joystick, "Q*Bert", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.QuickStep, CartType.A4K, () => GetRomData("Quickstp"), Controller.Joystick, "Quick Step", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Rampage, CartType.A16K, () => GetRomData("Rampage"), Controller.Joystick, "Rampage", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.RealSportsBaseball, CartType.A78S4, () => GetRomData("RSbaseball"), Controller.ProLineJoystick, "Real Sports Baseball", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.RiddleOfTheSphinx, CartType.A4K, () => GetRomData("Riddle"), Controller.Joystick, "Riddle of the Sphinx", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.RipOff, CartType.A7816, () => GetRomData("RipOff"), Controller.ProLineJoystick, "Rip Off", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.RiverRaid, CartType.A4K, () => GetRomData("Riveraid"), Controller.Joystick, "River Raid", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.RobotFindsKitten, CartType.A7832, () => GetRomData("RobotFindsKitten78"), Controller.Joystick, "Robot Finds Kitten", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.RobotTank, CartType.DC8K, () => GetRomData("Robotank"), Controller.Joystick, "Robot Tank", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Robotron2084, CartType.A7832, () => GetRomData("Robotron2084"), Controller.Joystick, "Robotron 2084", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SantaSimon, CartType.A7848, () => GetRomData("SantaSimon"), Controller.Joystick, "Santa Simon", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Scramble, CartType.A7848, () => GetRomData("Scramble"), Controller.ProLineJoystick, "Scramble", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.ScrapYardDog, CartType.A78SG, () => GetRomData("Scrapyarddog"), Controller.ProLineJoystick, "Scrapyard Dog", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Sirius, CartType.A78SGR, () => GetRomData("SIRIUS"), Controller.ProLineJoystick, "Sirius", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Seaquest, CartType.A4K, () => GetRomData("SEAQUEST"), Controller.Joystick, "Seaquest", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Sentinel, CartType.A78SG, () => GetRomData("Sentinel"), Controller.Lightgun, "Sentinel", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.ShootinGallery, CartType.A4K, () => GetRomData("SHOOTIN"), Controller.Joystick, "Shootin' Gallery", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Skiing, CartType.A2K, () => GetRomData("SKIING"), Controller.Joystick, "Skiing", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.SkyDiver, CartType.A2K, () => GetRomData("Skydiver"), Controller.Joystick, "Sky Diver", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SkyJinks, CartType.A2K, () => GetRomData("SKYJINKS"), Controller.Joystick, "Sky Jinks", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Solaris, CartType.A16K, () => GetRomData("SOLARIS"), Controller.Joystick, "Solaris", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SolarStorm, CartType.A4K, () => GetRomData("SOLRSTRM"), Controller.Paddles, "Solar Storm", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.SpaceDuel, CartType.A7832, () => GetRomData("SPACDUEL"), Controller.ProLineJoystick, "Space Duel", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.SpaceInvaders, CartType.A4K, () => GetRomData("SPCINVAD"), Controller.Joystick, "Space Invaders", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SpaceInvaders78, CartType.A7816, () => GetRomData("SI7800"), Controller.Joystick, "Space Invaders", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.SpaceShuttle, CartType.A8K, () => GetRomData("SPCSHUTL"), Controller.Joystick, "Space Shuttle", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.SpiderFighter, CartType.A16K, () => GetRomData("spiderfi"), Controller.Joystick, "Spider Fighter", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.StarVoyager, CartType.A4K, () => GetRomData("STARVYGR"), Controller.Joystick, "Star Voyager", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Stampede, CartType.A2K, () => GetRomData("STAMPEDE"), Controller.Joystick, "Stampede", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Starmaster, CartType.A4K, () => GetRomData("STARMAST"), Controller.Joystick, "Starmaster", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Subterranea, CartType.A8K, () => GetRomData("Subterranea"), Controller.Joystick, "Subterranea", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.SummerGames, CartType.A78SGR, () => GetRomData("Summergames"), Controller.Joystick, "Summer Games", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SuperBreakout, CartType.A4K, () => GetRomData("superb"), Controller.Paddles, "Super Breakout", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SuperCircusAtariAge, CartType.A7832P, () => GetRomData("SUPERCIR"), Controller.ProLineJoystick, "Super Circus AtariAge", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.SuperHuey, CartType.A7848, () => GetRomData("Superhuey"), Controller.ProLineJoystick, "Super Huey UH-IX", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Superman1, CartType.A4K, () =>GetRomData("SUPRMAN1"), Controller.Joystick, "Superman", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.SuperPacMan, CartType.A7832, () => GetRomData("SUPERPAC", true), Controller.ProLineJoystick, "Super Pac-Man", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.SuperSkateboardin, CartType.A7832, () => GetRomData("Superskateboardin"), Controller.ProLineJoystick, "Super Skateboardin'", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.TankCommand, CartType.A78S4, () => GetRomData("Tankcommand"), Controller.ProLineJoystick, "Tank Command", ManufacturerFroggo),
            new GameProgramInfo(GameProgramId.Tennis, CartType.A2K, () => GetRomData("Tennis"), Controller.Joystick, "Tennis", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.Thwocker, CartType.DC8K, () => GetRomData("Thwocker"), Controller.Joystick, "Thwocker Prototype", ManufacturerActivision),
            new GameProgramInfo(GameProgramId.TitleMatch, CartType.A7832, () => GetRomData("Titlematch"), Controller.ProLineJoystick, "Titlematch Pro Wrestling", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.TouchdownFootball, CartType.A78SG, () => GetRomData("Touchdownfootball"), Controller.ProLineJoystick, "Touchdown Football", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.TomcatF14, CartType.A7832, () => GetRomData("Tomcatf14"), Controller.ProLineJoystick, "Tomcat F14", ManufacturerAbsolute),
            new GameProgramInfo(GameProgramId.Tubes, CartType.A7816, () => GetRomData("Tubes", true), Controller.ProLineJoystick, "Tubes", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.VideoCheckers, CartType.A4K, () => GetRomData("Vidcheck"), Controller.Joystick, "Video Checkers", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.VideoChess, CartType.A4K, () => GetRomData("Vidchess"), Controller.Joystick, "Video Chess", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.VideoOlympics, CartType.A2K, () => GetRomData("Vid_olym"), Controller.Paddles, "Video Olympics", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.VideoPinball, CartType.A4K, () => GetRomData("Vidpin"), Controller.Joystick, "Video Pinball", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Warlords, CartType.A4K, () => GetRomData("Warlords"), Controller.Paddles, "Warlords", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Wasp, CartType.A7832, () => GetRomData("WaspSE"), Controller.Joystick, "Wasp", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Waterski, CartType.A78S4, () => GetRomData("Waterski"), Controller.ProLineJoystick, "Water Ski", ManufacturerFroggo),
            new GameProgramInfo(GameProgramId.WingWar, CartType.A8K, () => GetRomData("Wingwar"), Controller.Joystick, "Wing War", ManufacturerImagic),
            new GameProgramInfo(GameProgramId.Worm, CartType.A7816, () => GetRomData("worm0730"), Controller.Joystick, "Worm", ManufacturerHomebrew),
            new GameProgramInfo(GameProgramId.Xenophobe, CartType.A78SG, () => GetRomData("Xenophobe"), Controller.ProLineJoystick, "Xenophobe", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.Xevious78, CartType.A7832, () => GetRomData("Xevious"), Controller.ProLineJoystick, "Xevious", ManufacturerAtari),
            new GameProgramInfo(GameProgramId.YarsRevenge, CartType.A4K, () => GetRomData("Yar_rev"), Controller.Joystick, "Yars Revenge", ManufacturerAtari),
        };

        static readonly IDictionary<GameProgramId, GameProgramInfo> GameProgramDict = new Dictionary<GameProgramId, GameProgramInfo>();

        #endregion

        public IEnumerable<GameProgramInfo> GetAllGamePrograms()
        {
            return GameProgramCollection;
        }

        public GameProgramInfo GetGameProgram(GameProgramId gameProgramId)
        {
            return GameProgramDict[gameProgramId];
        }

        #region Constructors

        static GameProgramInfoRepository()
        {
            foreach (var gpi in GameProgramCollection)
            {
                GameProgramDict[gpi.Id] = gpi;
            }
        }

        #endregion

        #region Helpers

        static byte[] GetRomData(string romname, bool isA78Format = false)
        {
            var path = string.Format("Model/RomResources/{0}.{1}", romname, isA78Format ? "a78" : "bin");
            var uri = new Uri(path, UriKind.Relative);
            var rs = Application.GetResourceStream(uri);
            using (var br = new BinaryReader(rs.Stream))
            {
                if (isA78Format)
                    br.BaseStream.Seek(0x80, SeekOrigin.Begin);
                return br.ReadBytes((int)rs.Stream.Length);
            }
        }

        #endregion
    }
}
