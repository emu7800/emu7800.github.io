// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EMU7800.Core;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class GameProgramLibraryService
    {
        #region Fields

        readonly StringBuilder _sb = new StringBuilder();

        #endregion

        public IEnumerable<GameProgramInfoViewItemCollection> GetGameProgramInfoViewItemCollections(IEnumerable<ImportedGameProgramInfo> importedGameProgramInfoSet)
        {
            if (importedGameProgramInfoSet == null)
                throw new ArgumentNullException("importedGameProgramInfoSet");

            var mtcDict = new Dictionary<string, IList<ImportedGameProgramInfo>>();
            var mfcDict = new Dictionary<string, IList<ImportedGameProgramInfo>>();
            var atcDict = new Dictionary<string, IList<ImportedGameProgramInfo>>();
            foreach (var gpi in importedGameProgramInfoSet)
            {
                AddToMachineTypeDict(mtcDict, gpi);
                AddToManufacturerDict(mfcDict, gpi);
                AddToAuthorDict(atcDict, gpi);
            }

            var mtc = ToMachineTypeCollections(mtcDict);
            var mfc = ToManufacturerCollections(mfcDict);
            var atc = ToAuthorCollections(atcDict);

            var gpivics = mtc.Concat(mfc).Concat(atc);
            return gpivics;
        }

        #region Helpers

        #region MachineType Collections

        IEnumerable<GameProgramInfoViewItemCollection> ToMachineTypeCollections(IDictionary<string, IList<ImportedGameProgramInfo>> dict)
        {
            var gpiviList = dict.Keys
                .OrderBy(key => key)
                    .Select(key => new GameProgramInfoViewItemCollection
                    {
                        Name                       = key,
                        GameProgramInfoViewItemSet = dict[key]
                            .OrderBy(igpi => igpi.GameProgramInfo.Title)
                                .Select(igpi => new GameProgramInfoViewItem
                                {
                                    Title                   = igpi.GameProgramInfo.Title,
                                    SubTitle                = ToMachineTypeSubTitle(igpi),
                                    ImportedGameProgramInfo = igpi
                                }).ToArray()
                    });
            return gpiviList;
        }

        static void AddToMachineTypeDict(IDictionary<string, IList<ImportedGameProgramInfo>> dict, ImportedGameProgramInfo igpi)
        {
            var machineTypeKey = ToMachineTypeKey(igpi);
            IList<ImportedGameProgramInfo> gpiList;
            if (!dict.TryGetValue(machineTypeKey, out gpiList))
            {
                gpiList = new List<ImportedGameProgramInfo>();
                dict.Add(machineTypeKey, gpiList);
            }
            dict[machineTypeKey].Add(igpi);
        }

        static string ToMachineTypeKey(ImportedGameProgramInfo igpi)
        {
            switch (igpi.GameProgramInfo.MachineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    return "2600";
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    return "7800";
                default:
                    return null;
            }
        }

        string ToMachineTypeSubTitle(ImportedGameProgramInfo igpi)
        {
            _sb.Clear();
            AppendWord(igpi.GameProgramInfo.Manufacturer);
            AddControllerInfo(igpi);
            AppendTvTypeWord(igpi.GameProgramInfo.MachineType);
            AppendWord(igpi.GameProgramInfo.Year);
            return _sb.ToString();
        }

        #endregion

        #region Author Collections

        IEnumerable<GameProgramInfoViewItemCollection> ToAuthorCollections(IDictionary<string, IList<ImportedGameProgramInfo>> dict)
        {
            var gpiviList = dict.Keys
                .OrderBy(key => key)
                    .Select(key => new GameProgramInfoViewItemCollection
                    {
                        Name                       = key,
                        GameProgramInfoViewItemSet = dict[key]
                            .OrderBy(igpi => igpi.GameProgramInfo.Year)
                                .Select(igpi => new GameProgramInfoViewItem
                                {
                                    Title                   = igpi.GameProgramInfo.Title,
                                    SubTitle                = ToAuthorSubTitle(igpi),
                                    ImportedGameProgramInfo = igpi
                                }).ToArray()
                    });
            return gpiviList;
        }

        static void AddToAuthorDict(IDictionary<string, IList<ImportedGameProgramInfo>> dict, ImportedGameProgramInfo igpi)
        {
            if (string.IsNullOrWhiteSpace(igpi.GameProgramInfo.Author))
                return;

            IList<ImportedGameProgramInfo> gpiList;
            if (!dict.TryGetValue(igpi.GameProgramInfo.Author, out gpiList))
            {
                gpiList = new List<ImportedGameProgramInfo>();
                dict.Add(igpi.GameProgramInfo.Author, gpiList);
            }
            dict[igpi.GameProgramInfo.Author].Add(igpi);
        }

        string ToAuthorSubTitle(ImportedGameProgramInfo igpi)
        {
            _sb.Clear();
            AppendWord(igpi.GameProgramInfo.Manufacturer);
            AddControllerInfo(igpi);
            AppendMachineTypeWord(igpi.GameProgramInfo.MachineType);
            AppendWord(igpi.GameProgramInfo.Year);
            return _sb.ToString();
        }

        #endregion

        #region Manufacturer Collections

        IEnumerable<GameProgramInfoViewItemCollection> ToManufacturerCollections(IDictionary<string, IList<ImportedGameProgramInfo>> dict)
        {
            var gpiviList = dict.Keys
                .OrderBy(key => key)
                    .Select(key => new GameProgramInfoViewItemCollection
                    {
                        Name                       = key,
                        GameProgramInfoViewItemSet = dict[key]
                            .OrderBy(igpi => igpi.GameProgramInfo.Title)
                                .Select(igpi => new GameProgramInfoViewItem
                                {
                                    Title                   = igpi.GameProgramInfo.Title,
                                    SubTitle                = ToManufacturerSubTitle(igpi),
                                    ImportedGameProgramInfo = igpi
                                }).ToArray()
                    });
            return gpiviList;
        }

        static void AddToManufacturerDict(IDictionary<string, IList<ImportedGameProgramInfo>> dict, ImportedGameProgramInfo igpi)
        {
            if (string.IsNullOrWhiteSpace(igpi.GameProgramInfo.Manufacturer))
                return;
            IList<ImportedGameProgramInfo> gpiList;
            if (!dict.TryGetValue(igpi.GameProgramInfo.Manufacturer, out gpiList))
            {
                gpiList = new List<ImportedGameProgramInfo>();
                dict.Add(igpi.GameProgramInfo.Manufacturer, gpiList);
            }
            dict[igpi.GameProgramInfo.Manufacturer].Add(igpi);
        }

        string ToManufacturerSubTitle(ImportedGameProgramInfo igpi)
        {
            _sb.Clear();
            if (!string.IsNullOrWhiteSpace(igpi.GameProgramInfo.Author))
                AppendWord(igpi.GameProgramInfo.Author);
            AddControllerInfo(igpi);
            AppendMachineTypeWord(igpi.GameProgramInfo.MachineType);
            if (!string.IsNullOrWhiteSpace(igpi.GameProgramInfo.Year))
                AppendWord(igpi.GameProgramInfo.Year);
            return _sb.ToString();
        }

        #endregion

        void AddControllerInfo(ImportedGameProgramInfo igpi)
        {
            if (igpi.GameProgramInfo.LController == igpi.GameProgramInfo.RController)
            {
                AddControllerInfo(igpi.GameProgramInfo.LController, true);
            }
            else
            {
                AddControllerInfo(igpi.GameProgramInfo.LController, false);
                AddControllerInfo(igpi.GameProgramInfo.RController, false);
            }
        }

        void AddControllerInfo(Controller controller, bool plural)
        {
            switch (controller)
            {
                case Controller.ProLineJoystick:
                    AppendControllerWord("Proline Joystick", plural);
                    break;
                case Controller.Joystick:
                    AppendControllerWord("Joystick", plural);
                    break;
                case Controller.Paddles:
                    AppendControllerWord("Paddle", plural);
                    break;
                case Controller.Keypad:
                    AppendControllerWord("Keypad", plural);
                    break;
                case Controller.Driving:
                    AppendControllerWord("Driving Paddle", plural);
                    break;
                case Controller.BoosterGrip:
                    AppendControllerWord("Booster Grip", plural);
                    break;
                case Controller.Lightgun:
                    AppendControllerWord("Lightgun", plural);
                    break;
            }
        }

        void AppendControllerWord(string value, bool plural)
        {
            AppendWord(value);
            if (plural)
                _sb.Append("s");
        }

        void AppendMachineTypeWord(MachineType machineType)
        {
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                    AppendWord("2600 NTSC");
                    break;
                case MachineType.A2600PAL:
                    AppendWord("2600 PAL");
                    break;
                case MachineType.A7800NTSC:
                    AppendWord("7800 NTSC");
                    break;
                case MachineType.A7800PAL:
                    AppendWord("7800 PAL");
                    break;
            }
        }

        void AppendTvTypeWord(MachineType machineType)
        {
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A7800NTSC:
                    AppendWord("NTSC");
                    break;
                case MachineType.A2600PAL:
                case MachineType.A7800PAL:
                    AppendWord("PAL");
                    break;
            }
        }

        void AppendWord(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (_sb.Length > 0)
                _sb.Append(", ");
            _sb.Append(value);
        }

        #endregion
    }
}
