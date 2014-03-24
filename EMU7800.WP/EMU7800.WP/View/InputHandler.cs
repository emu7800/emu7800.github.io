using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using EMU7800.Core;

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
        InputBox[] _inputBoxen;

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

        public void Update()
        {
            if (_machineInputButtonUpCounter > 0 && --_machineInputButtonUpCounter == 0)
                RaiseMachineInput(_machineInputForButtonUpCounter, false);
        }

        public void HandleInput()
        {
            if (_inputBoxen == null)
                return;

            var touchCollection = TouchPanel.GetState();
            for (var i = 0; i < touchCollection.Count; i++)
            {
                var tl = touchCollection[i];
                HandleTouchLocationInput(tl);
                for (var j = 0; j < _inputBoxen.Length; j++)
                    _inputBoxen[j].HandleTouchLocationInput(tl);
            }

            var keyboardState = Keyboard.GetState();
            HandleKeyboardInput(keyboardState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_inputBoxen == null)
                return;

            for (var i = 0; i < _inputBoxen.Length; i++)
                _inputBoxen[i].Draw(spriteBatch);
        }

        public virtual void HandleTouchLocationInput(TouchLocation tl) {}

        public virtual void HandleKeyboardInput(KeyboardState ks) {}

        public void RaiseMachineInputWithButtonUpCounter(MachineInput machineInput, bool down)
        {
            if (_machineInputButtonUpCounter > 0)
                return;
            _machineInputButtonUpCounter = 5;
            _machineInputForButtonUpCounter = machineInput;
            RaiseMachineInput(machineInput, down);
        }

        public void RaiseMachineInput(MachineInput machineInput, bool down)
        {
            _machine.InputState.RaiseInput(_currentPlayerNo, machineInput, down);
        }

        public void RaiseMachinePaddleInput(int valMax, int val)
        {
            _machine.InputState.RaisePaddleInput(_currentPlayerNo, valMax, val);
        }

        protected void RegisterInputBoxen(InputBox[] inputBoxen)
        {
            _inputBoxen = inputBoxen;
        }

        protected Texture2D EmptyCircleTexture { get; private set; }
        protected Texture2D SelectCircleTexture { get; private set; }
        protected Texture2D ResetCircleTexture { get; private set; }

        #region Constructors

        protected InputHandler(MachineBase machine)
        {
            if (machine == null)
                throw new ArgumentNullException("machine");

            _machine = machine;
            EmptyCircleTexture = LoadTexture("appbar.basecircle.rest.png");
            SelectCircleTexture = LoadTexture("appbar.basecircle.rest_S.png");
            ResetCircleTexture = LoadTexture("appbar.basecircle.rest_R.png");
        }

        #endregion

        #region Helpers

        static Texture2D LoadTexture(string name)
        {
            using (var stream = TitleContainer.OpenStream(name))
            {
                var graphicsDevice = SharedGraphicsDeviceManager.Current.GraphicsDevice;
                return Texture2D.FromStream(graphicsDevice, stream);
            }
        }

        #endregion
    }
}
