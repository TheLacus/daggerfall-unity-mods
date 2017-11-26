// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley (original Real Grass)
// Contributors:    TheLacus (Water plants, mod version and improvements) 
//                  Midopa

// #define TEST_PERFORMANCE

using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using Climates = DaggerfallConnect.Arena2.MapsFile.Climates;

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

        static RealGrass instance;
        static Mod mod;
        static ModSettings settings;

        bool isEnabled;

        DetailPrototypesCreator detailPrototypesCreator;
        DetailPrototypesDensity detailPrototypesDensity;

        // Optional details
        bool waterPlants, winterPlants, terrainStones, flowers;

        // Terrain settings
        float detailObjectDistance;
        float detailObjectDensity;

        #endregion

        #region Properties

        // Singleton
        public static RealGrass Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<RealGrass>();
                return instance;
            }
        }

        public static Mod Mod
        {
            get { return mod; }
        }

        /// <summary>
        /// Resources folder on disk.
        /// </summary>
        public static string ResourcesFolder
        {
            get { return Path.Combine(mod.DirPath, "Resources"); }
        }

        // Details status
        public bool WaterPlants { get { return waterPlants; } }
        public bool WinterPlants { get { return winterPlants; } }
        public bool TerrainStones { get { return terrainStones; } }
        public bool Flowers { get { return flowers; } }

        /// <summary>
        /// Details will be rendered up to this distance from the player.
        /// </summary>
        public float DetailObjectDistance
        {
            get { return detailObjectDistance; }
            set { detailObjectDistance = value; }
        }

        #endregion

        #region Unity

        /// <summary>
        /// Mod loader.
        /// </summary>
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;

            // Add script to the scene.
            GameObject go = new GameObject("RealGrass");
            instance = go.AddComponent<RealGrass>();

            // After finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
        }

        void Awake()
        {
            if (instance != null && this != instance)
                Destroy(this.gameObject);

            Debug.LogFormat("{0} started.", this);
        }

        void Start()
        {
            StartMod(true, false);
            isEnabled = true;

            RealGrassConsoleCommands.RegisterCommands();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (mod == null)
                return base.ToString();

            return string.Format("{0} v.{1}", mod.Title, mod.ModInfo.ModVersion);
        }

        /// <summary>
        /// Toggle mod and add/remove grass on existing terrains.
        /// </summary>
        /// <returns>New status of mod.</returns>
        public bool ToggleMod()
        {
            ToggleMod(!isEnabled);
            return isEnabled;
        }

        /// <summary>
        /// Set status of mod and add/remove grass on existing terrains.
        /// </summary>
        /// <param name="enable">New status to set.</param>
        public void ToggleMod(bool enable)
        {
            if (isEnabled == enable)
                return;

            if (enable)
                StartMod(false, true);
            else
                StopMod();

            isEnabled = enable;
        }

        /// <summary>
        /// Restart mod to apply changes.
        /// </summary>
        public void RestartMod()
        {
            if (enabled)
                StopMod();

            StartMod(false, true);

            isEnabled = true;
        }

        #endregion

        #region Mod Logic

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
            ClimateBases climate = GetClimate(daggerTerrain.MapData.worldClimate);

            // Update detail layers
            detailPrototypesDensity.InitDetailsLayers();
            switch (climate)
            {
                case ClimateBases.Temperate:
                case ClimateBases.Mountain:
                case ClimateBases.Swamp:
                    if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                    {
                        // Summer
                        detailPrototypesCreator.UpdateClimateSummer(climate);
                        detailPrototypesDensity.SetDensitySummer(tilemap, climate);
                    }
                    else if (waterPlants && winterPlants)
                    {
                        // Winter
                        detailPrototypesCreator.UpdateClimateWinter(climate);
                        detailPrototypesDensity.SetDensityWinter(tilemap);
                    }
                    break;

                case ClimateBases.Desert:
                    if (waterPlants)
                    {
                        detailPrototypesCreator.UpdateClimateDesert();
                        detailPrototypesDensity.SetDensityDesert(tilemap);
                    }
                    break;
            }

            // Assign detail prototypes to the terrain
            terrainData.detailPrototypes = detailPrototypesCreator.DetailPrototypes;

            // Assign detail layers to the terrain
            Indices indices = detailPrototypesCreator.Indices;
            terrainData.SetDetailLayer(0, 0, indices.Grass, detailPrototypesDensity.Grass); // Grass
            if (waterPlants)
            {
                terrainData.SetDetailLayer(0, 0, indices.WaterPlants, detailPrototypesDensity.WaterPlants); // Water plants near water
                terrainData.SetDetailLayer(0, 0, indices.Waterlilies, detailPrototypesDensity.Waterlilies); // Waterlilies and grass inside water
            }
            if (terrainStones)
            {
                terrainData.SetDetailLayer(0, 0, indices.Stones, detailPrototypesDensity.Stones); // Stones
                terrainData.SetDetailLayer(0, 0, indices.Rocks, detailPrototypesDensity.Rocks);
            }
            if (flowers)
            {
                terrainData.SetDetailLayer(0, 0, indices.Bushes, detailPrototypesDensity.Bushes);
                terrainData.SetDetailLayer(0, 0, indices.Flowers, detailPrototypesDensity.Flowers); // Flowers
                terrainData.SetDetailLayer(0, 0, indices.CommonFlowers, detailPrototypesDensity.CommonFlowers);
            }

#if TEST_PERFORMANCE

            stopwatch.Stop();
            Debug.LogFormat("RealGrass - Time elapsed: {0} ms.", stopwatch.Elapsed.Milliseconds);

