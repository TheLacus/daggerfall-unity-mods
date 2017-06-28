// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    
// Version:         0.3

// #define TEST_WIND
// #define TEST_PERFORMANCE

using System;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Weather;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;

namespace VibrantWind
{
    /// <summary>
    /// Change wind strength according to current weather.
    /// </summary>
    public class VibrantWind : MonoBehaviour
    {
        #region Fields

        // This mod
        static Mod mod;
        static VibrantWind instance;

        // DU components
        StreamingWorld streamingWorld;
        PlayerWeather playerWeather;

        // Current strength of wind
        WindValues currentStrength;

        bool isEnabled = false;

        #endregion

        #region Properties

        /// <summary>
        /// Vibrant Wind instance.
        /// </summary>
        public static VibrantWind Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<VibrantWind>();
                return instance;
            }
            private set { instance = value; }
        }

        /// <summary>
        /// Gets a value indicating whether Vibrant Wind is enabled.
        /// Set new status with <see cref="ToggleMod"/>.
        /// </summary>
        public static bool IsEnabled { get { return instance.isEnabled; } }

        /// <summary>
        /// Vibrant Wind mod.
        /// </summary>
        public static Mod Mod { get { return mod; } }

        /// <summary>
        /// Strengths of wind as per user settings.
        /// </summary>
        public WindStrength windStrength { get; set; }

        /// <summary>
        /// Gets the current wind strength.
        /// </summary>
        public WindValues CurrentWindStrength { get { return currentStrength; } }

        #endregion

        #region Setup

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("VibrantWind");
            instance = go.AddComponent<VibrantWind>();

            // Set mod as Ready
            mod.IsReady = true;
        }

        private void Awake()
        {
            // Add commands to console
            VibrantWindConsoleCommands.RegisterCommands();
        }

        private void Start()
        {
            // Get Daggerfall Unity components
            streamingWorld = GameManager.Instance.StreamingWorld;
            playerWeather = GameManager.Instance.PlayerGPS.GetComponent<PlayerWeather>();

            // Get settings
            ModSettings settings = new ModSettings(mod);
            const string speedSection = "Speed", bendingSection = "Bending", sizeSection = "Size";

            Tuple<float, float> speedRange = settings.GetTupleFloat(speedSection, "Range");
            int speedInt = settings.GetInt(speedSection, "Interpolation", 0, 4);

            Tuple<float, float> bendingRange = settings.GetTupleFloat(bendingSection, "Range");
            int bendingInt = settings.GetInt(bendingSection, "Interpolation", 0, 4);

            Tuple<float, float> sizeRange = settings.GetTupleFloat(sizeSection, "Range");
            int sizeInt = settings.GetInt(sizeSection, "Interpolation", 0, 4);

            // Get all wind values
            windStrength = new WindStrengths().GetStrengths(speedRange, speedInt, bendingRange, bendingInt, sizeRange, sizeInt);

            // Subscribe to events
            ToggleMod(true);

            // Set ModMessages
            mod.MessageReceiver = VibrantWindModMessages.MessageReceiver;

            Debug.Log(string.Format("{0}, version {1}, correctly started. {2}", mod.Title, mod.ModInfo.ModVersion, windStrength));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Toggles Vibrant Wind mod.
        /// Check current status with <see cref="IsEnabled"/>.
        /// </summary>
        /// <param name="toggle">Enable or disable the mod.</param>
        public void ToggleMod(bool toggle)
        {
            if (toggle == isEnabled)
                return;

            if (toggle)
            {
                StreamingWorld.OnInitWorld += StreamingWorld_OnInitWorld;
                DaggerfallTerrain.OnPromoteTerrainData += DaggerfallTerrain_OnPromoteTerrainData;
                WeatherManager.OnWeatherChange += WeatherManager_OnWeatherChange;
            }
            else
            {
                StreamingWorld.OnInitWorld -= StreamingWorld_OnInitWorld;
                DaggerfallTerrain.OnPromoteTerrainData -= DaggerfallTerrain_OnPromoteTerrainData;
                WeatherManager.OnWeatherChange -= WeatherManager_OnWeatherChange;
            }

            isEnabled = toggle;
            Debug.Log(GetStatusMessage());
        }

        /// <summary>
        /// Gets a message for the current status.
        /// </summary>
        public string GetStatusMessage()
        {
            string status = isEnabled ? "enabled" : "disabled";
            return string.Format("{0} {1} is {2}.", mod.Title, mod.ModInfo.ModVersion, status);
        }

        /// <summary>
        /// Applies immediately the wind strength on all active terrains.
        /// </summary>
        public void ApplyWindStrength(WindValues wind)
        {
            UpdateWindStrength(wind);
            SetWindStrength();
        }

        /// <summary>
        /// Applies immediately the wind strength on all active terrains
        /// with user settings. <paramref name="windRel"/> is in range 0-1.
        /// </summary>
        public void ApplyWindStrength(float windRel)
        {
            ApplyWindStrength(windStrength[windRel]);
        }
        
        #endregion

        #region OnEvents

        /// <summary>
        /// Update wind strength on terrain creation.
        /// This is called at startup and when player teleports.
        /// </summary>
        private void StreamingWorld_OnInitWorld()
        {
            UpdateWindStrength(playerWeather.WeatherType);
        }

        /// <summary>
        /// Apply wind strength on terrain promotion.
        /// This is called for every terrain.
        /// </summary>
        /// <param name="daggerTerrain"></param>
        /// <param name="terrainData">New terrain.</param>
        private void DaggerfallTerrain_OnPromoteTerrainData(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            SetWindStrength(terrainData);
        }

        /// <summary>
        /// Update and apply wind strength on new weather.
        /// </summary>
        /// <param name="weather">Current weather.</param>
        private void WeatherManager_OnWeatherChange(WeatherType weather)
        {

#if TEST_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif
            UpdateWindStrength(weather);
            SetWindStrength();

#if TEST_PERFORMANCE
            stopwatch.Stop();
            Debug.Log(string.Format("VibrantWind - time elsapsed {0}", stopwatch.Elapsed.Milliseconds));
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set <paramref name="newStrength"/> as wind strength.
        /// Use <see cref="SetWindStrength()"/> to apply.
        /// </summary>
        private void UpdateWindStrength(WindValues windValues)
        {
            currentStrength = windValues;
        }

        /// <summary>
        /// Update wind strength for <paramref name="weather"/>.
        /// Use <see cref="SetWindStrength()"/> to apply.
        /// </summary>
        /// <param name="weather">Current weather.</param>
        private void UpdateWindStrength(WeatherType weather)
        {
            currentStrength = GetWindStrength(weather);

#if TEST_WIND
            Debug.Log(string.Format("VibrantWind: weather [{0}], {1}", weather, currentStrength));
#endif
        }

        /// <summary>
        /// Gets the wind strength for <paramref name="weather"/>.
        /// </summary>
        /// <param name="weather">New weather.</param>
        /// <returns>New wind strength.</returns>
        private WindValues GetWindStrength(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Sunny:
                default:
                    return windStrength.None;

                case WeatherType.Cloudy:
                    return windStrength.VeryLight;

                case WeatherType.Overcast:
                case WeatherType.Fog:
                    return windStrength.Light;

                case WeatherType.Rain:
                    return windStrength.Medium;

                case WeatherType.Thunder:
                    return windStrength.Strong;

                case WeatherType.Snow:
                    return windStrength.VeryStrong;
            }
        }

        /// <summary>
        /// Set wind strength to all terrains.
        /// </summary>
        private void SetWindStrength()
        {
            try
            {
                Terrain[] terrains = streamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>();
                foreach (TerrainData terrainData in terrains.Select(x => x.terrainData))
                    SetWindStrength(terrainData);
            }
            catch (Exception e)
            {
                Debug.LogError("VibrantWind: " + e.ToString());
            }
        }

        /// <summary>
        /// Set wind strength to <paramref name="terrainData"/>.
        /// </summary>
        private void SetWindStrength(TerrainData terrainData)
        {

#if TEST_WIND
            Debug.Log(string.Format("VibrantWind - previous values: {0}, {1}, {2}, new value: {3}", terrainData.wavingGrassStrength, 
                terrainData.wavingGrassAmount, terrainData.wavingGrassSpeed, currentStrength));
#endif

            terrainData.wavingGrassStrength = currentStrength.Speed;
            terrainData.wavingGrassAmount = currentStrength.Bending;
            terrainData.wavingGrassSpeed = currentStrength.Size;
        }

        #endregion
    }
}
