// Project:         RealGrass/Plants&Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Developers:      Original RealGrass by Uncanny_Valley, improvements by Midopa,
//                  Grass&Plants and maintenance by TheLacus (TheLacus@yandex.com)
//
// Original Author: TheLacus
// Contributors:

using System.IO;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility.AssetInjection;

namespace RealGrass
{
    #region Structs

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

    #endregion

    /// <summary>
    /// Loader for Real Grass and Grass and Plants
    /// </summary>
    public class RealGrassLoader
    {
        #region Fields & Properties

        // This mod
        private static Mod mod;

        // Settings
        private static ModSettings settings;
        private static float
            detailObjectDistance,
            detailObjectDensity,
            wavingGrassAmount,
            wavingGrassSpeed,
            wavingGrassStrength;

        // Resources folder on disk
        private static string resourcesFolder;

        /// <summary>
        /// Real Grass settings
        /// </summary>
        public static ModSettings Settings
        {
            get { return settings; }
            internal set { settings = value; }
        }

        #endregion

        #region Mod Loader

        /// <summary>
        /// Mod loader.
        /// </summary>
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;

            // Load settings
            settings = new ModSettings(mod);
            LoadTerrainSettings();

            // Get resources folder
            resourcesFolder = Path.Combine(mod.DirPath, "Resources");

            // Add script to the scene.
            if (settings.GetBool("WaterPlants", "WaterPlants"))
            {
                // Grass and water plants
                GameObject go = new GameObject("PlantsAndGrass");
                /*GrassAndPlants realGrass =*/ go.AddComponent<GrassAndPlants>();
            }
            else
            {
                // Only grass
                GameObject go = new GameObject("RealGrass");
                /*RealGrass realGrass =*/ go.AddComponent<RealGrass>();
            }

            // After finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set common settings for terrain.
        /// </summary>
        public static void InitTerrain(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            // Resolution of the detail map
            terrainData.SetDetailResolution(256, 8);

            // Grass max distance and density
            Terrain terrain = daggerTerrain.gameObject.GetComponent<Terrain>();
            terrain.detailObjectDistance = detailObjectDistance;
            terrain.detailObjectDensity = detailObjectDensity;

            // Waving grass settings
            terrainData.wavingGrassTint = Color.gray;
            terrainData.wavingGrassAmount = wavingGrassAmount;
            terrainData.wavingGrassSpeed = wavingGrassSpeed;
            terrainData.wavingGrassStrength = wavingGrassStrength;

            // Set seed for terrain
            Random.InitState(TerrainHelper.MakeTerrainKey(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY));
        }

        /// <summary>
        /// Load common settings for terrain.
        /// </summary>
        private static void LoadTerrainSettings()
        {
            const string terrainSection = "Terrain", windSection = "Wind";
            detailObjectDistance = settings.GetFloat(terrainSection, "DetailDistance", 10f);
            detailObjectDensity = settings.GetFloat(terrainSection, "DetailDensity", 0.1f, 1f);
            wavingGrassAmount = settings.GetFloat(windSection, "WavingAmount", 0f, 1f);
            wavingGrassSpeed = settings.GetFloat(windSection, "WavingSpeed", 0f, 1f);
            wavingGrassStrength = settings.GetFloat(windSection, "WavingStrength", 0f, 1f);
        }

        /// <summary>
        /// Use Grass or GrassBillboard shader for terrain grass.
        /// </summary>
        /// <param name="useGrassShader">Use Grass shader?</param>
        public static DetailRenderMode GetGrassShader(out bool useGrassShader)
        {
            useGrassShader = settings.GetBool("Grass", "UseGrassShader");

            if (!useGrassShader)
            {
                // We use billboards as they are cheap and make the grass terrain 
                // dense from every angolation
                return DetailRenderMode.GrassBillboard;
            }
            else
            {
                // Optionally uses the grass rendermode
                return DetailRenderMode.Grass;
            }
        }

        /// <summary>
        /// Get texture from disk or from mod.
        /// </summary>
        /// <param name="name">Name of texture.</param>
        public static Texture2D LoadTexture(string name)
        {
            Texture2D tex;

            if (!TextureReplacement.ImportTextureFromDisk(resourcesFolder, name, out tex))
                tex = mod.GetAsset<Texture2D>(name);

            if (tex != null)
                return tex;

            Debug.LogError("Real Grass: Failed to load texture " + name);
            return null;
        }

        /// <summary>
        /// Get gameobject from mod and customize
        /// texture from disk asset (if present).
        /// </summary>
        /// <param name="name">Name of gameobject.</param>
        public static GameObject LoadGameObject(string name)
        {
            var go = mod.GetAsset<GameObject>(name);

            if (go != null)
            {
                Texture2D tex;
                Material material = go.GetComponent<MeshRenderer>().material;

                if (TextureReplacement.ImportTextureFromDisk(resourcesFolder, material.mainTexture.name, out tex))
                    material.mainTexture = tex;

                return go;
            }

            Debug.LogError("Real Grass: Failed to load model " + name);
            return null;
        }
        
        #endregion
    }
}
