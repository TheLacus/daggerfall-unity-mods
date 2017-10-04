// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

// #define TEST_PERFORMANCE
// #define TEST_VALUES

using System;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Weather;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace VibrantWind
{
    /// <summary>
    /// Change wind strength according to current weather.
    /// </summary>
    public class VibrantWind : MonoBehaviour
    {
        #region Fields

        // Number of weathers.
        static readonly int precision = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().Distinct().Count();

        // This mod
        static Mod mod;
        static VibrantWind instance;
        bool isEnabled = false;

        // DU components
        StreamingWorld streamingWorld;
        PlayerWeather playerWeather;
        WindZone windZone;

        /// <summary>
        /// Wind settings for terrain wind (grass).
        /// </summary>
        WindStrength[] terrainWind;

        /// <summary>
        /// Wind settings for WindZone (trees, particles).
        /// </summary>
        float[] ambientWind;

        // Index for current weather.
        int _weather;

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
        /// Vibrant Wind mod.
        /// </summary>
        public static Mod Mod
        {
            get { return mod; }
        }

        public WindStrength TerrainWindStrength
        {
            get { return terrainWind[_weather]; }
        }

        public float AmbientWindStrength
        {
            get { return ambientWind[_weather]; }
        }

        /// <summary>
        /// Current weather.
        /// </summary>
        public WeatherType Weather
        {
            get { return (WeatherType)_weather; }
            internal set { _weather = (int)value; }
        }

        #endregion

        #region Unity

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

        void Awake()
        {
            if (instance != null && this != instance)
                Destroy(this.gameObject);

            // Add commands to console
            VibrantWindConsoleCommands.RegisterCommands();
        }

        void Start()
        {
            // Get Daggerfall Unity components
            streamingWorld = GameManager.Instance.StreamingWorld;
            playerWeather = GameManager.Instance.PlayerGPS.GetComponent<PlayerWeather>();
            windZone = GameManager.Instance.WeatherManager.GetComponent<WindZone>();

            // Setup mod
            Setup();
            mod.MessageReceiver = VibrantWindModMessages.MessageReceiver;

            // Start mod
            ToggleMod(true, false);
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (mod == null)
                return base.ToString();

            return string.Format("{0} v.{1} (running: {2}).", mod.Title, mod.ModInfo.ModVersion, isEnabled);
        }

        /// <summary>
        /// Toggles Vibrant Wind mod.
        /// </summary>
        public void ToggleMod(bool applyImmediately = false)
        {
            ToggleMod(!isEnabled, applyImmediately);
        }

        /// <summary>
        /// Toggles Vibrant Wind mod.
        /// </summary>
        /// <param name="toggle">Enable or disable the mod.</param>
        public void ToggleMod(bool toggle, bool applyImmediately = false)
        {
            if (toggle == isEnabled)
                return;

            if (toggle)
            {
                StreamingWorld.OnInitWorld += StreamingWorld_OnInitWorld;
                DaggerfallTerrain.OnPromoteTerrainData += DaggerfallTerrain_OnPromoteTerrainData;
                WeatherManager.OnWeatherChange += WeatherManager_OnWeatherChange;

                if (applyImmediately)
                    ForceWeather(playerWeather.WeatherType);
            }
            else
            {
                StreamingWorld.OnInitWorld -= StreamingWorld_OnInitWorld;
                DaggerfallTerrain.OnPromoteTerrainData -= DaggerfallTerrain_OnPromoteTerrainData;
                WeatherManager.OnWeatherChange -= WeatherManager_OnWeatherChange;
            }

            isEnabled = toggle;
            Debug.Log(this.ToString());
        }

        /// <summary>
        /// Set wind values for this weather.
        /// </summary>
        public void ForceWeather(WeatherType weather)
        {
            WeatherManager_OnWeatherChange(weather);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create wind values with user settings.
        /// </summary>
        private void Setup()
        {
            // Get settings
            ModSettings settings = new ModSettings(mod);

            // Get terrain wind values
            float[] speed = GetValuesFromSettings(settings, "Speed");
            float[] bending = GetValuesFromSettings(settings, "Bending");
            float[] size = GetValuesFromSettings(settings, "Size");
            terrainWind = new WindStrength[precision];
            for (int i = 0; i < precision; i++)
                terrainWind[i] = new WindStrength(speed[i], bending[i], size[i]);

            // Get ambient wind values
            ambientWind = GetValuesFromSettings(settings, "Force");
        }

        /// <summary>
        /// Set wind strength to all terrains.
        /// </summary>
        private void SetTerrainWindStrength()
        {
            foreach (Terrain terrain in streamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>())
                terrainWind[_weather].Assign(terrain.terrainData);
        }

        /// <summary>
        /// Set wind strength to <paramref name="terrainData"/>.
        /// </summary>
        private void SetTerrainWindStrength(TerrainData terrainData)
        {
            terrainWind[_weather].Assign(terrainData);
        }

        /// <summary>
        /// Set wind strength to WindZone (does nothing if not present).
        /// </summary>
        private void SetAmbientWindStrength()
        {
            if (!windZone)
                return;

            windZone.windMain = ambientWind[_weather];
        }

        #endregion

        #region Static Methods

        private static float[] GetValuesFromSettings(ModSettings settings, string section)
        {
            var range = settings.GetTupleFloat(section, "Range");
            int interpolationType = settings.GetInt(section, "Interpolation", 0, 4);

            var interpolation = new Interpolation(range.First, range.Second, precision, interpolationType);
            float[] values = interpolation.GetValues();

#if TEST_VALUES

            Func<float[], string> allValues = x => string.Join(",", x.Select(y => y.ToString()).ToArray());
            Debug.LogFormat("VibrantWind - {0}: {1}", settings.Field, allValues(values));

#endif

            return values;
        }

        #endregion

        #region Events Handlers

        /// <summary>
        /// Update weather on terrain creation.
        /// This is called at startup and when player teleports.
        /// </summary>
        private void StreamingWorld_OnInitWorld()
        {
            if (Weather != playerWeather.WeatherType)
            {
                Weather = playerWeather.WeatherType;
                SetAmbientWindStrength();
            }
        }

        /// <summary>
        /// Apply wind strength on terrain promotion.
        /// This is called for every terrain.
        /// </summary>
        /// <param name="daggerTerrain"></param>
        /// <param name="terrainData">New terrain.</param>
        private void DaggerfallTerrain_OnPromoteTerrainData(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            SetTerrainWindStrength(terrainData);
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

            Weather = weather;

            SetTerrainWindStrength();
            SetAmbientWindStrength();

#if TEST_PERFORMANCE
            stopwatch.Stop();
            Debug.Log(string.Format("VibrantWind - time elsapsed {0}", stopwatch.Elapsed.Milliseconds));
#endif
        }

        #endregion
    }
}
