// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using UnityEngine;
using DaggerfallWorkshop.Game.Weather;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace VibrantWind
{
    /// <summary>
    /// Receives messages from other mods and executes commands.
    /// </summary>
    public static class VibrantWindModMessages
    {
        public static void MessageReceiver(string message, object data, DFModMessageCallback _)
        {
            switch (message)
            {
                case "ForceWeather":

                    try
                    {
                        VibrantWind.Instance.ForceWeather((WeatherType)data);
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
