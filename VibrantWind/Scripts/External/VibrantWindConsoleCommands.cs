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

                vibrantWind.ToggleMod(!VibrantWind.IsEnabled);
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

                return vibrantWind.CurrentWindText;
            }
        }

        private static class SetWindStrength
        {
            public static readonly string name = "vwind_setstrength";
            public static readonly string description = "Set immediately wind strength; should be in range 0.0-1.0";
            public static readonly string usage = "vwind_setstrength {x.y}";

            public static string Execute(params string[] args)
            {
                const string error = "Failed to set wind strength;";
                float strength;

                try
                {
                    // Get strength from arguments
                    strength = float.Parse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                catch (IndexOutOfRangeException)
                {
                    return string.Format("{0} missing parameter for strength\n{1}.", error, usage);
                }
                catch (FormatException)
                {
                    return string.Format("{0} {1} is not a valid number.", error, args[0]);
                }
                catch (Exception e)
                {
                    return string.Format("{0} {1}", error, e.Message);
                }

                // Check value
                if (strength < 0)
                    return string.Format("{0} strength can't be a negative value.");
                else if (strength > 1)
                {
                    if (args.Length != 2 || args[1] != "-force")
                        return string.Format("{0} strength must be lower than one.\nIf you really want use {1} -force", error, usage);
                }
                else if (args.Length != 1)
                    return string.Format("{0} Unknown parameters.", error);

                // Get instance
                var vibrantWind = VibrantWind.Instance;
                if (vibrantWind == null)
                    return noInstanceMessage;

                // Apply new value for wind strength
                vibrantWind.ApplyWindStrength(strength);
                return string.Format("Wind strength is now {0}", strength);
            }
        }
    }
}
