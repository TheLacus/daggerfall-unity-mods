// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley (original Real Grass)
// Contributors:    TheLacus (mod version, additional terrain details) 
//                  Midopa

// #define TEST_PERFORMANCE

using System.Collections;
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
    #region Static Classes

    public static class DaysOfYear
    {
        public const int

            GrowDay =  1 * 30 + 15,
            Spring  =  2 * 30 +  1,
            Summer  =  5 * 30 +  1,
            MidYear =  6 * 30 + 15,
            Fall    =  8 * 30 +  1,
            Winter  = 11 * 30 +  1,
            DieDay  = 12 * 30 - 15;
    }

    #endregion

    /// <summary>
    /// Places grass and other details on Daggerall Unity terrain.
    /// </summary>
    public class RealGrass : MonoBehaviour
    {
        #region Fields

        static RealGrass instance;  
        static GameObject fireflies;
        static GameObject butterflies;

        DetailPrototypesManager detailPrototypesManager;
        DensityManager densityManager;
        bool isEnabled;
        Coroutine initTerrains;

        internal const string TexturesFolder = "Grass";

        #endregion

        #region Properties

        public static RealGrass Instance
        {
            get { return instance ?? (instance = FindObjectOfType<RealGrass>()); }
        }

        public static Mod Mod { get; private set; }

        public bool RealisticGrass { get; private set; }

        // Optional features
        public bool WaterPlants { get; private set; }
        public bool WinterPlants { get; private set; }
        public bool TerrainStones { get; private set; }
        public bool Flowers { get; private set; }

        /// <summary>
        /// Make flying insects with particle systems.
        /// </summary>
        public bool FlyingInsects { get; private set; }

        /// <summary>
        /// Details will be rendered up to this distance from the player.
        /// </summary>
        public float DetailObjectDistance { get; set; }

        /// <summary>
        /// General density of details.
        /// </summary>
        public float DetailObjectDensity { get; set; }

        #endregion

        #region Unity

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            Mod = initParams.Mod;

            // Add script to the scene.
            GameObject go = new GameObject("RealGrass");
            instance = go.AddComponent<RealGrass>();

            // After finishing, set the mod's IsReady flag to true.
            Mod.IsReady = true;
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
            Mod.MessageReceiver = MessageReceiver;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (Mod == null)
                return base.ToString();

            return string.Format("{0} v.{1}", Mod.Title, Mod.ModInfo.ModVersion);
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

        #region Internal Methods    

        /// <summary>
        /// Instantiate particle system insects at the given position.
        /// </summary>
        internal void DoInsects(bool isNight, Vector3 position)
        {
            GameObject particles = isNight ?
                (fireflies ?? (fireflies = Mod.GetAsset<GameObject>("Fireflies"))) :
                (butterflies ?? (butterflies = Mod.GetAsset<GameObject>("Butterflies")));

            GameObject go = Instantiate(particles, position, Quaternion.identity);
            GameManager.Instance.StreamingWorld.TrackLooseObject(go, false, -1, -1, true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add Grass and other details on terrain.
        /// </summary>
        private void AddTerrainDetails(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
#if TEST_PERFORMANCE

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

#endif

            Random.InitState(TerrainHelper.MakeTerrainKey(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY));

            // Terrain settings
            terrainData.SetDetailResolution(256, 8);
            terrainData.wavingGrassTint = Color.gray;
            Terrain terrain = daggerTerrain.gameObject.GetComponent<Terrain>();
            terrain.detailObjectDistance = DetailObjectDistance;
            terrain.detailObjectDensity = DetailObjectDensity;

            // Get the current season and climate
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            ClimateBases climate = GetClimate(daggerTerrain.MapData.worldClimate);

            // Update detail layers
            Color32[] tilemap = daggerTerrain.TileMap;
            densityManager.InitDetailsLayers();
            switch (climate)
            {
                case ClimateBases.Temperate:
                case ClimateBases.Mountain:
                case ClimateBases.Swamp:
                    if (currentSeason != DaggerfallDateTime.Seasons.Winter)
                    {
                        // Summer
                        detailPrototypesManager.UpdateClimateSummer(climate);
                        densityManager.SetDensitySummer(terrain, tilemap, climate);
                    }
                    else
                    {
                        // Winter
                        detailPrototypesManager.UpdateClimateWinter(climate);
                        densityManager.SetDensityWinter(terrain, tilemap, climate);
                    }
                    break;

                case ClimateBases.Desert:
                    detailPrototypesManager.UpdateClimateDesert();
                    densityManager.SetDensityDesert(tilemap);
                    break;
            }

            // Assign detail prototypes to the terrain
            terrainData.detailPrototypes = detailPrototypesManager.DetailPrototypes;

            // Assign detail layers to the terrain
            terrainData.SetDetailLayer(0, 0, detailPrototypesManager.Grass, densityManager.Grass);
            if (RealisticGrass)
            {
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.GrassDetails, densityManager.GrassDetails);
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.GrassAccents, densityManager.GrassAccents);
            }
            if (WaterPlants)
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.WaterPlants, densityManager.WaterPlants);
            if (TerrainStones)
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.Rocks, densityManager.Rocks);
            if (Flowers)
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.Flowers, densityManager.Flowers);

