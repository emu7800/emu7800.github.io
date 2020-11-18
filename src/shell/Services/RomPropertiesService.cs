// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMU7800.Services
{
    public static class RomPropertiesService
    {
        #region Fields

        const int
            CsvColumnTitle        = 0,
            CsvColumnManufacturer = 1,
            CsvColumnAuthor       = 2,
            CsvColumnQualifier    = 3,
            CsvColumnYear         = 4,
            CsvColumnModelNo      = 5,
            CsvColumnRarity       = 6,
            CsvColumnCartType     = 7,
            CsvColumnMachineType  = 8,
            CsvColumnLController  = 9,
            CsvColumnRController  = 10,
            CsvColumnMD5          = 11,
            CsvColumnHelpUri      = 12;

        const string ReferenceRepositoryCsvHeader
            = "Title,Manufacturer,Author,Qualifier,Year,ModelNo,Rarity,CartType,MachineType,LController,RController,MD5,HelpUri";

        readonly static Regex _regexMd5KeyType = new(@"^([0-9a-f]{32,32})$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        #endregion

        public static IEnumerable<GameProgramInfo> ToGameProgramInfo(IEnumerable<string> romPropertiesCsv)
            => VerifyReferenceRepositoryCsvHeader(romPropertiesCsv)
                .Select(csv => Split(csv, 13))
                .Select(sl => new GameProgramInfo
                {
                    MD5          = sl[CsvColumnMD5],
                    MachineType  = ToMachineType(sl[CsvColumnMachineType]),
                    CartType     = ToCartType(sl[CsvColumnCartType]),
                    LController  = ToController(sl[CsvColumnLController], sl[CsvColumnMachineType]),
                    RController  = ToController(sl[CsvColumnRController], sl[CsvColumnMachineType]),
                    HelpUri      = sl[CsvColumnHelpUri],
                    Title        = sl[CsvColumnTitle],
                    Manufacturer = sl[CsvColumnManufacturer],
                    Author       = sl[CsvColumnAuthor],
                    Qualifier    = sl[CsvColumnQualifier],
                    Year         = sl[CsvColumnYear],
                    ModelNo      = sl[CsvColumnModelNo],
                    Rarity       = sl[CsvColumnRarity]
                })
                .Where(gpi => IsMD5(gpi.MD5))
                .ToList()
                ;

        #region Helpers

        static IEnumerable<string> VerifyReferenceRepositoryCsvHeader(IEnumerable<string> romPropertiesCsv)
            => romPropertiesCsv.Take(1).First() == ReferenceRepositoryCsvHeader
                ? romPropertiesCsv
                : Enumerable.Empty<string>();

        static string[] Split(string line, int columnLimit = 13)
        {
            var output = new string[columnLimit];
            var pos = 0;
            var len = 0;
            var i = 0;
            var quoteOff = true;
            while (i < output.Length)
            {
                if (pos + len >= line.Length)
                {
                    output[i++] = ExtractField(line, pos, len);
                    pos = line.Length;
                    len = 0;
                }
                else if (quoteOff && line[pos + len] == ',')
                {
                    output[i++] = ExtractField(line, pos, len);
                    pos += len + 1;
                    len = 0;
                }
                else if (line[pos + len] == '"'
                      || line[pos + len] == '\\' && pos + ++len < line.Length && line[pos + len] == '"')
                {
                    quoteOff ^= true;
                    len++;
                }
                else
                {
                    len++;
                }
            }
            return output;

            static string ExtractField(string s, int pos, int len)
                => s.Substring(pos, len).Trim(new[] { '\\', '"', ' ' });
        }

        static CartType ToCartType(string s)
            => IsEquals(s, "A2K")     ? CartType.A2K
             : IsEquals(s, "TV8K")    ? CartType.TV8K
             : IsEquals(s, "A4K")     ? CartType.A4K
             : IsEquals(s, "PB8K")    ? CartType.PB8K
             : IsEquals(s, "MN16K")   ? CartType.MN16K
             : IsEquals(s, "A16K")    ? CartType.A16K
             : IsEquals(s, "A16KR")   ? CartType.A16KR
             : IsEquals(s, "A8K")     ? CartType.A8K
             : IsEquals(s, "A8KR")    ? CartType.A8KR
             : IsEquals(s, "A32K")    ? CartType.A32K
             : IsEquals(s, "A32KR")   ? CartType.A32KR
             : IsEquals(s, "CBS12K")  ? CartType.CBS12K
             : IsEquals(s, "DC8K")    ? CartType.DC8K
             : IsEquals(s, "DPC")     ? CartType.DPC
             : IsEquals(s, "M32N12K") ? CartType.M32N12K
             : IsEquals(s, "A7808")   ? CartType.A7808
             : IsEquals(s, "A7816")   ? CartType.A7816
             : IsEquals(s, "A7832")   ? CartType.A7832
             : IsEquals(s, "A7832P")  ? CartType.A7832P
             : IsEquals(s, "A7848")   ? CartType.A7848
             : IsEquals(s, "A78SG")   ? CartType.A78SG
             : IsEquals(s, "A78SGP")  ? CartType.A78SGP
             : IsEquals(s, "A78SGR")  ? CartType.A78SGR
             : IsEquals(s, "A78S9")   ? CartType.A78S9
             : IsEquals(s, "A78S4")   ? CartType.A78S4
             : IsEquals(s, "A78S4R")  ? CartType.A78S4R
             : IsEquals(s, "A78AB")   ? CartType.A78AB
             : IsEquals(s, "A78AC")   ? CartType.A78AC
             : CartType.Unknown;

        static MachineType ToMachineType(string s)
            => IsEquals(s, "A2600NTSC") ? MachineType.A2600NTSC
             : IsEquals(s, "A2600PAL")  ? MachineType.A2600PAL
             : IsEquals(s, "A7800NTSC") ? MachineType.A7800NTSC
             : IsEquals(s, "A7800PAL")  ? MachineType.A7800PAL
             : MachineType.Unknown;

        static Controller ToController(string s, string mt)
            => IsEquals(s, "Joystick")        ? Controller.Joystick
             : IsEquals(s, "Paddles")         ? Controller.Paddles
             : IsEquals(s, "Keypad")          ? Controller.Keypad
             : IsEquals(s, "Driving")         ? Controller.Driving
             : IsEquals(s, "BoosterGrip")     ? Controller.BoosterGrip
             : IsEquals(s, "ProLineJoystick") ? Controller.ProLineJoystick
             : IsEquals(s, "Lightgun")        ? Controller.Lightgun
             : IsEquals(s, "Mindlink")        ? Controller.None
             : ToMachineType(mt) switch
             {
                 MachineType.A2600NTSC or MachineType.A2600PAL => Controller.Joystick,
                 MachineType.A7800NTSC or MachineType.A7800PAL => Controller.ProLineJoystick,
                 _                                             => Controller.None,
             };

        static bool IsMD5(string s)
            => !string.IsNullOrWhiteSpace(s) && _regexMd5KeyType.IsMatch(s);

        static bool IsEquals(string s1, string s2)
            => (s1 != null) && s1.Equals(s2, StringComparison.OrdinalIgnoreCase);

        #endregion
    }
}
