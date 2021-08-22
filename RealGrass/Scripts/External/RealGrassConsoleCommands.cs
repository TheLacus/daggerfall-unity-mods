// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using System.Globalization;
using UnityEngine;
using Wenzil.Console;

namespace RealGrass
{
    public static class RealGrassConsoleCommands
    {
        public static void RegisterCommands(RealGrass realGrass, RealGrassOptions options)
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(ToggleRealGrass.name, ToggleRealGrass.description, ToggleRealGrass.usage, ToggleRealGrass.Execute(realGrass));
                ConsoleCommandsDatabase.RegisterCommand(DetailObjectDistance.name, DetailObjectDistance.description, DetailObjectDistance.usage, DetailObjectDistance.Execute(realGrass, options));
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering RealGrass Console commands: {0}", e.Message));
            }
        }

        private static class ToggleRealGrass
        {
            public static readonly string name = "realgrass_toggle";
            public static readonly string description = "Enable/Disable RealGrass mod.";
            public static readonly string usage = "realgrass_toggle";

            public static ConsoleCommandCallback Execute(RealGrass realGrass)
            {
                return args => $"RealGrass is now {(realGrass.ToggleMod() ? "enabled" : "disabled")}";
            }
        }

        private static class DetailObjectDistance
        {
            public static readonly string name = "realgrass_distance";
            public static readonly string description = "Change detail object distance; should be in range 0-250.";
            public static readonly string usage = "realgrass_distance {value}";

            public static ConsoleCommandCallback Execute(RealGrass realGrass, RealGrassOptions options)
            {
                return args =>
                {
                    if (args.Length < 1 || !float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                        return usage;

                    if ((value < 0 || value > 250) && (args.Length < 2 || args[1] != "-force"))
                        return "Use {value} {-force}. High values can fill memory and crash the game.";

                    options.DetailObjectDistance = value;
                    realGrass.RestartMod();
                    return string.Format("Detail object distance is {0}", value);
                };
            }
        }
    }
}
