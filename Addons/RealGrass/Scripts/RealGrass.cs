// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley (original Real Grass)
// Contributors:    TheLacus (Water plants, mod version and improvements) 
//                  Midopa

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

        #endregion

        #region Settings

        // Water plants
        static bool waterPlants;
        static bool WinterPlants; // Enable plants during winter

        // Stones
        static bool TerrainStones; // Enable stones on terrain

        // Flowers
        static bool flowers; // Enable flowers

        // Terrain
        static float detailObjectDistance;
        static float detailObjectDensity;
        static float wavingGrassAmount;
        static float wavingGrassSpeed;
        static float wavingGrassStrength;

        #endregion

        #region Init Mod

        /// <summary>
        /// Awake mod and set up vegetation settings
        /// </summary>
        private void Awake()
        {
            // Load settings
            LoadSettings();

            // Subscribe to the onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData += AddGrass;
        }

        /// <summary>
        /// Add Grass and other details on terrain.
        /// </summary>
        private void AddGrass(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            // Used to check performance
            // var stopwatch = new System.Diagnostics.Stopwatch();
            // stopwatch.Start();

            // Terrain settings 
            InitTerrain(daggerTerrain, terrainData);
            Color32[] tilemap = daggerTerrain.TileMap;

            // Get the current season and climate
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int currentClimate = daggerTerrain.MapData.worldClimate;

            // Update detail layers
            if (currentClimate > 225 && currentClimate != Climate.Desert3)
            {
                if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                {
                    // Summer
                    detailPrototypesCreator.UpdateClimateSummer(currentClimate);
                    detailPrototypesDensity.SetDensitySummer(tilemap, currentClimate);
                }
                else if (waterPlants && WinterPlants)
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
            terrainData.detailPrototypes = detailPrototypesCreator.detailPrototype;

            // Assign detail layers to the terrain
            terrainData.SetDetailLayer(0, 0, 0, detailPrototypesDensity.Grass); // Grass
            if (waterPlants)
            {
                terrainData.SetDetailLayer(0, 0, 1, detailPrototypesDensity.WaterPlants); // Water plants near water
                terrainData.SetDetailLayer(0, 0, 2, detailPrototypesDensity.Waterlilies); // Waterlilies and grass inside water
            }
            if (TerrainStones)
                terrainData.SetDetailLayer(0, 0, 3, detailPrototypesDensity.Stones); // Stones
            if (flowers)
                terrainData.SetDetailLayer(0, 0, 4, detailPrototypesDensity.Flowers); // Flowers

            // stopwatch.Stop();
            // Write result
            // Debug.Log("RealGrass - Time elapsed: " + stopwatch.Elapsed);
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Load settings.
        /// </summary>
        private void LoadSettings()
        {
            const string waterPlantsSection = "WaterPlants", stonesSection = "TerrainStones";

            ModSettings settings = RealGrassLoader.Settings;
            
            // Optional details
            WinterPlants = settings.GetBool(waterPlantsSection, "WinterPlants");
            TerrainStones = settings.GetBool(stonesSection, "TerrainStones");
            flowers = settings.GetInt(stonesSection, "flowersDensity", 0, 100) != 0;

            // Detail prototypes
            detailPrototypesCreator = new DetailPrototypesCreator(settings, waterPlants);
            detailPrototypesDensity = new DetailPrototypesDensity(settings, waterPlants, TerrainStones, flowers);

            // Terrain
            const string terrainSection = "Terrain", windSection = "Wind";
            detailObjectDistance = settings.GetFloat(terrainSection, "DetailDistance", 10f);
            detailObjectDensity = settings.GetFloat(terrainSection, "DetailDensity", 0.1f, 1f);
            wavingGrassAmount = settings.GetFloat(windSection, "WavingAmount", 0f, 1f);
            wavingGrassSpeed = settings.GetFloat(windSection, "WavingSpeed", 0f, 1f);
            wavingGrassStrength = settings.GetFloat(windSection, "WavingStrength", 0f, 1f);
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

            // Waving grass settings
            terrainData.wavingGrassTint = Color.gray;
            terrainData.wavingGrassAmount = wavingGrassAmount;
            terrainData.wavingGrassSpeed = wavingGrassSpeed;
            terrainData.wavingGrassStrength = wavingGrassStrength;

            // Set seed for terrain
            Random.InitState(TerrainHelper.MakeTerrainKey(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY));
        }

        #endregion
    }
}