#endif
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

        #region Private Methods

        /// <summary>
        /// Start mod and optionally add grass to existing terrains.
        /// </summary>
        /// <param name="loadSettings">Load user settings.</param>
        /// <param name="initTerrains">Add Grass to existing terrains (unnecessary at startup).</param>
        private void StartMod(bool loadSettings, bool initTerrains)
        {
            if(loadSettings)
            {
                try
                {
                    // Load settings and setup components
                    PrototypesProperties properties;
                    Density density;
                    LoadSettings(out properties, out density);
                    detailPrototypesCreator = new DetailPrototypesCreator(properties);
                    detailPrototypesDensity = new DetailPrototypesDensity(density);
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("RealGrass: Failed to setup mod as per user settings!\n{0}", e.ToString());
                    return;
                }
            }

            // Subscribe to onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData += AddGrass;

            // Place details on existing terrains
            if(initTerrains)
                StartCoroutine(InitTerrains());

            Debug.Log("Real Grass is now enabled; subscribed to terrain promotion.");
        }

        private IEnumerator InitTerrains()
        {
            var terrains = GameManager.Instance.StreamingWorld.StreamingTarget.GetComponentsInChildren<DaggerfallTerrain>();
            foreach (DaggerfallTerrain daggerTerrain in terrains)
            {
                AddGrass(daggerTerrain, daggerTerrain.gameObject.GetComponent<Terrain>().terrainData);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Stop mod and remove grass fom existing terrains.
        /// </summary>
        private void StopMod()
        {
            // Unsubscribe from onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData -= AddGrass;

            // Remove details from terrains
            Terrain[] terrains = GameManager.Instance.StreamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>();
            foreach (TerrainData terrainData in terrains.Select(x => x.terrainData))
            {
                for (int i = 0; i < 5; i++)
                    terrainData.SetDetailLayer(0, 0, i, detailPrototypesDensity.Empty);
                terrainData.detailPrototypes = null;
            }

            Debug.Log("Real Grass is now disabled; unsubscribed from terrain promotion.");
        }

        /// <summary>
        /// Load settings.
        /// </summary>
        private void LoadSettings(out PrototypesProperties properties, out Density density)
        {
            const string waterPlantsSection = "WaterPlants", grassSection = "Grass", texturesSection = "Textures";

            // Load settings
            settings = new ModSettings(mod);

            // Optional details
            int waterPlantsMode = settings.GetInt(waterPlantsSection, "Mode", 0, 2);
            waterPlants = waterPlantsMode != 0;
            winterPlants = waterPlantsMode == 2;

            // Detail prototypes settings
            properties = new PrototypesProperties()
            {
                import = settings.GetBool(texturesSection, "Import"),
                packName = settings.GetString(texturesSection, "Pack"),
                grassHeight = settings.GetTupleFloat(grassSection, "Height"),
                grassWidth = settings.GetTupleFloat(grassSection, "Width"),
                noiseSpread = settings.GetFloat(grassSection, "NoiseSpread"),
                grassColors = new GrassColors()
                {
                    springHealthy = settings.GetColor(grassSection, "SpringHealthy"),
                    springDry = settings.GetColor(grassSection, "SpringDry"),
                    summerHealty = settings.GetColor(grassSection, "SummerHealty"),
                    summerDry = settings.GetColor(grassSection, "SummerDry"),
                    fallHealty = settings.GetColor(grassSection, "FallHealty"),
                    fallDry = settings.GetColor(grassSection, "FallDry"),
                },
                useGrassShader = settings.GetInt(grassSection, "Shader", 0, 1) == 1,
                noiseSpreadPlants = settings.GetFloat(waterPlantsSection, "NoiseSpread"),
            };

            // Detail prototypes density
            density = new Density()
            {
                grassThick = settings.GetTupleInt(grassSection, "ThickDensity"),
                grassThin = settings.GetTupleInt(grassSection, "ThinDensity"),
                waterPlants = settings.GetTupleInt(waterPlantsSection, "Density"),
                desertPlants = settings.GetTupleInt(waterPlantsSection, "DesertDensity"),
            };

            switch(settings.GetInt("Others", "Flowers"))
            {
                case 0:
                    flowers = false;
                    break;
                case 1:
                    flowers = true;
                    density.flowers = 5;
                    density.bushes = 2;
                    break;
                case 2:
                    flowers = true;
                    density.flowers = 25;
                    density.bushes = 7;
                    break;
                case 3:
                    flowers = true;
                    density.flowers = 50;
                    density.bushes = 15;
                    break;
            }

            switch (settings.GetInt("Others", "Stones"))
            {
                case 0:
                    terrainStones = false;
                    break;

                case 1:
                    terrainStones = true;
                    density.stones = new Range<int>(2, 6);
                    density.rocks = 2;
                    break;
                case 2:
                    terrainStones = true;
                    density.stones = new Range<int>(4, 12);
                    density.rocks = 4;
                    break;
            }

            detailObjectDistance = settings.GetFloat("Advanced", "DetailDistance", 10f);
            detailObjectDensity = settings.GetFloat("Advanced", "DetailDensity", 0.1f, 1f);
        }

        private static ClimateBases GetClimate(int climateIndex)
        {
            switch ((Climates)climateIndex)
            {
                case Climates.Swamp:
                case Climates.Rainforest:
                    return ClimateBases.Swamp;

                case Climates.Desert:
                case Climates.Desert2:
                case Climates.Subtropical:
                    return ClimateBases.Desert;

                case Climates.Mountain:
                case Climates.MountainWoods:
                    return ClimateBases.Mountain;

                case Climates.Woodlands:
                case Climates.HauntedWoodlands:
                    return ClimateBases.Temperate;

                case Climates.Ocean:
                default:
                    return (ClimateBases)(-1);
            }
        }

        #endregion
    }
}