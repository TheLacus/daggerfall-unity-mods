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
using DaggerfallWorkshop.Game.Utility.ModSupport;
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

        // Details resources
        private DetailPrototype[] detailPrototype;

        // Details layers
        private int[,] details0, details1, details2, details3, details4;

        #endregion

        #region Constants

        // Size of tile map
        const int tilemapSize = 128;

        // Textures for grass billboards
        const string brownGrass = "tex_BrownGrass";
        const string greenGrass = "tex_GreenGrass";

        // Meshes for grass shader
        const string brownGrassMesh = "BrownGrass";
        const string greenGrassMesh = "GreenGrass";

        // Models for water plants
        const string TemperateGrass = "TemperateGrass"; // water plants for temperate
        const string Waterlily = "Waterlily"; // waterlilies for temperate
        const string SwampGrass = "SwampGrass"; // water plants for swamp
        const string MountainGrass = "MountainGrass"; // grass for mountain near water
        const string WaterMountainGrass = "WaterMountainGrass"; // grass for mountain inside water
        const string DesertGrass = "DesertGrass"; // grass near water for desert

        // Winter models for water plants
        const string TemperateGrassWinter = "TemperateGrassWinter"; // water plants for temperate
        const string SwampGrassWinter = "SwampGrassWinter"; // water plants for swamp
        const string MountainGrassWinter = "MountainGrassWinter"; // grass for mountain near water

        // Little stones for farms
        const string Stone = "Stone";

        // Flowers
        const string Flowers = "Flowers";

        #endregion

        #region Settings

        // Grass
        static float MinGrassHeight;
        static float MaxGrassHeight;
        static float MinGrassWidth;
        static float MaxGrassWidth;

        static int thickLower;
        static int thickHigher;
        static int thinLower;
        static int thinHigher;

        static float NoiseSpread;
        static bool useGrassShader; //GrassBillboard or Grass

        // Water plants
        static bool waterPlants;
        static bool WinterPlants; // Enable plants during winter
        static int waterPlantsLower;
        static int waterPlantsHigher;
        static int desertLower;
        static int desertHigher;
        static float NoiseSpreadPlants;

        // Stones
        static bool TerrainStones; // Enable stones on terrain
        static int stonesLower;
        static int stonesHigher;
        static float NoiseSpreadStones;

        // Flowers
        static bool flowers; // Enable flowers
        static int flowersDensity; // Density of flowers

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

            CreatePrototypes();
        }

        /// <summary>
        /// Add Grass and other details on terrain.
        /// </summary>
        private void AddGrass(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            // Used to check performance
             var stopwatch = new System.Diagnostics.Stopwatch();
             stopwatch.Start();

            // Terrain settings 
            InitTerrain(daggerTerrain, terrainData);
            Color32[] tilemap = daggerTerrain.TileMap;

            // Get the current season and climate
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int currentClimate = daggerTerrain.MapData.worldClimate;

            // Create details layers
            details0 = new int[256, 256];
            details1 = new int[256, 256];
            details2 = new int[256, 256];
            details3 = new int[256, 256];
            details4 = new int[256, 256];

            // Update detail layers
            if (currentClimate > 225 && currentClimate != Climate.Desert3)
            {
                if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                {
                    // Summer
                    UpdateClimateSummer(currentClimate);
                    SetDensitySummer(tilemap, currentClimate);
                }
                else if (waterPlants && WinterPlants)
                {
                    // Winter
                    UpdateClimateWinter(currentClimate);
                    SetDensityWinter(tilemap);
                }
            }
            else if (waterPlants && 
                (currentClimate == Climate.Desert || currentClimate == Climate.Desert2 || currentClimate == Climate.Desert3))
            {
                // Desert
                UpdateClimateDesert();
                SetDensityDesert(tilemap);
            }

            // Assign detail prototypes to the terrain
            terrainData.detailPrototypes = detailPrototype;

            // Assign detail layers to the terrain
            terrainData.SetDetailLayer(0, 0, 0, details0); // Grass
            if (waterPlants)
            {
                terrainData.SetDetailLayer(0, 0, 1, details1); // Water plants near water
                terrainData.SetDetailLayer(0, 0, 2, details2); // Waterlilies and grass inside water
            }
            if (TerrainStones)
                terrainData.SetDetailLayer(0, 0, 3, details3); // Stones
            if (flowers)
                terrainData.SetDetailLayer(0, 0, 4, details4); // Flowers

             stopwatch.Stop();
            // Write result
             Debug.Log("RealGrass - Time elapsed: " + stopwatch.Elapsed);
        }

        #endregion

        #region Update Climate
        
        /// <summary>
        /// Load assets for Summer.
        /// </summary>
        private void UpdateClimateSummer (int currentClimate)
        {
            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

                    // Mountain
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(brownGrass);
                    else
                        detailPrototype[0].prototype = RealGrassLoader.LoadGameObject(brownGrassMesh);
                    if (waterPlants)
                    {
                        detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(MountainGrass);
                        detailPrototype[2].prototype = RealGrassLoader.LoadGameObject(WaterMountainGrass);
                    }
                    break;

                case Climate.Swamp:
                case Climate.Swamp2:

                    // Swamp
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(brownGrass);
                    else
                        detailPrototype[0].prototype = RealGrassLoader.LoadGameObject(brownGrassMesh);
                    if (waterPlants)
                        detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(SwampGrass);
                    break;

                case Climate.Temperate:
                case Climate.Temperate2:

                    // Temperate
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(greenGrass);
                    else
                        detailPrototype[0].prototype = RealGrassLoader.LoadGameObject(greenGrassMesh);
                    if (waterPlants)
                    {
                        detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(TemperateGrass);
                        detailPrototype[2].prototype = RealGrassLoader.LoadGameObject(Waterlily);
                    }
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Summer)", currentClimate));
                    gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Load assets for Winter.
        /// </summary>
        private void UpdateClimateWinter(int currentClimate)
        {
            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

                    // Mountain
                    detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(MountainGrassWinter);
                    break;

                case Climate.Swamp:
                case Climate.Swamp2:

                    // Swamp
                    detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(SwampGrassWinter);
                    break;

                case Climate.Temperate:
                case Climate.Temperate2:

                    // Temperate
                    detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(TemperateGrassWinter);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Winter)", currentClimate));
                    gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// Load assets for Desert, which doesn't support seasons.
        /// </summary>
        private void UpdateClimateDesert()
        {
            detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(DesertGrass);
        }

        #endregion

        #region Set Density

        /// <summary>
        /// Set density for Summer.
        /// </summary>
        private void SetDensitySummer (Color32[] tilemap, int currentClimate)
        {
            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        // Four corner tiles
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            details0[y * 2, x * 2] = RandomThick();
                            details0[y * 2, (x * 2) + 1] = RandomThick();
                            details0[(y * 2) + 1, x * 2] = RandomThick();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThick();
                            if (flowers)
                            {
                                var index = RandomPosition(y, x);
                                details4[index.First, index.Second] = RandomFlowers();
                            }
                            break;

                        // Upper left corner 
                        case 40:
                        case 224:
                        case 164:
                        case 176:
                        case 181:
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // Lower left corner 
                        case 41:
                        case 221:
                        case 165:
                        case 177:
                        case 182:
                            details0[y * 2, x * 2] = RandomThin();
                            break;

                        // Lower right corner 
                        case 42:
                        case 222:
                        case 166:
                        case 178:
                        case 183:
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Upper right corner 
                        case 43:
                        case 223:
                        case 167:
                        case 179:
                        case 180:
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // Left side
                        case 44:
                        case 66:
                        case 160:
                        case 168:
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[y * 2, x * 2] = RandomThin();
                            break;

                        // Left side: grass and plants
                        case 84:
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[y * 2, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                                details1[y * 2, x * 2] = RandomWaterPlants();
                            }
                            break;

                        // Lower side
                        case 45:
                        case 67:
                        case 161:
                        case 169:
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[y * 2, x * 2] = RandomThin();
                            break;

                        // Lower side: grass and plants
                        case 85:
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[y * 2, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                details1[y * 2, (x * 2)] = RandomWaterPlants();
                            }
                            break;

                        // Right side
                        case 46:
                        case 64:
                        case 162:
                        case 170:
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Right side: grass and plants
                        case 86:
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            }
                            break;

                        // Upper side
                        case 47:
                        case 65:
                        case 163:
                        case 171:
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // Upper side: grass and plants
                        case 87:
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            break;

                        // All expect lower right
                        case 48:
                        case 62:
                        case 156:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect lower right: grass and plants
                        case 88:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                details1[y * 2, x * 2] = RandomWaterPlants();
                                details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            break;

                        // All expect upper right
                        case 49:
                        case 63:
                        case 157:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // All expect upper right: grass and plants
                        case 89:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            break;


                        // All expect upper left
                        case 50:
                        case 60:
                        case 158:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect upper left: grass and plants
                        case 90:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                details1[y * 2, x * 2] = RandomWaterPlants();
                                details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            break;

                        // All expect lower left
                        case 51:
                        case 61:
                        case 159:
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect lower left: grass and plants
                        case 91:
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            break;

                        // Left to right
                        case 204:
                        case 206:
                        case 214:
                            details0[y * 2, x * 2] = RandomThin();
                            details0[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // Right to left
                        case 205:
                        case 207:
                        case 213:
                            details0[(y * 2) + 1, x * 2] = RandomThin();
                            details0[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Swamp upper right corner
                        case 81:
                            if (waterPlants)
                                details1[(y * 2), (x * 2)] = RandomWaterPlants();
                            break;

                        // Swamp lower left corner
                        case 83:
                            if (waterPlants)
                                details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            break;

                        // In-water grass
                        // case 0 is not enabled because is used for the sea
                        case 1:
                        case 2:
                        case 3:
                            if (waterPlants)
                            {
                                // Mountain: grass
                                if (currentClimate == Climate.Mountain || currentClimate == Climate.Mountain2)
                                {
                                    details2[y * 2, x * 2] = Random.Range(1, 2);
                                    details2[(y * 2) + 1, (x * 2) + 1] = Random.Range(1, 2);
                                }
                                // Temperate: waterlilies
                                else if (currentClimate == Climate.Temperate || currentClimate == Climate.Temperate2)
                                {
                                    details2[y * 2, x * 2] = 1;
                                    details2[(y * 2) + 1, (x * 2) + 1] = 1;
                                    details2[(y * 2) + 1, x * 2] = 1;
                                    details2[y * 2, (x * 2) + 1] = 1;
                                }
                            }
                            break;

                        // Little stones
                        case 216:
                        case 217:
                        case 218:
                        case 219:
                            if (TerrainStones)
                            {
                                details3[y * 2, x * 2] = RandomStones();
                                details3[(y * 2) + 1, (x * 2) + 1] = RandomStones();
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set density for Winter.
        /// </summary>
        private void SetDensityWinter(Color32[] tilemap)
        {
            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        // Left side
                        case 84:
                            details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            details1[y * 2, x * 2] = RandomWaterPlants();
                            break;
                        // Lower side
                        case 85:
                            details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            details1[y * 2, (x * 2)] = RandomWaterPlants();
                            break;
                        // Right side
                        case 86:
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            break;
                        // Upper side
                        case 87:
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            break;
                        // Corners
                        case 88:
                            details1[y * 2, x * 2] = RandomWaterPlants();
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            break;
                        case 89:
                            details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            break;
                        case 90:
                            details1[y * 2, x * 2] = RandomWaterPlants();
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            break;
                        case 91:
                            details1[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            details1[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set density for Desert locations.
        /// Desert has grass around water but not on mainland, also desert regions don't have winter season.
        /// </summary>
        private void SetDensityDesert(Color32[] tilemap)
        {
            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        // Left side
                        case 84:
                            details1[(y * 2) + 1, x * 2] = RandomDesert();
                            details1[y * 2, x * 2] = RandomDesert();
                            break;
                        // Lower side
                        case 85:
                            details1[y * 2, (x * 2) + 1] = RandomDesert();
                            details1[y * 2, (x * 2)] = RandomDesert();
                            break;
                        // Right side
                        case 86:
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            details1[y * 2, (x * 2) + 1] = RandomDesert();
                            break;
                        // Upper side
                        case 87:
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            details1[(y * 2) + 1, x * 2] = RandomDesert();
                            break;
                        // Corners
                        case 88:
                            details1[y * 2, x * 2] = RandomDesert();
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            break;
                        case 89:
                            details1[y * 2, (x * 2) + 1] = RandomDesert();
                            details1[(y * 2) + 1, x * 2] = RandomDesert();
                            break;
                        case 90:
                            details1[y * 2, x * 2] = RandomDesert();
                            details1[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            break;
                        case 91:
                            details1[y * 2, (x * 2) + 1] = RandomDesert();
                            details1[(y * 2) + 1, x * 2] = RandomDesert();
                            break;
                    }
                }
            }
        }

        #endregion

        #region Random Generators

        /// <summary>
        /// Generate random values for the placement of thin grass. 
        /// </summary>
        private static int RandomThin()
        {
            return Random.Range(thinLower, thinHigher);
        }

        /// <summary>
        /// Generate random values for the placement of thick grass.
        /// </summary>
        private static int RandomThick()
        {
            return Random.Range(thickLower, thickHigher);
        }

        /// <summary>
        /// Generate random values for the placement of water plants.
        /// </summary>
        private static int RandomWaterPlants()
        {
            return Random.Range(waterPlantsLower, waterPlantsHigher);
        }

        /// <summary>
        /// Generate random values for the placement of desert grass.
        /// </summary>
        private static int RandomDesert()
        {
            return Random.Range(desertLower, desertHigher);
        }

        /// <summary>
        /// Generate random values for the placement of stones. 
        /// </summary>
        private static int RandomStones()
        {
            return Random.Range(stonesLower, stonesHigher);
        }

        /// <summary>
        /// Generate random values for the placement of flowers. 
        /// </summary>
        private static int RandomFlowers()
        {
            return Random.Range(0, 100) < flowersDensity ? 1 : 0;
        }

        /// <summary>
        /// Get a random position on tile.
        /// </summary>
        /// <param name="y">First index.</param>
        /// <param name="x">Second index.</param>
        /// <returns>One of the four possible position on tile.</returns>
        private static Tuple<int, int> RandomPosition(int y, int x)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return new Tuple<int, int>(y * 2, x * 2);
                case 1:
                    return new Tuple<int, int>(y * 2, (x * 2) + 1);
                case 2:
                    return new Tuple<int, int>((y * 2) + 1, x * 2);
                case 3:
                default:
                    return new Tuple<int, int>((y * 2) + 1, (x * 2) + 1);
            }
        }

        #endregion

        #region Setup Methods

        /// <summary>
        /// Load settings.
        /// </summary>
        private void LoadSettings()
        {
            ModSettings settings = RealGrassLoader.Settings;
            const string grass = "Grass", waterPlantsSection = "WaterPlants", stones = "TerrainStones";

            // Grass
            MinGrassHeight = settings.GetFloat(grass, "MinGrassHeight");
            MaxGrassHeight = settings.GetFloat(grass, "MaxGrassHeight");
            MinGrassWidth = settings.GetFloat(grass, "MinGrassWidth");
            MaxGrassWidth = settings.GetFloat(grass, "MaxGrassWidth");
            thickLower = settings.GetInt(grass, "thickLower");
            thickHigher = settings.GetInt(grass, "thickHigher");
            thinLower = settings.GetInt(grass, "thinLower");
            thinHigher = settings.GetInt(grass, "thinHigher");
            NoiseSpread = settings.GetFloat(grass, "NoiseSpread");
            useGrassShader = settings.GetBool("Grass", "UseGrassShader");

            // Water plants
            waterPlants = settings.GetBool("WaterPlants", "WaterPlants");
            waterPlantsLower = settings.GetInt(waterPlantsSection, "waterPlantsLower");
            waterPlantsHigher = settings.GetInt(waterPlantsSection, "waterPlantsHigher");
            desertLower = settings.GetInt(waterPlantsSection, "desertLower");
            desertHigher = settings.GetInt(waterPlantsSection, "desertHigher");
            WinterPlants = settings.GetBool(waterPlantsSection, "WinterPlants");
            NoiseSpreadPlants = settings.GetFloat(waterPlantsSection, "NoiseSpread");

            // Stones
            TerrainStones = settings.GetBool(stones, "TerrainStones");
            stonesLower = settings.GetInt(stones, "stonesLower");
            stonesHigher = settings.GetInt(stones, "stonesHigher");
            flowersDensity = settings.GetInt(stones, "flowersDensity", 0, 100);
            flowers = flowersDensity != 0;
            NoiseSpreadStones = settings.GetFloat(stones, "NoiseSpread");

            // Terrain
            const string terrainSection = "Terrain", windSection = "Wind";
            detailObjectDistance = settings.GetFloat(terrainSection, "DetailDistance", 10f);
            detailObjectDensity = settings.GetFloat(terrainSection, "DetailDensity", 0.1f, 1f);
            wavingGrassAmount = settings.GetFloat(windSection, "WavingAmount", 0f, 1f);
            wavingGrassSpeed = settings.GetFloat(windSection, "WavingSpeed", 0f, 1f);
            wavingGrassStrength = settings.GetFloat(windSection, "WavingStrength", 0f, 1f);
        }

        private void CreatePrototypes()
        {
            // Create a holder for our grass and plants
            detailPrototype = new DetailPrototype[5];

            // Grass settings
            // We use GrassBillboard or Grass rendermode
            detailPrototype[0] = new DetailPrototype()
            {
                minHeight = MinGrassHeight,
                minWidth = MinGrassWidth,
                maxHeight = MaxGrassHeight,
                maxWidth = MaxGrassWidth,
                noiseSpread = NoiseSpread,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.Grass,
                usePrototypeMesh = useGrassShader
            };

            // Near-water plants settings
            // Here we use the Grass shader which support meshes, and textures with transparency.
            // This allow us to have more realistic plants which still bend in the wind.
            detailPrototype[1] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = NoiseSpreadPlants,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.Grass
            };

            // In-water plants settings
            // We use Grass as above
            detailPrototype[2] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = NoiseSpreadPlants,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.Grass
            };

            // Little stones settings
            // For stones we use VertexLit as we are placing 3d static models.
            detailPrototype[3] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = NoiseSpreadStones,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.VertexLit,
                prototype = RealGrassLoader.LoadGameObject(Stone)
            };

            // Flowers
            detailPrototype[4] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = NoiseSpreadStones,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.Grass,
                prototype = RealGrassLoader.LoadGameObject(Flowers)
            };
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