// Project:         RealGrass/Plants&Grass for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Original Author: TheLacus
// Contributors:

using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace RealGrass
{
    /// <summary>
    /// Daggerfall climates.
    /// </summary>
    public struct Climate
    {
        public const int

            None = 0,
            Ocean = 223,
            Desert = 224,
            Desert2 = 225,
            Mountain = 226,
            Swamp = 227,
            Swamp2 = 228,
            Desert3 = 229,
            Mountain2 = 230,
            Temperate = 231,
            Temperate2 = 232;
    }

    public static class RealGrassHelper
    {
        // Real grass mod
        public static Mod mod;

        /// <summary>
        /// Get texture from mod.
        /// </summary>
        /// <param name="name">Name of texture.</param>
        public static Texture2D LoadTexture(string name)
        {
            var tex = mod.GetAsset<Texture2D>(name);

            if (tex != null)
                return tex;

            Debug.LogError("Real Grass: Failed to load texture " + name);
            return null;
        }

        /// <summary>
        /// Get gameobject from mod.
        /// </summary>
        /// <param name="name">Name of gameobject.</param>
        public static GameObject LoadGameObject(string name)
        {
            var go = mod.GetAsset<GameObject>(name);

            if (go != null)
                return go;

            Debug.LogError("Real Grass: Failed to load model " + name);
            return null;
        }
    }
}
