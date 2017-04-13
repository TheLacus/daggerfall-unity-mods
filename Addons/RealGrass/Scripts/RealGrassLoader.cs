// Project:         RealGrass/Plants&Grass for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Developers:      Original RealGrass by Uncanny_Valley, improvements by Midopa,
//                  Grass&Plants and maintenance by TheLacus
//
// Original Author: TheLacus
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace RealGrass
{
    /// <summary>
    /// Loader for Real Grass and Grass and Plants
    /// </summary>
    public class RealGrassLoader : MonoBehaviour
    {
        // Real Grass or Grass and Plants?
        static bool waterPlants;

        // Load mod
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Load settings
            ModSettings settings = new ModSettings(initParams.Mod);
            waterPlants = settings.GetBool("Grass&Plants", "UseGrassAndPlants");

            RealGrassHelper.mod = initParams.Mod;

            // Add script to the scene.
            if (waterPlants)
            {
                GrassAndPlants.plantsAndGrassMod = initParams.Mod;
                GameObject go = new GameObject("PlantsAndGrass");
                GrassAndPlants realGrass = go.AddComponent<GrassAndPlants>();
            }
            else
            {
                RealGrass.realGrassMod = initParams.Mod;
                GameObject go = new GameObject("RealGrass");
                RealGrass realGrass = go.AddComponent<RealGrass>();
            }

            // After finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }
    }
}
