// Project:         Vibrant Wind for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

using System;
using UnityEngine;
using DaggerfallWorkshop.Game.Weather;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace VibrantWind
{
    /// <summary>
    /// Receives messages from other mods and executes commands.
    /// </summary>
    public class VibrantWindModMessages
    {
        private readonly WindManager windManager;

        internal VibrantWindModMessages(WindManager vibrantWind)
        {
            this.windManager = vibrantWind;
        }

        public void MessageReceiver(string message, object data, DFModMessageCallback _)
        {
            switch (message)
            {
                case "ForceWeather":

                    try
                    {
                        windManager.Weather = (WeatherType)data;
                    }
                    catch (InvalidCastException)
                    {
                        Debug.LogError("VibrantWind: Failed to set strength value as requested by mod; data is not WeatherType");
                    }
                    break;

                default:
                    Debug.LogError("VibrantWind: Unknown mod message.");
                    break;
            }
        }
    }
}
