using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EMU7800.Core;

namespace EMU7800.SL.Model
{
    public class ControllerInfo
    {
        #region Fields

        static readonly Collection<ControllerInfo> _controllerTypeCollection = new Collection<ControllerInfo>
        {
            new ControllerInfo(Controller.None),
            new ControllerInfo(Controller.Joystick),
            new ControllerInfo(Controller.ProLineJoystick, "ProLine Joystick"),
            new ControllerInfo(Controller.Paddles),
            new ControllerInfo(Controller.Driving),
            new ControllerInfo(Controller.BoosterGrip, "Booster Grip"),
            new ControllerInfo(Controller.Lightgun),
        };

        #endregion

        public Controller ControllerType { get; private set; }

        public string ControllerTypeName { get; private set; }

        public static ICollection<ControllerInfo> ControllerTypeCollection
        {
            get { return new ReadOnlyCollection<ControllerInfo>(_controllerTypeCollection); }
        }

        public static ControllerInfo ToControllerInfo(Controller controllerType)
        {
            return ControllerTypeCollection.First(c => c.ControllerType == controllerType);
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
