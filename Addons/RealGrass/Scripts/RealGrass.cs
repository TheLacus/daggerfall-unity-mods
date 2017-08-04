// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley (original Real Grass)
// Contributors:    TheLacus (Water plants, mod version and improvements) 
//                  Midopa

// #define TEST_PERFORMANCE

using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace RealGrass
{
    /// <summary>
    /// Real Grass creates detail prototypes layers on terrain to place various components:
    ///
    /// GRASS
    /// Adds a layer of tufts of grass, to give the impression of a grassy meadow. 
    /// There are two variants of grass, varying for different regions.
    ///
    /// WATER PLANTS 
    /// Places vegetation near water areas, like lakes and river.
    /// There is a total of four different plants, for the same number of climate regions: mountain, temperate, 
    /// desert and swamp. They all have two variations, summer and winter. 
    /// Additionally it places waterlilies above the surface of temperate lakes and some tufts 
    /// of grass inside the mountain water zones.
    /// Plants bend in the wind and waterlilies move slightly on the water surface moved by the same wind. 
    ///
    /// STONES
    /// Places little stones on the cultivated grounds near farms. 
    /// 
    /// FLOWERS
    /// Places flowers on grass terrain.
    ///
    /// Real Grass thread on DU forums:
    /// http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
    /// </summary>
    public class RealGrass : MonoBehaviour
    {
        #region Fields

        DetailPrototypesCreator detailPrototypesCreator;
        DetailPrototypesDensity detailPrototypesDensity;

        // Optional details
        static bool waterPlants; // Enable plants
        static bool winterPlants; // Enable plants during winter
        static bool terrainStones; // Enable stones on terrain
        static bool flowers; // Enable flowers

        // Terrain settings
        static float detailObjectDistance;
        static float detailObjectDensity;

        #endregion

        #region Unity

        /// <summary>
        /// Awake mod and set up vegetation settings
        /// </summary>
        private void Awake()
        {
            // Load settings
            LoadSettings();

            // Subscribe to the onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData += AddGrass;

            // Print on log
            string modInfo = RealGrassLoader.Mod.Title + " v." + RealGrassLoader.Mod.ModInfo.ModVersion;
            Debug.LogFormat("{0} started: subscribed to terrain promotion.", modInfo);
        }

        #endregion

        #region Add Grass

        /// <summary>
        /// Add Grass and other details on terrain.
        /// </summary>
        private void AddGrass(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {

#if TEST_PERFORMANCE

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

#endif

            // Terrain settings 
            InitTerrain(daggerTerrain, terrainData);
            Color32[] tilemap = daggerTerrain.TileMap;

            // Get the current season and climate
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int currentClimate = daggerTerrain.MapData.worldClimate;

            // Update detail layers
            detailPrototypesDensity.InitDetailsLayers();
            if (currentClimate > 225 && currentClimate != Climate.Desert3)
            {
                if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                {
                    // Summer
                    detailPrototypesCreator.UpdateClimateSummer(currentClimate);
                    detailPrototypesDensity.SetDensitySummer(tilemap, currentClimate);
                }
                else if (waterPlants && winterPlants)
                {
                    // Winter
                    detailPrototypesCreator.UpdateClimateWinter(currentClimate);
                    detailPrototypesDensity.SetDensityWinter(tilemap);
                }
            }
            else if (waterPlants && 
                (currentClimate == Climate.Desert || currentClimate == Climate.Desert2 || currentClimate == Climate.Desert3))
            {
                // Desert
                detailPrototypesCreator.UpdateClimateDesert();
                detailPrototypesDensity.SetDensityDesert(tilemap);
            }

            // Assign detail prototypes to the terrain
            terrainData.detailPrototypes = detailPrototypesCreator.DetailPrototypes;

            // Assign detail layers to the terrain
            terrainData.SetDetailLayer(0, 0, 0, detailPrototypesDensity.Grass); // Grass
            if (waterPlants)
            {
                terrainData.SetDetailLayer(0, 0, 1, detailPrototypesDensity.WaterPlants); // Water plants near water
                terrainData.SetDetailLayer(0, 0, 2, detailPrototypesDensity.Waterlilies); // Waterlilies and grass inside water
            }
            if (terrainStones)
                terrainData.SetDetailLayer(0, 0, 3, detailPrototypesDensity.Stones); // Stones
            if (flowers)
                terrainData.SetDetailLayer(0, 0, 4, detailPrototypesDensity.Flowers); // Flowers

#if TEST_PERFORMANCE

            stopwatch.Stop();
            Debug.LogFormat("RealGrass - Time elapsed: {0} ms.", stopwatch.Elapsed.Milliseconds);

#endif

        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Load settings.
        /// </summary>
        private void LoadSettings()
        {
            const string waterPlantsSection = "WaterPlants", stonesSection = "TerrainStones", flowersSection = "Flowers";

            ModSettings settings = RealGrassLoader.Settings;

            // Optional details
            waterPlants = settings.GetBool(waterPlantsSection, "Enable");
            winterPlants = settings.GetBool(waterPlantsSection, "EnableWinter");
            terrainStones = settings.GetBool(stonesSection, "Enable");
            flowers = settings.GetInt(flowersSection, "Density", 0, 100) != 0;

            // Detail prototypes
            detailPrototypesCreator = new DetailPrototypesCreator(settings, waterPlants, terrainStones, flowers);
            detailPrototypesDensity = new DetailPrototypesDensity(settings, waterPlants, terrainStones, flowers);

            // Terrain
            const string terrainSection = "Terrain";
            detailObjectDistance = settings.GetFloat(terrainSection, "DetailDistance", 10f);
            detailObjectDensity = settings.GetFloat(terrainSection, "DetailDensity", 0.1f, 1f);
        }

        /// <summary>
        /// Set settings for terrain.
        /// </summary>
        private void InitTerrain(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            // Resolution of the detail map
            terrainData.SetDetailResolution(256, 8);

            // Grass max distance and density
            Terrain terrain = daggerTerrain.gameObject.GetComponent<Terrain>();
            terrain.detailObjectDistance = detailObjectDistance;
            terrain.detailObjectDensity = detailObjectDensity;

            // Waving grass tint
            terrainData.wavingGrassTint = Color.gray;

            // Set seed for terrain
            Random.InitState(TerrainHelper.MakeTerrainKey(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY));
        }

        #endregion
    }
}