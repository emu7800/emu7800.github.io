// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services
{
    public class GameProgramLibraryService
    {
        public static IEnumerable<GameProgramInfoViewItemCollection> GetGameProgramInfoViewItemCollections(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfoSet)
            => ToGameProgramInfoViewItemCollections(
                    ToDict(importedGameProgramInfoSet, igpi => To2600or7800Word(igpi)),
                    igpi => igpi.GameProgramInfo.Title,
                    igpi => ToMachineTypeSubTitle(igpi))
               .Concat(
               ToGameProgramInfoViewItemCollections(
                   ToDict(importedGameProgramInfoSet, igpi => igpi.GameProgramInfo.Manufacturer),
                   igpi => igpi.GameProgramInfo.Title,
                   igpi => ToManufacturerSubTitle(igpi))
               ).Concat(
               ToGameProgramInfoViewItemCollections(
                   ToDict(importedGameProgramInfoSet, igpi => igpi.GameProgramInfo.Author),
                   igpi => igpi.GameProgramInfo.Year,
                   igpi => ToAuthorSubTitle(igpi))
               ).ToList();

        #region Helpers

        static IEnumerable<GameProgramInfoViewItemCollection> ToGameProgramInfoViewItemCollections(
                Dictionary<string, List<ImportedGameProgramInfo>> dict,
                Func<ImportedGameProgramInfo, string> orderByFunc,
                Func<ImportedGameProgramInfo, string> subTitleFunc)
            => dict.OrderBy(kvp => kvp.Key)
                   .Select(kvp => new GameProgramInfoViewItemCollection
                   {
                       Name = kvp.Key,
                       GameProgramInfoViewItemSet = kvp.Value
                           .OrderBy(igpi => orderByFunc(igpi))
                           .Select(igpi => ToGameProgramInfoViewItem(igpi, igpi => subTitleFunc(igpi)))
                           .ToList(),
                   });

        static GameProgramInfoViewItem ToGameProgramInfoViewItem(ImportedGameProgramInfo igpi, Func<ImportedGameProgramInfo, string> subTitleFunc)
            => new()
            {
                Title = igpi.GameProgramInfo.Title,
                SubTitle = subTitleFunc(igpi),
                ImportedGameProgramInfo = igpi
            };

        static Dictionary<string, List<ImportedGameProgramInfo>> ToDict(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfoSet, Func<ImportedGameProgramInfo, string> keySelector)
            => importedGameProgramInfoSet.GroupBy(keySelector)
                                         .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                                         .ToDictionary(g => g.Key, g => g.ToList());

        static string ToAuthorSubTitle(ImportedGameProgramInfo igpi)
            => ToCommaDelimitedString(ToAuthorSubTitleWords(igpi));

        static string ToManufacturerSubTitle(ImportedGameProgramInfo igpi)
            => ToCommaDelimitedString(ToManufacturerSubTitleWords(igpi));

        static string ToMachineTypeSubTitle(ImportedGameProgramInfo igpi)
            => ToCommaDelimitedString(ToMachineTypeSubTitleWords(igpi));

        static IEnumerable<string> ToAuthorSubTitleWords(ImportedGameProgramInfo igpi)
            => new List<string>
            {
                igpi.GameProgramInfo.Manufacturer,
                ToControllerWord(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
                AreControllersSame(igpi) ? string.Empty : ToControllerWord(igpi.GameProgramInfo.RController),
                ToMachineTypeWord(igpi.GameProgramInfo.MachineType),
                igpi.GameProgramInfo.Year
            };

        static IEnumerable<string> ToManufacturerSubTitleWords(ImportedGameProgramInfo igpi)
            => new List<string>
            {
                igpi.GameProgramInfo.Author,
                ToControllerWord(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
                AreControllersSame(igpi) ? string.Empty : ToControllerWord(igpi.GameProgramInfo.RController),
                ToMachineTypeWord(igpi.GameProgramInfo.MachineType),
                igpi.GameProgramInfo.Year
            };

        static IEnumerable<string> ToMachineTypeSubTitleWords(ImportedGameProgramInfo igpi)
            => new List<string>
            {
                igpi.GameProgramInfo.Manufacturer,
                ToControllerWord(igpi.GameProgramInfo.LController, AreControllersSame(igpi)),
                AreControllersSame(igpi) ? string.Empty : ToControllerWord(igpi.GameProgramInfo.RController),
                ToTvTypeWord(igpi.GameProgramInfo.MachineType),
                igpi.GameProgramInfo.Year
            };

        static string ToCommaDelimitedString(IEnumerable<string> list)
            => string.Join(", ", list.Where(s => !string.IsNullOrWhiteSpace(s)));

        static bool AreControllersSame(ImportedGameProgramInfo igpi)
            => igpi.GameProgramInfo.LController == igpi.GameProgramInfo.RController;

        static string ToControllerWord(Controller controller, bool plural)
            => ToControllerWord(controller) + (plural ? "s" : string.Empty);

        static string ToControllerWord(Controller controller)
            => controller switch
            {
                Controller.ProLineJoystick => "Proline Joystick",
                Controller.Joystick        => "Joystick",
                Controller.Paddles         => "Paddle",
                Controller.Keypad          => "Keypad",
                Controller.Driving         => "Driving Paddle",
                Controller.BoosterGrip     => "Booster Grip",
                Controller.Lightgun        => "Lightgun",
                _                          => string.Empty,
            };

        static string To2600or7800Word(ImportedGameProgramInfo igpi)
            => igpi.GameProgramInfo.MachineType switch
            {
                MachineType.A2600NTSC or MachineType.A2600PAL => "2600",
                MachineType.A7800NTSC or MachineType.A7800PAL => "7800",
                _                                             => string.Empty,
            };

        static string ToMachineTypeWord(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC => "2600 NTSC",
                MachineType.A2600PAL  => "2600 PAL",
                MachineType.A7800NTSC => "7800 NTSC",
                MachineType.A7800PAL  => "7800 PAL",
                _                     => string.Empty,
            };

        static string ToTvTypeWord(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC or MachineType.A7800NTSC => "NTSC",
                MachineType.A2600PAL  or MachineType.A7800PAL  => "PAL",
                _                                              => string.Empty,
            };

        #endregion
    }
}
