using System;
using System.IO;
using EMU7800.Core;

namespace EMU7800.Win
{
    public class HSC7800Factory
    {
        #region Fields

        const string
            RamFileName = "emu7800 scores.hsc" ;

        byte[] _hscRamBytes;

        readonly GameProgramLibrary _gameProgramLibrary;
        readonly GlobalSettings _globalSettings;
        readonly ILogger _logger;

        #endregion

        #region Constructors

        public HSC7800Factory(GameProgramLibrary gpl, ILogger logger)
        {
            if (gpl == null)
                throw new ArgumentNullException("gpl");
            if (logger == null)
                throw new ArgumentNullException("logger");
            _gameProgramLibrary = gpl;
            _logger = logger;
            _globalSettings = new GlobalSettings(logger);
        }

        #endregion

        public HSC7800 CreateHSC7800()
        {
            var romBytes = _gameProgramLibrary.Get78HighScoreCartBytes();
            if (romBytes == null)
                return null;
            var fullName = Path.Combine(_globalSettings.OutputDirectory, RamFileName);
            var bytes = _gameProgramLibrary.GetRomBytes(fullName);
            if (bytes.Length != 0x800)
                throw new ArgumentException("Specified RAM bytes not 0x800 in size!");
            _hscRamBytes = bytes;
            _logger.WriteLine("Loaded high score cart data: " + fullName);
            return new HSC7800(romBytes, _hscRamBytes);
        }

        public void SaveRam()
        {
            if (_hscRamBytes == null)
            {
                _logger.WriteLine("HSC7800Factory: SaveRam: Unable to save high score cart data because it was never loaded first.");
                return;
            }
            var fullName = Path.Combine(_globalSettings.OutputDirectory, RamFileName);
            File.WriteAllBytes(fullName, _hscRamBytes);
            _logger.WriteLine("HSC7800Factory: Saved high score cart data: " + fullName);
        }
    }
}
