// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley (original Real Grass)
// Contributors:    TheLacus (mod version, additional terrain details) 
//                  Midopa

// #define TEST_PERFORMANCE

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Climates = DaggerfallConnect.Arena2.MapsFile.Climates;

namespace RealGrass
{
    [Flags]
    public enum GrassStyle
    {
        Classic = 0,
        Mixed = 1,
        Full = 1 | 2,
    }

    public static class DaysOfYear
    {
        public const int

            GrowDay = 1 * 30 + 15,
            Spring = 2 * 30 + 1,
            Summer = 5 * 30 + 1,
            MidYear = 6 * 30 + 15,
            Fall = 8 * 30 + 1,
            Winter = 11 * 30 + 1,
            DieDay = 12 * 30 - 15;
    }

    public class RealGrassOptions
    {
        internal GrassStyle GrassStyle { get; set; }
        public bool WaterPlants { get; set; }
        public bool TerrainStones { get; set; }
        public bool FlyingInsects { get; set; }
        public float DetailObjectDistance { get; set; }
        public float DetailObjectDensity { get; set; }
    }

    /// <summary>
    /// Places grass and other details on Daggerall Unity terrain.
    /// </summary>
    public class RealGrass : MonoBehaviour
    {
        #region Fields

        internal const string TexturesFolder = "Grass";

        private static Mod mod;

        private readonly RealGrassOptions options = new RealGrassOptions();
        DetailPrototypesManager detailPrototypesManager;
        DensityManager densityManager;
        bool isEnabled;
        Coroutine initTerrains;

        #endregion

        #region Unity

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            GameObject go = new GameObject(mod.Title);
            go.AddComponent<RealGrass>();
        }

        void Awake()
        {
            Debug.Log($"{this} started.");
        }

        void Start()
        {
            StartMod(true, false);
            isEnabled = true;

            RealGrassConsoleCommands.RegisterCommands(this, options);
            mod.MessageReceiver = MessageReceiver;
            mod.IsReady = true;
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

            UnityEngine.Random.InitState(TerrainHelper.MakeTerrainKey(daggerTerrain.MapPixelX, daggerTerrain.MapPixelY));

            // Terrain settings
            terrainData.SetDetailResolution(256, 8);
            terrainData.wavingGrassTint = Color.gray;
            Terrain terrain = daggerTerrain.gameObject.GetComponent<Terrain>();
            terrain.detailObjectDistance = options.DetailObjectDistance;
            terrain.detailObjectDensity = options.DetailObjectDensity;

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
                        detailPrototypesManager.UpdateClimateSummer(climate);
                        densityManager.SetDensitySummer(terrain, tilemap, climate);
                    }
                    else
                    {
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
            if ((options.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed && climate != ClimateBases.Desert)
            {
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.GrassDetails, densityManager.GrassDetails);
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.GrassAccents, densityManager.GrassAccents);
            }
            if (options.WaterPlants)
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.WaterPlants, densityManager.WaterPlants);
            if (options.TerrainStones)
                terrainData.SetDetailLayer(0, 0, detailPrototypesManager.Rocks, densityManager.Rocks);

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
                mod.LoadSettingsCallback = LoadSettings;
                mod.LoadSettings();
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
                style = "Style",
                waterPlantsSection = "WaterPlants",
                grassSection = "Grass",
                advancedSection = "Advanced";

            options.WaterPlants = settings.GetBool(waterPlantsSection, "Enabled");
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
                    SeasonInterpolation = settings.GetBool(grassSection, "SeasonInterpolation")
                },
                UseGrassShader = !settings.GetBool(style, "Billboard"),
                TextureOverride = settings.GetBool(advancedSection, "TextureOverride")
            };

            var density = new Density()
            {
                GrassThick = settings.GetTupleInt(grassSection, "ThickDensity"),
                GrassThin = settings.GetTupleInt(grassSection, "ThinDensity"),
                WaterPlants = settings.GetTupleInt(waterPlantsSection, "Density"),
                DesertPlants = settings.GetTupleInt(waterPlantsSection, "DesertDensity"),
            };

            switch (settings.GetInt(style, "Stones"))
            {
                case 0:
                    options.TerrainStones = false;
                    break;

                case 1:
                    options.TerrainStones = true;
                    density.Rocks = 2;
                    break;
                case 2:
                    options.TerrainStones = true;
                    density.Rocks = 4;
                    break;
            }

            if (change.HasChanged(style, "Style"))
            {
                switch (settings.GetValue<int>(style, "Style"))
                {
                    case 0:
                    default:
                        options.GrassStyle = GrassStyle.Classic;
                        break;
                    case 1:
                        options.GrassStyle = GrassStyle.Mixed;
                        break;
                    case 2:
                        options.GrassStyle = GrassStyle.Full;
                        break;
                }
            }

            if (change.HasChanged(advancedSection))
            {
                options.DetailObjectDistance = settings.GetValue<int>(advancedSection, "DetailDistance");
                string detailDistanceOverride = settings.GetValue<string>(advancedSection, "DetailDistanceOverride");
                if (!string.IsNullOrWhiteSpace(detailDistanceOverride) && int.TryParse(detailDistanceOverride, out int value))
                {
                    options.DetailObjectDistance = value;
                    Debug.Log($"{this}: override detail distance with {value}", this);
                }

                options.DetailObjectDensity = settings.GetValue<float>(advancedSection, "DetailDensity");
                options.FlyingInsects = settings.GetValue<bool>(advancedSection, "FlyingInsects");
            }

            detailPrototypesManager = new DetailPrototypesManager(mod, transform, options, properties);
            densityManager = new DensityManager(mod, options, density);

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