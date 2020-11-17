using System;

namespace EMU7800.SoundEmulator
{
    public class InputTapePlayer
    {
        #region Fields

        readonly InputTapeReader _inputTapeReader;
        byte[] _currentRegisters;
        bool _endOfTapeReached;

        #endregion

        public Action EndOfTapeReached;

        public void GetRegisterSettingsForNextFrame(SoundEmulator e)
        {
            if (_endOfTapeReached)
                return;
            var reg = _currentRegisters ?? _inputTapeReader.Dequeue();
            if (reg == null)
            {
                _endOfTapeReached = true;
                if (EndOfTapeReached != null)
                    EndOfTapeReached();
                return;
            }
            _currentRegisters = reg;

            // line format
            // tAUDC0 tAUDF0 tAUDV0  tAUDC1 tAUDF1 tAUDV1  pAUDCTL pAUDC1 pAUDF1  pAUDC2 pAUDF2 pAUDC3 pAUDF3 pAUDC4 pAUDF4  repeatCount

            e.PokeTia(Constants.TIA_AUDC0, reg[0]);
            e.PokeTia(Constants.TIA_AUDF0, reg[1]);
            e.PokeTia(Constants.TIA_AUDV0, reg[2]);
            e.PokeTia(Constants.TIA_AUDC1, reg[3]);
            e.PokeTia(Constants.TIA_AUDF1, reg[4]);
            e.PokeTia(Constants.TIA_AUDV1, reg[5]);

            e.PokePokey(Constants.POKEY_AUDCTL, reg[6]);
            e.PokePokey(Constants.POKEY_AUDC1, reg[7]);
            e.PokePokey(Constants.POKEY_AUDF1, reg[8]);
            e.PokePokey(Constants.POKEY_AUDC2, reg[9]);
            e.PokePokey(Constants.POKEY_AUDF2, reg[10]);
            e.PokePokey(Constants.POKEY_AUDC3, reg[11]);
            e.PokePokey(Constants.POKEY_AUDF3, reg[12]);
            e.PokePokey(Constants.POKEY_AUDC4, reg[13]);
            e.PokePokey(Constants.POKEY_AUDF4, reg[14]);

            if (--reg[15] <= 0)
                _currentRegisters = null;
        }

        #region Constructors

        public InputTapePlayer(InputTapeReader inputTapeReader)
        {
            if (inputTapeReader == null)
                throw new ArgumentNullException("inputTapeReader");
            _inputTapeReader = inputTapeReader;
        }

        #endregion
    }
}
