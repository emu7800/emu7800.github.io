﻿using Moga.Windows.Phone;
using System;

namespace EMU7800.D2D.Shell
{
    public class MogaController
    {
        #region Fields

        readonly ControllerManager _manager;

        #endregion

        public bool IsConnected
        {
            get
            {
                var mogaState = GetMogaState();
                return mogaState == ControllerResult.Connected;
            }
        }

        public Single XAxisValue { get; private set; }
        public Single YAxisValue { get; private set; }
        public Single ZAxisValue { get; private set; }
        public Single RZAxisValue { get; private set; }

        public ControllerAction KeyCodeA { get; private set; }
        public ControllerAction KeyCodeB { get; private set; }
        public ControllerAction KeyCodeSelect { get; private set; }
        public ControllerAction KeyCodeReset { get; private set; }

        public void Poll()
        {
            if (!IsConnected)
            {
                XAxisValue    = 0.0f;
                YAxisValue    = 0.0f;
                KeyCodeA      = ControllerAction.Unpressed;
                KeyCodeB      = ControllerAction.Unpressed;
                KeyCodeSelect = ControllerAction.Unpressed;
                KeyCodeReset  = ControllerAction.Unpressed;
                return;
            }

            XAxisValue    = _manager.GetAxisValue(Axis.X);
            YAxisValue    = _manager.GetAxisValue(Axis.Y);
            ZAxisValue    = _manager.GetAxisValue(Axis.Z);
            RZAxisValue   = _manager.GetAxisValue(Axis.RZ);

            KeyCodeA      = _manager.GetKeyCode(KeyCode.A);
            KeyCodeB      = _manager.GetKeyCode(KeyCode.B);
            KeyCodeSelect = _manager.GetKeyCode(KeyCode.Select);
            KeyCodeReset  = _manager.GetKeyCode(KeyCode.Start);
        }

        public void Launching()
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                _manager.Connect();
            }

            catch (Exception)
            {
                // SDK samples did this, so just in case...
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        public void Activated()
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                _manager.Resuming();
            }
            catch (Exception)
            {
                // paranoia: initial submission with MOGA support failed on start-up
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        public void Deactivated()
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                _manager.Suspending();
            }
            catch (Exception)
            {
                // paranoia: initial submission with MOGA support failed on start-up
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        public void Closing()
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                _manager.Close();
            }
            catch (Exception)
            {
                // paranoia: initial submission with MOGA support failed on start-up
            }
            // ReSharper restore EmptyGeneralCatchClause
        }

        #region Constructors

        public MogaController()
        {
            _manager = new ControllerManager();
#if DEBUG
            _manager.StateChanged += _manager_StateChanged;
#endif
        }

#if DEBUG
        void _manager_StateChanged(StateEvent __param0)
        {
            System.Diagnostics.Debug.WriteLine("MogaController StateChanged: " + __param0.StateKey + "=" + __param0.StateValue);
        }
#endif

        #endregion

        #region Helpers

        ControllerResult GetMogaState()
        {
            var result = ControllerResult.False;

            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                result = _manager.GetState(ControllerState.Connection);
            }
            catch (Exception)
            {
                // paranoia: initial submission with MOGA support failed on start-up
            }
            // ReSharper restore EmptyGeneralCatchClause

            return result;
        }

        #endregion
    }
}
