using System;
using EMU7800.Core;

namespace EMU7800.Win
{
    public class MachineFactory
    {
        #region Fields

        readonly GameProgramLibrary _gameProgramLibrary;
        readonly HSC7800 _hsc;
        readonly ILogger _logger;

        #endregion

        #region Constructors

        public MachineFactory(GameProgramLibrary gameProgramLibrary, HSC7800 hsc, ILogger logger)
        {
            if (gameProgramLibrary == null)
                throw new ArgumentNullException("gameProgramLibrary");

            _gameProgramLibrary = gameProgramLibrary;
            _hsc = hsc;
            _logger = logger;
        }

        #endregion

        #region Public Members

        public MachineBase BuildMachine(string romFullName, bool use7800Bios)
        {
            if (string.IsNullOrWhiteSpace(romFullName))
                throw new ArgumentNullException("romFullName");

            var gp = _gameProgramLibrary.GetGameProgramFromFullName(romFullName);
            if (gp == null)
                return null;

            var biosBytes = use7800Bios ? _gameProgramLibrary.Get78BiosBytes(gp.MachineType) : null;
            var bios = (biosBytes != null) ? new Bios7800(biosBytes) : null;

            if (use7800Bios && bios == null)
                _logger.WriteLine("7800 BIOS requested but not found.");

            var romBytes = _gameProgramLibrary.GetRomBytes(romFullName);
            var cart = Cart.Create(romBytes, gp.CartType);
            return MachineBase.Create(gp.MachineType, cart, bios, _hsc, gp.LController, gp.RController, _logger);
        }

        public MachineBase BuildMachine(string romFullName)
        {
            return BuildMachine(romFullName, false);
        }

        #endregion
    }
}