#if TEST_PERFORMANCE

            stopwatch.Stop();
            Debug.LogFormat("RealGrass - Time elapsed: {0} ms.", stopwatch.Elapsed.Milliseconds);

#endif
        }

        /// <summary>
        /// Start mod and optionally add grass to existing terrains.
        /// </summary>
        /// <param name="loadSettings">Load user settings.</param>
        /// <param name="initTerrains">Add Grass to existing terrains (unnecessary at startup).</param>
        private void StartMod(bool loadSettings, bool initTerrains)
        {
            if (loadSettings)
            {
                Mod.LoadSettingsCallback = LoadSettings;
                Mod.LoadSettings();
            }

            // Subscribe to events
            DaggerfallTerrain.OnPromoteTerrainData += DaggerfallTerrain_OnPromoteTerrainData;

            // Place details on existing terrains
            if (initTerrains)
                RefreshTerrainDetailsAsync();

            Debug.Log("Real Grass is now enabled; subscribed to terrain promotion.");
        }

        /// <summary>
        /// Stop mod and remove grass fom existing terrains.
        /// </summary>
        private void StopMod()
        {
            // Unsubscribe from events
            DaggerfallTerrain.OnPromoteTerrainData -= DaggerfallTerrain_OnPromoteTerrainData;          

            // Remove details from terrains
            Terrain[] terrains = GameManager.Instance.StreamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>();
            foreach (TerrainData terrainData in terrains.Select(x => x.terrainData))
            {
                foreach (var layer in terrainData.GetSupportedLayers(0, 0, terrainData.detailWidth, terrainData.detailHeight))
                    terrainData.SetDetailLayer(0, 0, layer, DensityManager.Empty);
                terrainData.detailPrototypes = null;
            }

            Debug.Log("Real Grass is now disabled; unsubscribed from terrain promotion.");
        }

        private void LoadSettings(ModSettings settings, ModSettingsChange change)
        {
            const string
                waterPlantsSection  = "WaterPlants",
                grassSection        = "Grass",
                othersSection       = "Others",
                advancedSection     = "Advanced";

            // Optional details
            int waterPlantsMode = settings.GetInt(waterPlantsSection, "Mode");
            WaterPlants = waterPlantsMode != 0;
            WinterPlants = waterPlantsMode == 2;

            // Detail prototypes settings
            var properties = new PrototypesProperties()
            {
                GrassHeight = settings.GetTupleFloat(grassSection, "Height"),
                GrassWidth = settings.GetTupleFloat(grassSection, "Width"),
                NoiseSpread = settings.GetFloat(grassSection, "NoiseSpread"),
                GrassColors = new GrassColors()
                {
                    SpringHealthy = settings.GetColor(grassSection, "SpringHealthy"),
                    SpringDry = settings.GetColor(grassSection, "SpringDry"),
                    SummerHealty = settings.GetColor(grassSection, "SummerHealty"),
                    SummerDry = settings.GetColor(grassSection, "SummerDry"),
                    FallHealty = settings.GetColor(grassSection, "FallHealty"),
                    FallDry = settings.GetColor(grassSection, "FallDry"),
                },
                UseGrassShader = settings.GetInt(grassSection, "Shader") == 1,
                NoiseSpreadPlants = settings.GetFloat(waterPlantsSection, "NoiseSpread"),
                TextureOverride = settings.GetBool(advancedSection, "TextureOverride")
            };

            // Detail prototypes density
            var density = new Density()
            {
                GrassThick = settings.GetTupleInt(grassSection, "ThickDensity"),
                GrassThin = settings.GetTupleInt(grassSection, "ThinDensity"),
                WaterPlants = settings.GetTupleInt(waterPlantsSection, "Density"),
                DesertPlants = settings.GetTupleInt(waterPlantsSection, "DesertDensity"),
            };

            switch (settings.GetInt(othersSection, "Flowers"))
            {
                case 0:
                    Flowers = false;
                    break;
                case 1:
                    Flowers = true;
                    density.Flowers = 5;
                    break;
                case 2:
                    Flowers = true;
                    density.Flowers = 25;
                    break;
                case 3:
                    Flowers = true;
                    density.Flowers = 50;
                    break;
            }

            switch (settings.GetInt(othersSection, "Stones"))
            {
                case 0:
                    TerrainStones = false;
                    break;

                case 1:
                    TerrainStones = true;
                    density.Rocks = 2;
                    break;
                case 2:
                    TerrainStones = true;
                    density.Rocks = 4;
                    break;
            }

            if (change.HasChanged(grassSection, "Realistic"))
                RealisticGrass = settings.GetValue<bool>(grassSection, "Realistic");
            
            if (change.HasChanged(othersSection, "FlyingInsects"))
                FlyingInsects = settings.GetValue<bool>(othersSection, "FlyingInsects");

            if (change.HasChanged(advancedSection))
            {
                DetailObjectDistance = settings.GetValue<int>(advancedSection, "DetailDistance");
                DetailObjectDensity = settings.GetValue<float>(advancedSection, "DetailDensity");
            }

            detailPrototypesManager = new DetailPrototypesManager(properties);
            densityManager = new DensityManager(density);

            if (isEnabled)
                RefreshTerrainDetailsAsync();
        }

        /// <summary>
        /// Re-applies detail layers to terrains.
        /// </summary>
        private void RefreshTerrainDetailsAsync()
        {
            if (initTerrains != null)
                StopCoroutine(initTerrains);

            initTerrains = StartCoroutine(RefreshTerrainDetailsCoroutine());
        }

        private IEnumerator RefreshTerrainDetailsCoroutine()
        {
            // Do player terrain first
            var playerTerrain = GameManager.Instance.StreamingWorld.PlayerTerrainTransform.GetComponentInChildren<DaggerfallTerrain>();
            AddTerrainDetails(playerTerrain, playerTerrain.gameObject.GetComponent<Terrain>().terrainData);
            yield return null;

            // Do other terrains
            var terrains = GameManager.Instance.StreamingWorld.StreamingTarget.GetComponentsInChildren<DaggerfallTerrain>();
            foreach (DaggerfallTerrain daggerTerrain in terrains.Where(x => x != playerTerrain))
            {
                AddTerrainDetails(daggerTerrain, daggerTerrain.gameObject.GetComponent<Terrain>().terrainData);
                yield return null;
            }

            initTerrains = null;
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

        private void MessageReceiver(string message, object data, DFModMessageCallback callBack)
        {
            switch (message)
            {
                case "toggle":
                    if (data is bool)
                        ToggleMod((bool)data);
                    break;

                default:
                    Debug.LogErrorFormat("{0}: unknown message received ({1}).", this, message);
                    break;
            }
        }

        #endregion

        #region Event Handlers

        private void DaggerfallTerrain_OnPromoteTerrainData(DaggerfallTerrain sender, TerrainData terrainData)
        {
            AddTerrainDetails(sender, terrainData);
        }

        #endregion
    }
}