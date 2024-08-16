// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EMU7800.Services;

public static partial class RomPropertiesService
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

    static readonly Regex _regexMd5KeyType = CompiledMd5RegEx();

    [GeneratedRegex("^([0-9a-f]{32,32})$", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
    private static partial Regex CompiledMd5RegEx();

    #endregion

    public static IEnumerable<GameProgramInfo> ToGameProgramInfo(IEnumerable<string> romPropertiesCsv)
        => VerifyReferenceRepositoryCsvHeader(romPropertiesCsv)
            .Select(Split)
            .Select(sl => new GameProgramInfo
            {
                MD5          = sl[CsvColumnMD5],
                MachineType  = MachineTypeUtil.From(sl[CsvColumnMachineType]),
                CartType     = CartTypeUtil.From(sl[CsvColumnCartType]),
                LController  = ControllerUtil.From(sl[CsvColumnLController], sl[CsvColumnMachineType]),
                RController  = ControllerUtil.From(sl[CsvColumnRController], sl[CsvColumnMachineType]),
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
    {
        var seenFirstLine = false;
        foreach (var csvLine in romPropertiesCsv)
        {
            if (!seenFirstLine)
            {
                if (csvLine != ReferenceRepositoryCsvHeader)
                {
                    yield break;
                }
                seenFirstLine = true;
                continue;
            }
            yield return csvLine;
        }
    }

    static string[] Split(string line)
    {
        var output = new string[13];
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
            => s.Substring(pos, len).Trim(['\\', '"', ' ']);
    }

    static bool IsMD5(string s)
        => _regexMd5KeyType.IsMatch(s);

    #endregion
}