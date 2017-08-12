// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using System.Globalization;
using UnityEngine;
using Wenzil.Console;

namespace VibrantWind
{
    /// <summary>
    /// Console commands for Vibrant Wind.
    /// </summary>
    public static class VibrantWindConsoleCommands
    {
        const string noInstanceMessage = "Vibrant Wind instance not found.";

        public static void RegisterCommands()
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(ToggleVibrantWind.name, ToggleVibrantWind.description, ToggleVibrantWind.usage, ToggleVibrantWind.Execute);
                ConsoleCommandsDatabase.RegisterCommand(GetWindStrength.name, GetWindStrength.description, GetWindStrength.usage, GetWindStrength.Execute);
                ConsoleCommandsDatabase.RegisterCommand(SetWindStrength.name, SetWindStrength.description, SetWindStrength.usage, SetWindStrength.Execute);

            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering Vibrant Wind Console commands: {0}", e.Message));

            }
        }

        private static class ToggleVibrantWind
        {
            public static readonly string name = "vwind_toggle";
            public static readonly string description = "Enable/Disable automatic wind strength changes.";
            public static readonly string usage = "vwind_toggle";

            public static string Execute(params string[] args)
            {
                var vibrantWind = VibrantWind.Instance;
                if (vibrantWind == null)
                    return noInstanceMessage;

                vibrantWind.ToggleMod();
                return vibrantWind.GetStatusMessage();
            }
        }

        private static class GetWindStrength
        {
            public static readonly string name = "vwind_getstrength";
            public static readonly string description = "Get current wind strength.";
            public static readonly string usage = "vwind_getstrength";

            public static string Execute(params string[] args)
            {
                var vibrantWind = VibrantWind.Instance;
                if (vibrantWind == null)
                    return noInstanceMessage;

                return vibrantWind.WindStrength.ToString();
            }
        }

        private static class SetWindStrength
        {
            public static readonly string name = "vwind_setstrength";
            public static readonly string description = "Set immediately wind strength; values should be in range 0.0-1.0";
            public static readonly string usage = "vwind_setstrength {speed} {bending} {size}";

            public static string Execute(params string[] args)
            {
                // Allow values higher than one
                bool force = args.Length == 4 && args[3] == "-force";
                if (!force && args.Length != 3)
                    return string.Format("Unknown parameters, use {0}", usage);

                // Speed component
                float speed;
                string message = GetValue(args[0], force, out speed);
                if (message != null)
                    return message;

                // Bending component
                float bending;
                message = GetValue(args[1], force, out bending);
                if (message != null)
                    return message;

                // Size component
                float size;
                message = GetValue(args[2], force, out size);
                if (message != null)
                    return message;

                // Get instance
                var vibrantWind = VibrantWind.Instance;
                if (vibrantWind == null)
                    return noInstanceMessage;

                // Apply new value for wind strength
                vibrantWind.ApplyWindStrength(new WindStrength(speed, bending, size));
                return string.Format("Wind strength is now {0}", vibrantWind.WindStrength.ToString());
            }
        }

        private static string GetValue(string par, bool force, out float value)
        {
            const string error = "Failed to set wind strength;";
            value = default(float);

            try
            {
                value = float.Parse(par, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            catch (IndexOutOfRangeException)
            {
                return string.Format("{0} missing parameter for strength\n{1}.", error, SetWindStrength.usage);
            }
            catch (FormatException)
            {
                return string.Format("{0} {1} is not a valid number.", error, par);
            }
            catch (Exception e)
            {
                return string.Format("{0} {1}", error, e.Message);
            }

            if (value < 0)
                return string.Format("{0} strength can't be a negative value.");

            else if (value > 1 && !force)
                return string.Format("{0} strength must be lower than one.\nIf you really want use {1} -force", error, SetWindStrength.usage);

            return null;
        }
    }
}
