// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

// #define TEST_WIND
// #define TEST_PERFORMANCE

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

        // This mod
        static Mod mod;
        static VibrantWind instance;
        bool isEnabled = false;

        // DU components
        StreamingWorld streamingWorld;
        PlayerWeather playerWeather;

        /// <summary>
        /// All possible values for the wind strength.
        /// </summary>
        WindProfiles wind;

        /// <summary>
        /// Current wind strength.
        /// </summary>
        WindStrength windStrength;

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

        /// <summary>
        /// Current wind strength.
        /// </summary>
        public WindStrength WindStrength
        {
            get { return windStrength; }
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

            // Get settings
            ModSettings settings = new ModSettings(mod);
            const string speedSection = "Speed", bendingSection = "Bending", sizeSection = "Size";

            // Get all wind values
            var Speed = new StrengthSettings() {
                Range = settings.GetTupleFloat(speedSection, "Range"),
                Interpolation = settings.GetInt(speedSection, "Interpolation", 0, 4)
            };
            var Bending = new StrengthSettings() {
                Range = settings.GetTupleFloat(bendingSection, "Range"),
                Interpolation = settings.GetInt(bendingSection, "Interpolation", 0, 4)
            };
            var Size = new StrengthSettings()
            {
                Range = settings.GetTupleFloat(sizeSection, "Range"),
                Interpolation = settings.GetInt(sizeSection, "Interpolation", 0, 4)
            };
            wind = WindProfilesCreator.Get(Speed, Bending, Size);

            // Start mod
            Debug.Log(this.ToString());
            mod.MessageReceiver = VibrantWindModMessages.MessageReceiver;
            ToggleMod(true);
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            if (mod == null || wind == null)
                return base.ToString();

            return string.Format("{0} v.{1} - {2}", mod.Title, mod.ModInfo.ModVersion, wind.ToString());
        }

        /// <summary>
        /// Toggles Vibrant Wind mod.
        /// </summary>
        public void ToggleMod()
        {
            ToggleMod(!isEnabled);
        }

        /// <summary>
        /// Toggles Vibrant Wind mod.
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
            return string.Format("Vibrant Wind is {0}", isEnabled ? "enabled" : "disabled");
        }

        /// <summary>
        /// Applies immediately the wind strength on all active terrains.
        /// </summary>
        public void ApplyWindStrength(WindStrength strength)
        {
            UpdateWindStrength(strength);
            SetWindStrength();
        }

        /// <summary>
        /// Applies immediately the wind strength on all active terrains with user settings.
        /// <paramref name="relativeStrength"/> is in range 0: no wind - 1: max wind.
        /// </summary>
        public void ApplyWindStrength(float relativeStrength)
        {
            ApplyWindStrength(wind[relativeStrength]);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set current wind strength.
        /// Use <see cref="SetWindStrength()"/> to apply.
        /// </summary>
        private void UpdateWindStrength(WindStrength windStrength)
        {
            this.windStrength = windStrength;
        }

        /// <summary>
        /// Update wind strength for <paramref name="weather"/>.
        /// Use <see cref="SetWindStrength()"/> to apply.
        /// </summary>
        /// <param name="weather">Current weather.</param>
        private void UpdateWindStrength(WeatherType weather)
        {
            this.windStrength = GetWindStrength(weather);

#if TEST_WIND
            Debug.Log(string.Format("VibrantWind: weather [{0}], {1}", weather, currentStrength));
#endif
        }

        /// <summary>
        /// Gets the wind strength for <paramref name="weather"/>.
        /// </summary>
        /// <param name="weather">New weather.</param>
        private WindStrength GetWindStrength(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Sunny:
                default:
                    return wind.None;

                case WeatherType.Cloudy:
                    return wind.VeryLight;

                case WeatherType.Overcast:
                case WeatherType.Fog:
                    return wind.Light;

                case WeatherType.Rain:
                    return wind.Medium;

                case WeatherType.Thunder:
                    return wind.Strong;

                case WeatherType.Snow:
                    return wind.VeryStrong;
            }
        }

        /// <summary>
        /// Set wind strength to all terrains.
        /// </summary>
        private void SetWindStrength()
        {
            foreach (Terrain terrain in streamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>())
                windStrength.Assign(terrain.terrainData);
        }

        /// <summary>
        /// Set wind strength to <paramref name="terrainData"/>.
        /// </summary>
        private void SetWindStrength(TerrainData terrainData)
        {
            windStrength.Assign(terrainData);
        }

        #endregion

        #region Events Handlers

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
    }
}
