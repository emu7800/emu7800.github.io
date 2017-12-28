using System.Collections.Generic;

using EMU7800.Core;

namespace EMU7800.WP.Model
{
    public class ControllerInfo
    {
        #region Fields

        static readonly Dictionary<Controller, ControllerInfo> _controllerTypeDict = new Dictionary <Controller, ControllerInfo>
        {
            { Controller.None,              new ControllerInfo(Controller.None) },
            { Controller.Joystick,          new ControllerInfo(Controller.Joystick) },
            { Controller.ProLineJoystick,   new ControllerInfo(Controller.ProLineJoystick, "ProLine Joystick") },
            { Controller.Paddles,           new ControllerInfo(Controller.Paddles) },
            { Controller.Driving,           new ControllerInfo(Controller.Driving) },
            { Controller.BoosterGrip,       new ControllerInfo(Controller.BoosterGrip, "Booster Grip") },
            { Controller.Lightgun,          new ControllerInfo(Controller.Lightgun) },
        };

        #endregion

        public Controller ControllerType { get; private set; }

        public string ControllerTypeName { get; private set; }

        public static ControllerInfo ToControllerInfo(Controller controllerType)
        {
            return _controllerTypeDict[controllerType];
        }

        #region Constructors

        public ControllerInfo() : this(Controller.None)
        {
        }

        public ControllerInfo(Controller controllerType) : this(controllerType, null)
        {
        }

        public ControllerInfo(Controller controllerType, string controllerTypeName)
        {
            ControllerType = controllerType;
            ControllerTypeName = controllerTypeName ?? controllerType.ToString();
        }

        #endregion
    }
}
