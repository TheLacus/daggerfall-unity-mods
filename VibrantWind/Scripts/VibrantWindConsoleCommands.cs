// Project:         Vibrant Wind for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

using System;
using UnityEngine;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Weather;
using System.Collections;
using DaggerfallWorkshop.Game;
using System.Text;
using System.Linq;

namespace VibrantWind
{
    /// <summary>
    /// Console commands for Vibrant Wind.
    /// </summary>
    public static class VibrantWindConsoleCommands
    {
        public static void RegisterCommands(VibrantWind vibrantWind)
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(ToggleVibrantWind.name, ToggleVibrantWind.description, ToggleVibrantWind.usage, ToggleVibrantWind.Execute(vibrantWind));
                ConsoleCommandsDatabase.RegisterCommand(VibrantWindDebug.name, VibrantWindDebug.description, VibrantWindDebug.usage, VibrantWindDebug.Execute(vibrantWind));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static class ToggleVibrantWind
        {
            public static readonly string name = "vwind_toggle";
            public static readonly string description = "Enable/Disable automatic wind strength changes.";
            public static readonly string usage = "vwind_toggle";

            public static ConsoleCommandCallback Execute(VibrantWind vibrantWind)
            {
                return (args) =>
                {
                    vibrantWind.ToggleMod(true);
                    return vibrantWind.ToString();
                };
            }
        }

        private static class VibrantWindDebug
        {
            public static readonly string name = "vwind_debug";
            public static readonly string description = "Debug tools.";
            public static readonly string usage = Usage();

            public static ConsoleCommandCallback Execute(VibrantWind vibrantWind)
            {
                WindManager windManager = vibrantWind.WindManager;
                return (args) =>
                {
                    if (args.Length < 1 || !int.TryParse(args[0], out int mode))
                        return usage;

                    switch (mode)
                    {
                        case 0:
                            return string.Format("Terrain: {0}\nAmbient: {1}", windManager.TerrainWindStrength, windManager.AmbientWindStrength);

                        case 1:
                            try
                            {
                                if (!int.TryParse(args[1], out int weather) || !Enum.IsDefined(typeof(WeatherType), weather))
                                    return usage;

                                windManager.Weather = (WeatherType)weather;
                                return "weather set.";
                            }
                            catch
                            {
                                return usage;
                            }

                        case 2:
                            vibrantWind.StartCoroutine(TestWeathers(windManager));
                            return "Close console to start test.";
                    }

                    return usage;
                };
            }

            private static string Usage()
            {
                string usage = "vwind_debug {mode}";
                usage += "\n0: Get current wind strength";
                usage += "\n1: Set weather for wind strength.";
                usage += "\n2: Test all weathers in succession.";
                return usage;
            }

            private static IEnumerator TestWeathers(WindManager windManager)
            {
                const int waitSeconds = 6;
                const string spacer = "################################################";

                var weatherManager = GameManager.Instance.WeatherManager;
                WeatherType currentWeather = windManager.Weather;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(spacer);

                foreach (var weather in Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().Distinct())
                {
                    weatherManager.SetWeather(weather);
                    DaggerfallUI.Instance.PopupMessage(weather.ToString());

                    stringBuilder.AppendLine(weather.ToString().ToUpperInvariant());
                    stringBuilder.AppendLine(string.Format("Terrain: {0}", windManager.TerrainWindStrength));
                    stringBuilder.AppendLine(string.Format("Ambient: {0}", windManager.AmbientWindStrength));

                    yield return new WaitForSeconds(waitSeconds);
                }

                weatherManager.SetWeather(currentWeather);
                stringBuilder.Append(spacer);
                Debug.Log(stringBuilder);
                DaggerfallUI.Instance.PopupMessage("Test ended");
            }
        }
    }
}
