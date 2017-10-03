// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using UnityEngine;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Weather;

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
                ConsoleCommandsDatabase.RegisterCommand(ForceWeather.name, ForceWeather.description, ForceWeather.usage, ForceWeather.Execute);

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

                vibrantWind.ToggleMod(true);
                return vibrantWind.ToString();
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

                return string.Format("Terrain: {0}\nAmbient: {1}", vibrantWind.TerrainWindStrength, vibrantWind.AmbientWindStrength);
            }
        }

        private static class ForceWeather
        {
            public static readonly string name = "vwind_forceweather";
            public static readonly string description = "Set weather for wind strength.";
            public static readonly string usage = "vwind_forceweather {weather index}";

            public static string Execute(params string[] args)
            {
                var vibrantWind = VibrantWind.Instance;
                if (vibrantWind == null)
                    return noInstanceMessage;

                try
                {
                    vibrantWind.ForceWeather((WeatherType)int.Parse(args[0]));
                    return "weather set.";
                }
                catch { return usage; }
            }
        }
    }
}
