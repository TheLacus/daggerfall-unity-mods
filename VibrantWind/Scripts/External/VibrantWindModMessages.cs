// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace VibrantWind
{
    /// <summary>
    /// Receives messages from other mods and executes commands.
    /// </summary>
    public static class VibrantWindModMessages
    {
        public static void MessageReceiver(string message, object data, DFModMessageCallback callBack)
        {
            const string GetStrength = "GetStrength"; // Return current strength of wind.
            const string SetStrength = "SetStrength"; // Set strength of wind.
            const string SetStrengthRel = "SetStrengthRel"; // Set strength of wind with user settings.

            switch (message)
            {
                case GetStrength:

                    if (callBack != null)
                    {
                        // Return wind strength as Vector3 (x: speed, y: bending, z: size)
                        callBack(GetStrength, (Vector3)VibrantWind.Instance.WindStrength);
                    }
                    else
                        Debug.LogError("VibrantWind: Failed to send strength value to mod; callBack is null");
                    break;

                case SetStrength:

                    try
                    {
                        // Set wind strength with float values from Vector3 (as GetStrength)
                        VibrantWind.Instance.ApplyWindStrength((Vector3)data);
                    }
                    catch (InvalidCastException)
                    {
                        Debug.LogError("VibrantWind: Failed to set strength value as requested by mod; data is not Vector3 of float");
                    }
                    break;

                case SetStrengthRel:

                    try
                    {
                        // Set strength with user settings. 'data' is a float from 0 (0% of strength) to 1 (100% of strength)
                        VibrantWind.Instance.ApplyWindStrength((float)data);
                    }
                    catch (InvalidCastException)
                    {
                        Debug.LogError("VibrantWind: Failed to set strength value as requested by mod; data is not float");
                    }
                    break;

                default:
                    Debug.LogError("VibrantWind: Unknown mod message.");
                    break;
            }
        }
    }
}
