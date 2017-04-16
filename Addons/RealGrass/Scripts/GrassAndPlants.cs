// Project:         Plants and Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley, TheLacus
// Contributors:    

using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace RealGrass
{
    /// <summary>
    /// Real Grass by Uncanny_Valley
    /// Grass and Plants by TheLacus
    ///
    /// This is a fork of Real Grass. 
    /// It creates detail prototypes layers on terrain to place various components:
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
    /// Real Grass thread on DU forums:
    /// http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
    /// </summary>
    public class GrassAndPlants : MonoBehaviour
    {
        #region Fields

        // Textures for grass billboards
        const string brownGrass = "tex_BrownGrass";
        const string greenGrass = "tex_GreenGrass";

        // Models for water plants
        const string TemperateGrass = "TemperateGrass"; // water plants for temperate
        const string Waterlily = "Waterlily"; // waterlilies for temperate
        const string SwampGrass = "SwampGrass"; // water plants for swamp
        const string MountainGrass = "MountainGrass"; // grass for mountain near water
        const string WaterMountainGrass = "WaterMountainGrass"; // grass for mountain inside water
        const string DesertGrass = "DesertGrass"; // grass near water for desert

        // Winter models for water plants
        static bool WinterPlants;
        const string TemperateGrassWinter = "TemperateGrassWinter"; // water plants for temperate
        const string SwampGrassWinter = "SwampGrassWinter"; // water plants for swamp
        const string MountainGrassWinter = "MountainGrassWinter"; // grass for mountain near water

        // Little stones for farms
        static bool TerrainStones;
        const string Stone = "Stone";

        // Create detailprototype
        private DetailPrototype[] detailPrototype;

        // Different values that determine the overal thickness and lenght of the grass, it creates some variation depending on different tiles.
        // Grass
        static int thickLower;
        static int thickHigher;
        static int thinLower;
        static int thinHigher;
        // Water plants for temperate, mountain and swamp
        static int waterPlantsLower;
        static int waterPlantsHigher;
        // Water plants for desert
        static int desertLower;
        static int desertHigher;

        // Shape
        static float MinGrassHeight;
        static float MaxGrassHeight;
        static float MinGrassWidth;
        static float MaxGrassWidth;

        // Spread
        static float NoiseSpread;

        #endregion

        #region Start Mod

        /// <summary>
        /// Awake mod and set up vegetation settings
        /// </summary>
        private void Awake()
        {
            // Load settings
            ModSettings settings = RealGrassLoader.Settings;
            const string grassShape = "GrassShape", grassVariation = "GrassVariation", plantsVariation = "PlantsVariation";
            MinGrassHeight = settings.GetFloat(grassShape, "MinGrassHeight");
            MaxGrassHeight = settings.GetFloat(grassShape, "MaxGrassHeight");
            MinGrassWidth = settings.GetFloat(grassShape, "MinGrassWidth");
            MaxGrassWidth = settings.GetFloat(grassShape, "MaxGrassWidth");
            thickLower = settings.GetInt(grassVariation, "thickLower");
            thickHigher = settings.GetInt(grassVariation, "thickHigher");
            thinLower = settings.GetInt(grassVariation, "thinLower");
            thinHigher = settings.GetInt(grassVariation, "thinHigher");
            waterPlantsLower = settings.GetInt(plantsVariation, "waterPlantsLower");
            waterPlantsHigher = settings.GetInt(plantsVariation, "waterPlantsHigher");
            desertLower = settings.GetInt(plantsVariation, "desertLower");
            desertHigher = settings.GetInt(plantsVariation, "desertHigher");
            NoiseSpread = settings.GetFloat(grassVariation, "NoiseSpread");
            WinterPlants = settings.GetBool("Grass&Plants", "WinterPlants");
            TerrainStones = settings.GetBool("Grass&Plants", "TerrainStones");

            // Subscribe to the onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData += AddGrass;

            // Create a holder for our grass and plants
            detailPrototype = new DetailPrototype[4];

            // Grass settings
            // We use billboards as they are cheap and make the grass terrain 
            // dense from every angolation
            detailPrototype[0] = new DetailPrototype()
            {
                minHeight = MinGrassHeight,
                minWidth = MinGrassWidth,
                maxHeight = MaxGrassHeight,
                maxWidth = MaxGrassWidth,
                noiseSpread = NoiseSpread,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.GrassBillboard
            };

            // Near-water plants settings
            // Here we use the Grass shader which support meshes, and textures with transparency.
            // This allow us to have more realistic plants which still bend in the wind.
            detailPrototype[1] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = 0.4f,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.Grass
            };

            // In-water plants settings
            // We use Grass as above
            detailPrototype[2] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = 0.4f,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.Grass
            };

            // Little stones settings
            // For stones we use VertexLit as we are placing 3d static models.
            detailPrototype[3] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = 0.4f,
                healthyColor = new Color(0.70f, 0.70f, 0.70f),
                dryColor = new Color(0.70f, 0.70f, 0.70f),
                renderMode = DetailRenderMode.VertexLit,
                prototype = RealGrassLoader.LoadGameObject(Stone)
            };
        }

        #endregion

        #region Add Grass and Plants

        /// <summary>
        /// Add Grass and plants on terrain.
        /// </summary>
        private void AddGrass(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            //			Used to check performance
            //			Stopwatch stopwatch = new Stopwatch();
            //			stopwatch.Start();

            // Terrain settings 
            RealGrassLoader.InitTerrain(terrainData);
            Color32[] tilemap = daggerTerrain.TileMap;

            // Get the current season and climate
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int currentClimate = daggerTerrain.MapData.worldClimate;

            // Create details layers
            int[,] details0, details1, details2, details3;
            details0 = new int[256, 256];
            details1 = new int[256, 256];
            details2 = new int[256, 256];
            details3 = new int[256, 256];

            // Proceed if the worldClimate contains grass, which is everything above 225, with the exception of 229
            if (currentClimate > 225 && currentClimate != Climate.Desert3)
            {
                // Summer grass and water plants
                if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                {
                    // Switch the grass texture and plants model based on the climate
                    switch (currentClimate)
                    {
                        case Climate.Mountain:
                        case Climate.Mountain2:

                            // Mountain
                            detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(brownGrass);
                            detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(MountainGrass);
                            detailPrototype[2].prototype = RealGrassLoader.LoadGameObject(WaterMountainGrass);
                            break;

                        case Climate.Swamp:
                        case Climate.Swamp2:

                            // Swamp
                            detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(brownGrass);
                            detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(SwampGrass);
                            break;

                        case Climate.Temperate:
                        case Climate.Temperate2:

                            // Temperate
                            detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(greenGrass);
                            detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(TemperateGrass);
                            detailPrototype[2].prototype = RealGrassLoader.LoadGameObject(Waterlily);
                            break;

                        default:
                            Debug.Log("Grass and Plants: ERROR - Unknown climate");
                            gameObject.SetActive(false);
                            break;
                    }

                    int colorValue;

                    // Check all the tiles, Daggerfall uses the red color value to draw tiles
                    for (int i = 0; i < 128; i++)
                    {
                        for (int j = 0; j < 128; j++)
                        {
                            colorValue = tilemap[(i * 128) + j].r; //For easier checking

                            switch (colorValue)
                            {
                                // Four corner tiles
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                    details0[i * 2, j * 2] = RandomThick();
                                    details0[i * 2, (j * 2) + 1] = RandomThick();
                                    details0[(i * 2) + 1, j * 2] = RandomThick();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThick();
                                    break;

                                // Upper left corner 
                                case 40:
                                case 224:
                                case 164:
                                case 176:
                                case 181:
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    break;

                                // Lower left corner 
                                case 41:
                                case 221:
                                case 165:
                                case 177:
                                case 182:
                                    details0[i * 2, j * 2] = RandomThin();
                                    break;

                                // Lower right corner 
                                case 42:
                                case 222:
                                case 166:
                                case 178:
                                case 183:
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    break;

                                // Upper right corner 
                                case 43:
                                case 223:
                                case 167:
                                case 179:
                                case 180:
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    break;

                                // Left side
                                case 44:
                                case 66:
                                case 160:
                                case 168:
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[i * 2, j * 2] = RandomThin();
                                    break;

                                // Left side: grass and plants
                                case 84:
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[i * 2, j * 2] = RandomThin();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    break;

                                // Lower side
                                case 45:
                                case 67:
                                case 161:
                                case 169:
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[i * 2, j * 2] = RandomThin();
                                    break;

                                // Lower side: grass and plants
                                case 85:
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[i * 2, j * 2] = RandomThin();
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[i * 2, (j * 2)] = RandomWaterPlants();
                                    break;

                                // Right side
                                case 46:
                                case 64:
                                case 162:
                                case 170:
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    break;

                                // Right side: grass and plants
                                case 86:
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    break;

                                // Upper side
                                case 47:
                                case 65:
                                case 163:
                                case 171:
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    break;

                                // Upper side: grass and plants
                                case 87:
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;

                                // All expect lower right
                                case 48:
                                case 62:
                                case 156:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    break;

                                // All expect lower right: grass and plants
                                case 88:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    break;


                                // All expect upper right
                                case 49:
                                case 63:
                                case 157:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    break;

                                // All expect upper right: grass and plants
                                case 89:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;


                                // All expect upper left
                                case 50:
                                case 60:
                                case 158:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    break;

                                // All expect upper left: grass and plants
                                case 90:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    break;

                                // All expect lower left
                                case 51:
                                case 61:
                                case 159:
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    break;

                                // All expect lower left: grass and plants
                                case 91:
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;

                                // Left to right
                                case 204:
                                case 206:
                                case 214:
                                    details0[i * 2, j * 2] = RandomThin();
                                    details0[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    break;

                                // Right to left
                                case 205:
                                case 207:
                                case 213:
                                    details0[(i * 2) + 1, j * 2] = RandomThin();
                                    details0[i * 2, (j * 2) + 1] = RandomThin();
                                    break;

                                // Swamp upper right corner
                                case 81:
                                    details1[(i * 2), (j * 2)] = RandomWaterPlants();
                                    break;

                                // Swamp lower left corner
                                case 83:
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    break;

                                // In-water grass
                                // case 0 is not enabled because is used for the sea
                                case 1:
                                case 2:
                                case 3:
                                    // Mountain: grass
                                    if (currentClimate == Climate.Mountain || currentClimate == Climate.Mountain2)
                                    {
                                        details2[i * 2, j * 2] = Random.Range(1, 2);
                                        details2[(i * 2) + 1, (j * 2) + 1] = Random.Range(1, 2);
                                    }
                                    // Temperate: waterlilies
                                    else if (currentClimate == Climate.Temperate || currentClimate == Climate.Temperate2)
                                    {
                                        details2[i * 2, j * 2] = 1;
                                        details2[(i * 2) + 1, (j * 2) + 1] = 1;
                                        details2[(i * 2) + 1, j * 2] = 1;
                                        details2[i * 2, (j * 2) + 1] = 1;
                                    }
                                    break;

                                // Little stones
                                case 216:
                                case 217:
                                case 218:
                                case 219:
                                    if (TerrainStones)
                                    {
                                        details3[i * 2, j * 2] = RandomThin();
                                        details3[(i * 2) + 1, (j * 2) + 1] = RandomThin();
                                    }
                                    break;

                            }
                        }
                    }
                }
                // Winter water plants
                else if (WinterPlants)
                {
                    // Switch the plants model based on the climate
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
                            Debug.Log("Grass and Plants: ERROR - Unknown climate");
                            gameObject.SetActive(false);
                            break;
                    }

                    int colorValue;

                    // Check all the tiles, Daggerfall uses the red color value to draw tiles
                    for (int i = 0; i < 128; i++)
                    {
                        for (int j = 0; j < 128; j++)
                        {
                            colorValue = tilemap[(i * 128) + j].r; //For easier checking

                            switch (colorValue)
                            {
                                // Left side
                                case 84:
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    break;
                                // Lower side
                                case 85:
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[i * 2, (j * 2)] = RandomWaterPlants();
                                    break;
                                // Right side
                                case 86:
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    break;
                                // Upper side
                                case 87:
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;
                                // Corners
                                case 88:
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    break;
                                case 89:
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;
                                case 90:
                                    details1[i * 2, j * 2] = RandomWaterPlants();
                                    details1[(i * 2) + 1, (j * 2) + 1] = RandomWaterPlants();
                                    break;
                                case 91:
                                    details1[i * 2, (j * 2) + 1] = RandomWaterPlants();
                                    details1[(i * 2) + 1, j * 2] = RandomWaterPlants();
                                    break;
                            }
                        }
                    }
                }
            }

            // Desert has grass around water but not on mainland, also desert regions don't have winter season
            else if (currentClimate == Climate.Desert || currentClimate == Climate.Desert2 || currentClimate == Climate.Desert3)
            {
                // Assign desert grass model
                detailPrototype[1].prototype = RealGrassLoader.LoadGameObject(DesertGrass);

                int colorValue;

                // Check all the tiles, Daggerfall uses the red color value to draw tiles
                for (int i = 0; i < 128; i++)
                {
                    for (int j = 0; j < 128; j++)
                    {
                        colorValue = tilemap[(i * 128) + j].r; //For easier checking

                        switch (colorValue)
                        {
                            // Left side
                            case 84:
                                details1[(i * 2) + 1, j * 2] = RandomDesert();
                                details1[i * 2, j * 2] = RandomDesert();
                                break;
                            // Lower side
                            case 85:
                                details1[i * 2, (j * 2) + 1] = RandomDesert();
                                details1[i * 2, (j * 2)] = RandomDesert();
                                break;
                            // Right side
                            case 86:
                                details1[(i * 2) + 1, (j * 2) + 1] = RandomDesert();
                                details1[i * 2, (j * 2) + 1] = RandomDesert();
                                break;
                            // Upper side
                            case 87:
                                details1[(i * 2) + 1, (j * 2) + 1] = RandomDesert();
                                details1[(i * 2) + 1, j * 2] = RandomDesert();
                                break;
                            // Corners
                            case 88:
                                details1[i * 2, j * 2] = RandomDesert();
                                details1[(i * 2) + 1, (j * 2) + 1] = RandomDesert();
                                break;
                            case 89:
                                details1[i * 2, (j * 2) + 1] = RandomDesert();
                                details1[(i * 2) + 1, j * 2] = RandomDesert();
                                break;
                            case 90:
                                details1[i * 2, j * 2] = RandomDesert();
                                details1[(i * 2) + 1, (j * 2) + 1] = RandomDesert();
                                break;
                            case 91:
                                details1[i * 2, (j * 2) + 1] = RandomDesert();
                                details1[(i * 2) + 1, j * 2] = RandomDesert();
                                break;
                        }
                    }
                }
            }

            // Assign detail prototypes to the terrain
            terrainData.detailPrototypes = detailPrototype;

            // Assign detail layers to the terrain
            terrainData.SetDetailLayer(0, 0, 0, details0); // Grass
            terrainData.SetDetailLayer(0, 0, 1, details1); // Water plants near water
            terrainData.SetDetailLayer(0, 0, 2, details2); // Waterlilies and grass inside water
            if (TerrainStones)
                terrainData.SetDetailLayer(0, 0, 3, details3); // Stones

            //			stopwatch.Stop();
            //			// Write result
            //			UnityEngine.Debug.Log("Time elapsed: " +
            //			                      stopwatch.Elapsed);
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}