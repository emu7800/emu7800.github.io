using EMU7800.Core;
using System;
using Windows.UI.Core;

namespace EMU7800.WP.View
{
    public abstract class InputHandler
    {
        #region Fields

        static readonly string[] CurrentPlayerNumberText = { "1", "2", "3", "4" };
        readonly MachineBase _machine;
        int _currentPlayerNo;
        int _machineInputButtonUpCounter;
        MachineInput _machineInputForButtonUpCounter;

        #endregion

        public int CurrentPlayerNo
        {
            get { return _currentPlayerNo; }
            set { _currentPlayerNo = value & 3; }
        }

        public string CurrentPlayerNoText
        {
            get { return CurrentPlayerNumberText[_currentPlayerNo]; }
        }

        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public virtual void OnPointerPressed(PointerEventArgs args)
        {
        }

        public virtual void OnPointerMoved(PointerEventArgs args)
        {
        }

        public virtual void OnPointerReleased(PointerEventArgs args)
        {
        }

        public virtual void Update()
        {
            if (_machineInputButtonUpCounter > 0 && --_machineInputButtonUpCounter == 0)
                RaiseMachineInput(_machineInputForButtonUpCounter, false);
        }

        public void RaiseMachineInputWithButtonUpCounter(MachineInput machineInput)
        {
            if (_machineInputButtonUpCounter > 0)
                return;
            _machineInputButtonUpCounter = 5;
            _machineInputForButtonUpCounter = machineInput;
            RaiseMachineInput(machineInput, true);
        }

        public void RaiseMachineInput(MachineInput machineInput, bool down)
        {
            _machine.InputState.RaiseInput(_currentPlayerNo, machineInput, down);
        }

        public void RaiseOppositePlayerMachineInput(MachineInput machineInput, bool down)
        {
            _machine.InputState.RaiseInput(_currentPlayerNo ^ 1, machineInput, down);
        }

        public void RaiseMachinePaddleInput(int valMax, int val)
        {
            _machine.InputState.RaisePaddleInput(_currentPlayerNo, valMax, val);
        }

        public void RaiseMachineLightgunInput(int scanline, int hpos, bool down)
        {
            _machine.InputState.RaiseLightgunPos(_currentPlayerNo, scanline, hpos);
            _machine.InputState.RaiseInput(_currentPlayerNo, MachineInput.Fire, down);
        }

        #region Constructors

        protected InputHandler(MachineBase machine)
        {
            if (machine == null)
                throw new ArgumentNullException("machine");
            _machine = machine;
        }

        #endregion
    }
}
