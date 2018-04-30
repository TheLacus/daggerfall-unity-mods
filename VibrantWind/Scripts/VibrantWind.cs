// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

// #define TEST_PERFORMANCE

using System;
using System.Collections;
using System.Linq;
using System.Text;
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
        int weather = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Vibrant Wind instance.
        /// </summary>
        public static VibrantWind Instance
        {
            get { return instance ?? (instance = FindObjectOfType<VibrantWind>()); }
        }

        /// <summary>
        /// Vibrant Wind mod.
        /// </summary>
        public static Mod Mod { get; private set; }

        public WindStrength TerrainWindStrength
        {
            get { return terrainWind[weather]; }
        }

        public float AmbientWindStrength
        {
            get { return ambientWind[weather]; }
        }

        /// <summary>
        /// Current weather.
        /// </summary>
        public WeatherType Weather
        {
            get { return (WeatherType)weather; }
            private set { weather = (int)value; }
        }

        #endregion

        #region Unity

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            // Get mod
            Mod = initParams.Mod;

            // Add script to scene
            GameObject go = new GameObject("VibrantWind");
            instance = go.AddComponent<VibrantWind>();

            // Set mod as Ready
            Mod.IsReady = true;
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
            playerWeather = GameManager.Instance.WeatherManager.PlayerWeather;
            windZone = GameManager.Instance.WeatherManager.GetComponent<WindZone>();

            // Setup mod
            Setup();
            Mod.MessageReceiver = VibrantWindModMessages.MessageReceiver;

            // Start mod
            ToggleMod(true, false);
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (Mod == null)
                return base.ToString();

            return string.Format("{0} v.{1} (running: {2}).", Mod.Title, Mod.ModInfo.ModVersion, isEnabled);
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

        /// <summary>
        /// Test all weathers and print results to log.
        /// </summary>
        internal void StartTestWeathers()
        {
            StartCoroutine(TestWeathers());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create wind values with user settings.
        /// </summary>
        private void Setup()
        {
#if TEST_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

            // Get settings
            ModSettings settings = Mod.GetSettings();
            var speed = GetInterpolation(settings, "Speed");
            var bending = GetInterpolation(settings, "Bending");
            var size = GetInterpolation(settings, "Size");
            var force = GetInterpolation(settings, "Force");

            // Get terrain values
            terrainWind = new WindStrength[precision];
            for (int i = 0; i < precision; i++)
                terrainWind[i] = new WindStrength(speed[i], bending[i], size[i]);

            // Get ambient values
            ambientWind = force.ToArray();

#if TEST_PERFORMANCE
            stopwatch.Stop();
            Debug.LogFormat("VibrantWind: setup, elapsed {0} ms", stopwatch.Elapsed.Milliseconds);
#endif
        }

        /// <summary>
        /// Set wind strength to all terrains.
        /// </summary>
        private void SetTerrainWindStrength()
        {
            foreach (Terrain terrain in streamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>())
                terrainWind[weather].Assign(terrain.terrainData);
        }

        /// <summary>
        /// Set wind strength to <paramref name="terrainData"/>.
        /// </summary>
        private void SetTerrainWindStrength(TerrainData terrainData)
        {
            terrainWind[weather].Assign(terrainData);
        }

        /// <summary>
        /// Set wind strength to WindZone (does nothing if not present).
        /// </summary>
        private void SetAmbientWindStrength()
        {
            if (!windZone)
                return;

            windZone.windMain = ambientWind[weather];
        }

        private IEnumerator TestWeathers()
        {
            const int waitSeconds = 6;
            const string spacer = "################################################";

            var weatherManager = GameManager.Instance.WeatherManager;
            WeatherType currentWeather = Weather;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(spacer);

            foreach (var weather in Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().Distinct())
            {
                weatherManager.SetWeather(weather);
                DaggerfallUI.Instance.PopupMessage(weather.ToString());

                stringBuilder.AppendLine(weather.ToString().ToUpperInvariant());
                stringBuilder.AppendLine(string.Format("Terrain: {0}", TerrainWindStrength));
                stringBuilder.AppendLine(string.Format("Ambient: {0}", AmbientWindStrength));

                yield return new WaitForSeconds(waitSeconds);
            }

            weatherManager.SetWeather(currentWeather);
            stringBuilder.Append(spacer);
            Debug.Log(stringBuilder);
            DaggerfallUI.Instance.PopupMessage("Test ended");
        }

        #endregion

        #region Static Methods

        private static Interpolation GetInterpolation(ModSettings settings, string section)
        {
            var interpolation = new Interpolation();
            settings.Deserialize(section, ref interpolation, true);
            interpolation.Precision = precision;
            return interpolation;
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
            Debug.LogFormat("VibrantWind: weather changed, elapsed {0} ms", stopwatch.Elapsed.Milliseconds);
#endif
        }

        #endregion
    }
}
