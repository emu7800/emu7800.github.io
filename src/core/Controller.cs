/*
 * Controller.cs
 *
 * Defines the set of all known controllers.
 *
 * Copyright © 2010, 2020 Mike Murphy
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Core
{
    public enum Controller
    {
        None,
        Joystick,
        Paddles,
        Keypad,
        Driving,
        BoosterGrip,
        ProLineJoystick,
        Lightgun,
    }

    public static class ControllerUtil
    {
        public static Controller From(string controllerStr)
            => Enum.TryParse<Controller>(controllerStr, true, out var c) ? c : Controller.None;

        public static Controller From(string controllerStr, string machineTypeStr)
        {
            var controller = From(controllerStr);
            return controller != Controller.None
                ? controller
                : MachineTypeUtil.From(machineTypeStr) switch
                {
                    MachineType.A2600NTSC or MachineType.A2600PAL => Controller.Joystick,
                    MachineType.A7800NTSC or MachineType.A7800PAL => Controller.ProLineJoystick,
                    _ => Controller.None,
                };
        }

        public static string ToControllerWordString(Controller controller, bool plural = false)
            => Pluralize(controller switch
            {
                Controller.ProLineJoystick => "Proline Joystick",
                Controller.Joystick        => "Joystick",
                Controller.Paddles         => "Paddle",
                Controller.Keypad          => "Keypad",
                Controller.Driving         => "Driving Paddle",
                Controller.BoosterGrip     => "Booster Grip",
                Controller.Lightgun        => "Lightgun",
                _ => string.Empty,
            }, plural);

        public static string ToString(Controller controller)
            => controller.ToString();

        public static IEnumerable<Controller> GetAllValues(bool excludeNone = true)
            => Enum.GetValues<Controller>()
                .Where(c => !excludeNone || c != Controller.None);

        static string Pluralize(string root, bool plural)
            => plural && root.Length > 0 ? root + "s" : string.Empty;
    }
}