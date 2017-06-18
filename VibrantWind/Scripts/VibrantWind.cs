// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        -
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    
// Version:         0.1

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

        // DU components
        StreamingWorld streamingWorld;
        PlayerWeather playerWeather;

        // Current strength of wind
        float currentStrength = 0;

        #region Properties

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
        public float CurrentWindStrength { get { return currentStrength; } }
        
        #endregion

        #endregion

        #region Setup

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            mod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("VibrantWind");
            go.AddComponent<VibrantWind>();

            // Set mod as Ready
            mod.IsReady = true;
        }

        void Start()
        {
            // Get Daggerfall Unity components
            streamingWorld = GameManager.Instance.StreamingWorld;
            playerWeather = GameManager.Instance.PlayerGPS.GetComponent<PlayerWeather>();

            // Get settings
            ModSettings settings = new ModSettings(mod);
            const string windStrengthSection = "WindStrength";
            Tuple<float, float> range = settings.GetTupleFloat(windStrengthSection, "Range");
            int interpolation = settings.GetInt(windStrengthSection, "Interpolation", 0, 4);

            // Get all strength values
            windStrength = WindStrengths.GetStrengths(range, interpolation);

            // Subscribe to events
            StreamingWorld.OnInitWorld += StreamingWorld_OnInitWorld;
            DaggerfallTerrain.OnPromoteTerrainData += DaggerfallTerrain_OnPromoteTerrainData;
            WeatherManager.OnWeatherChange += WeatherManager_OnWeatherChange;

            // Set ModMessages
            mod.MessageReciver = MessageReceiver;

            Debug.Log(string.Format("{0}, version {1}, correctly started. Wind is in range {2}-{3}", 
                mod.Title, mod.ModInfo.ModVersion, windStrength.None, windStrength.VeryStrong));
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
        private void UpdateWindStrength(float newStrength)
        {
            currentStrength = newStrength;
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
            Debug.Log(string.Format("VibrantWind: {0}, {1}", currentStrength, weather));
#endif
        }

        /// <summary>
        /// Gets the wind strength for <paramref name="weather"/>.
        /// </summary>
        /// <param name="weather">New weather.</param>
        /// <returns>New wind strength.</returns>
        private float GetWindStrength(WeatherType weather)
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

            terrainData.wavingGrassStrength = currentStrength;
            terrainData.wavingGrassAmount = currentStrength;
            terrainData.wavingGrassSpeed = currentStrength;
        }

        #endregion

        #region ModMessages

        /// <summary>
        /// Receive messages from other mods.
        /// </summary>
        private void MessageReceiver(string message, object data, DFModMessageCallback callBack)
        {
            const string GetStrength = "GetStrength"; // Return current strength of wind.
            const string SetStrength = "SetStrength"; // Set strength of wind.

            switch (message)
            {
                case GetStrength:
                    if (callBack != null)
                        callBack(GetStrength, currentStrength);
                    else
                        Debug.LogError("VibrantWind: Failed to send strength value to mod; callBack is null");
                    break;

                case SetStrength:
                    try
                    {
                        UpdateWindStrength((float)data);
                        SetWindStrength();
                    }
                    catch (InvalidCastException)
                    {
                        Debug.LogError("VibrantWind: Failed to set strength value as requested by mod; data is not float");
                    }
                    break;
            }
        }
        
        #endregion
    }
}
