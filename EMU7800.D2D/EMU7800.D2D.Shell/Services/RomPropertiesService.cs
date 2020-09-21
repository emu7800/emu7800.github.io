// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class RomPropertiesService
    {
        #region Fields

        const string PatternMd5KeyType  = @"^([0-9a-f]{32,32})$";
        readonly static Regex _regexMd5KeyType = new Regex(PatternMd5KeyType, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        #endregion

        public IEnumerable<GameProgramInfo> ToGameProgramInfo(IEnumerable<string> csvFileContent)
        {
            if (csvFileContent == null)
                throw new ArgumentNullException("csvFileContent");

            const MachineType nullMachineType = (MachineType)(-1);
            const CartType nullCartType = (CartType)(-1);
            const Controller nullController = (Controller)(-1);

            var csvSplitter = new CsvSplitter();
            foreach (var line in csvFileContent)
            {
                var splitLine = csvSplitter.Split(line);

                string md5 = null;
                var machineType = nullMachineType;
                var cartType = nullCartType;
                var lcontroller = nullController;
                var rcontroller = nullController;

                for (var i = 0; i < splitLine.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitLine[i]))
                    {
                        continue;
                    }
                    if (md5 == null && ToMD5(splitLine[i], ref md5))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    if (machineType == nullMachineType && ToMachineType(splitLine[i], ref machineType))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    if (cartType == nullCartType && ToCartType(splitLine[i], ref cartType))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    if (lcontroller == nullController && ToController(splitLine[i], ref lcontroller))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    if (rcontroller == nullController && ToController(splitLine[i], ref rcontroller))
                    {
                        splitLine[i] = null;
                    }
                }
                if (md5 == null || machineType == nullMachineType)
                    continue;

                RemoveNulls(splitLine);

                var gpi = new GameProgramInfo
                {
                    MD5          = md5,
                    MachineType  = machineType,
                    Title        = RemoveFirstNonNull(splitLine),
                    Manufacturer = RemoveFirstNonNull(splitLine),
                    Author       = RemoveFirstNonNull(splitLine),
                    Qualifier    = RemoveFirstNonNull(splitLine),
                    Year         = RemoveFirstNonNull(splitLine),
                    ModelNo      = RemoveFirstNonNull(splitLine),
                    Rarity       = RemoveFirstNonNull(splitLine),
                };
                if (cartType == nullCartType)
                {
                    cartType = CartType.None;
                    RemoveFirstNonNull(splitLine);
                }
                var defaultController = ToDefaultController(machineType);
                if (lcontroller == nullController)
                {
                    lcontroller = defaultController;
                    RemoveFirstNonNull(splitLine);
                }
                if (rcontroller == nullController)
                {
                    rcontroller = defaultController;
                    RemoveFirstNonNull(splitLine);
                }

                gpi.CartType    = cartType;
                gpi.LController = lcontroller;
                gpi.RController = rcontroller;
                gpi.HelpUri     = RemoveFirstNonNull(splitLine);

                yield return gpi;
            }
        }

        public IEnumerable<ImportedGameProgramInfo> ToImportedGameProgramInfo(IEnumerable<GameProgramInfo> gameProgramInfoSet, IEnumerable<string> csvFileContent)
        {
            if (gameProgramInfoSet == null)
                throw new ArgumentNullException("gameProgramInfoSet");
            if (csvFileContent == null)
                throw new ArgumentNullException("csvFileContent");

            var md5dict = ToMD5Dict(gameProgramInfoSet);
            var importedGameProgramInfoMd5Dict = new Dictionary<string, IList<ImportedGameProgramInfo>>(StringComparer.OrdinalIgnoreCase);

            var csvSplitter = new CsvSplitter();
            foreach (var line in csvFileContent)
            {
                var splitLine = csvSplitter.Split(line);

                string md5key = null;
                string storageKey = null;
                for (var i = 0; i < splitLine.Length; i++)
                {
                    if (ToMD5(splitLine[i], ref md5key))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    storageKey = splitLine[i];
                }
                if (md5key == null || storageKey == null)
                    continue;

                IList<GameProgramInfo> gpiList;
                if (!md5dict.TryGetValue(md5key, out gpiList))
                    continue;

                IList<ImportedGameProgramInfo> igpiList;
                if (!importedGameProgramInfoMd5Dict.TryGetValue(md5key, out igpiList))
                {
                    igpiList = gpiList.Select(gpi => new ImportedGameProgramInfo { GameProgramInfo = gpi }).ToList();
                    importedGameProgramInfoMd5Dict.Add(md5key, igpiList);
                }

                foreach (var igpi in igpiList)
                {
                    igpi.StorageKeySet.Add(storageKey);
                }
            }

            return importedGameProgramInfoMd5Dict.Values.SelectMany(igpi => igpi);
        }

        public IEnumerable<ImportedSpecialBinaryInfo> ToImportedSpecialBinaryInfo(IEnumerable<string> csvFileContent)
        {
            if (csvFileContent == null)
                throw new ArgumentNullException("csvFileContent");

            var csvSplitter = new CsvSplitter();
            foreach (var line in csvFileContent)
            {
                var splitLine = csvSplitter.Split(line);

                var type = SpecialBinaryType.None;
                string storageKey = null;
                for (var i = 0; i < splitLine.Length; i++)
                {
                    if (ToSpecialBinaryType(splitLine[i], ref type))
                    {
                        splitLine[i] = null;
                        continue;
                    }
                    storageKey = splitLine[i];
                }
                if (type == SpecialBinaryType.None || storageKey == null)
                    continue;

                var sbi = new ImportedSpecialBinaryInfo
                {
                    Type       = type,
                    StorageKey = storageKey
                };
                yield return sbi;
            }
        }

        public IEnumerable<string> ToCsvFileContent(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfo)
        {
            if (importedGameProgramInfo == null)
                throw new ArgumentNullException("importedGameProgramInfo");

            return from igpi in importedGameProgramInfo
                   from storageKey in igpi.StorageKeySet
                   select $"{igpi.GameProgramInfo.MD5},\"{storageKey}\"";
        }

        public IEnumerable<string> ToCsvFileContent(IEnumerable<ImportedSpecialBinaryInfo> importedSpecialBinaries)
        {
            if (importedSpecialBinaries == null)
                throw new ArgumentNullException("importedSpecialBinaries");

            return importedSpecialBinaries.Select(isbi => $"{isbi.Type},\"{isbi.StorageKey}\"");
        }

        public IDictionary<string, IList<GameProgramInfo>> ToMD5Dict(IEnumerable<GameProgramInfo> gameProgramInfoSet)
        {
            if (gameProgramInfoSet == null)
                throw new ArgumentNullException("gameProgramInfoSet");

            var dict = new Dictionary<string, IList<GameProgramInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var gpi in gameProgramInfoSet)
            {
                IList<GameProgramInfo> gpiList;
                if (!dict.TryGetValue(gpi.MD5, out gpiList))
                {
                    gpiList = new List<GameProgramInfo>();
                    dict.Add(gpi.MD5, gpiList);
                }
                gpiList.Add(gpi);
            }
            return dict;
        }

        #region Helpers

        class CsvSplitter
        {
            readonly StringBuilder _sb = new StringBuilder();
            readonly IList<string> _list = new List<string>();

            public string[] Split(string value)
            {
                _list.Clear();
                _sb.Clear();
                for (var i = 0; i < value.Length; i++)
                {
                    var ch1 = value[i];
                    if (ch1 == '"')
                    {
                        var ch2 = (i < value.Length - 1) ? value[i + 1] : 0;
                        if (ch2 == '"')
                        {
                            _sb.Append(ch1);
                            i++;
                            continue;
                        }
                        for (i++; i < value.Length; i++)
                        {
                            ch1 = value[i];
                            if (ch1 == '"')
                            {
                                break;
                            }
                            _sb.Append(ch1);
                        }
                    }
                    else if (ch1 == ',')
                    {
                        _list.Add(_sb.ToString().Trim());
                        _sb.Clear();
                    }
                    else
                    {
                        _sb.Append(ch1);
                    }
                }
                if (_sb.Length > 0)
                {
                    _list.Add(_sb.ToString().Trim());
                }
                return _list.ToArray();
            }
        }

        static string RemoveFirstNonNull(string[] ar)
        {
            var index = 0;
            string s;
            while (true)
            {
                s = ar[index];
                if (s != null || index + 1 == ar.Length)
                    break;
                index++;
            }
            if (s == null)
                return string.Empty;
            ar[index] = null;
            RemoveNulls(ar);
            return s;
        }

        static void RemoveNulls(string[] ar)
        {
            var srcIndex = 0;
            var tgtIndex = 0;
            while (srcIndex < ar.Length)
            {
                var src = ar[srcIndex++];
                if (src != null)
                    ar[tgtIndex++] = src;
            }
            while (tgtIndex < ar.Length)
            {
                ar[tgtIndex++] = null;
            }
        }

        static bool ToCartType(string token, ref CartType result)
        {
            if      (IsEquals(token, "A2K"))     result = CartType.A2K;
            else if (IsEquals(token, "TV8K"))    result = CartType.TV8K;
            else if (IsEquals(token, "A4K"))     result = CartType.A4K;
            else if (IsEquals(token, "PB8K"))    result = CartType.PB8K;
            else if (IsEquals(token, "MN16K"))   result = CartType.MN16K;
            else if (IsEquals(token, "A16K"))    result = CartType.A16K;
            else if (IsEquals(token, "A16KR"))   result = CartType.A16KR;
            else if (IsEquals(token, "A8K"))     result = CartType.A8K;
            else if (IsEquals(token, "A8KR"))    result = CartType.A8KR;
            else if (IsEquals(token, "A32K"))    result = CartType.A32K;
            else if (IsEquals(token, "A32KR"))   result = CartType.A32KR;
            else if (IsEquals(token, "CBS12K"))  result = CartType.CBS12K;
            else if (IsEquals(token, "DC8K"))    result = CartType.DC8K;
            else if (IsEquals(token, "DPC"))     result = CartType.DPC;
            else if (IsEquals(token, "M32N12K")) result = CartType.M32N12K;
            else if (IsEquals(token, "A7808"))   result = CartType.A7808;
            else if (IsEquals(token, "A7816"))   result = CartType.A7816;
            else if (IsEquals(token, "A7832"))   result = CartType.A7832;
            else if (IsEquals(token, "A7832P"))  result = CartType.A7832P;
            else if (IsEquals(token, "A7848"))   result = CartType.A7848;
            else if (IsEquals(token, "A78SG"))   result = CartType.A78SG;
            else if (IsEquals(token, "A78SGP"))  result = CartType.A78SGP;
            else if (IsEquals(token, "A78SGR"))  result = CartType.A78SGR;
            else if (IsEquals(token, "A78S9"))   result = CartType.A78S9;
            else if (IsEquals(token, "A78S4"))   result = CartType.A78S4;
            else if (IsEquals(token, "A78S4R"))  result = CartType.A78S4R;
            else if (IsEquals(token, "A78AB"))   result = CartType.A78AB;
            else if (IsEquals(token, "A78AC"))   result = CartType.A78AC;
            else return false;
            return true;
        }

        static bool ToMachineType(string token, ref MachineType result)
        {
            if      (IsEquals(token, "A2600NTSC")) result = MachineType.A2600NTSC;
            else if (IsEquals(token, "A2600PAL"))  result = MachineType.A2600PAL;
            else if (IsEquals(token, "A7800NTSC")) result = MachineType.A7800NTSC;
            else if (IsEquals(token, "A7800PAL"))  result = MachineType.A7800PAL;
            else return false;
            return true;
        }

        static bool ToController(string token, ref Controller result)
        {
            if      (IsEquals(token, "Joystick"))        result = Controller.Joystick;
            else if (IsEquals(token, "Paddles"))         result = Controller.Paddles;
            else if (IsEquals(token, "Keypad"))          result = Controller.Keypad;
            else if (IsEquals(token, "Driving"))         result = Controller.Driving;
            else if (IsEquals(token, "BoosterGrip"))     result = Controller.BoosterGrip;
            else if (IsEquals(token, "ProLineJoystick")) result = Controller.ProLineJoystick;
            else if (IsEquals(token, "Lightgun"))        result = Controller.Lightgun;
            else if (IsEquals(token, "Mindlink"))        result = Controller.None;
            else return false;
            return true;
        }

        static bool ToMD5(string token, ref string result)
        {
            if (!string.IsNullOrWhiteSpace(token) && _regexMd5KeyType.IsMatch(token))
            {
                result = token;
                return true;
            }
            return false;
        }

        static bool ToSpecialBinaryType(string token, ref SpecialBinaryType result)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;
            return Enum.IsDefined(typeof(SpecialBinaryType), token) && Enum.TryParse(token, true, out result);
        }

        static bool IsEquals(string s1, string s2)
        {
            return (s1 != null) && s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
        }

        static Controller ToDefaultController(MachineType machineType)
        {
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    return Controller.Joystick;
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    return Controller.ProLineJoystick;
                default:
                    return Controller.None;
            }
        }

        #endregion
    }
}
