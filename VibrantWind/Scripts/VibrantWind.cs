// Project:         Vibrant Wind for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

// #define TEST_PERFORMANCE

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VibrantWind
{
    public class WindManager
    {
        private readonly StreamingWorld streamingWorld;
        private readonly WindZone windZone;

        private int weather = -1;

        /// <summary>
        /// Wind settings for terrain wind (grass).
        /// </summary>
        public IReadOnlyList<WindStrength> TerrainWindValues { get; set; }

        /// <summary>
        /// Wind settings for WindZone (trees, particles).
        /// </summary>
        public IReadOnlyList<float> AmbientWindValues { get; set; }

        /// <summary>
        /// Current terrain wind strength.
        /// </summary>
        public WindStrength TerrainWindStrength
        {
            get { return TerrainWindValues[weather]; }
        }

        /// <summary>
        /// Current ambient wind strength.
        /// </summary>
        public float AmbientWindStrength
        {
            get { return AmbientWindValues[weather]; }
        }

        public WeatherType Weather
        {
            get => (WeatherType)weather;
            set => SetWeather(value);
        }

        public WindManager(StreamingWorld streamingWorld, WindZone windZone)
        {
            this.streamingWorld = streamingWorld ?? throw new ArgumentNullException(nameof(streamingWorld));
            this.windZone = windZone ?? throw new ArgumentNullException(nameof(windZone));
        }

        /// <summary>
        /// Set wind strength to <paramref name="terrainData"/>.
        /// </summary>
        public void ApplyTerrainWind(TerrainData terrainData)
        {
            TerrainWindValues[weather].Assign(terrainData);
        }

        private void SetWeather(WeatherType weatherType)
        {
#if TEST_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

            weather = (int)weatherType;

            if (windZone)
                windZone.windMain = AmbientWindValues[weather];

            if (!streamingWorld.IsInit)
            {
                foreach (Terrain terrain in streamingWorld.StreamingTarget.GetComponentsInChildren<Terrain>())
                    ApplyTerrainWind(terrain.terrainData);
            }

#if TEST_PERFORMANCE
            stopwatch.Stop();
            Debug.Log($"VibrantWind: weather changed, elapsed {stopwatch.Elapsed.TotalMilliseconds} ms");
#endif
        }
    }

    /// <summary>
    /// Change wind strength according to current weather.
    /// </summary>
    public class VibrantWind : MonoBehaviour
    {
        private static Mod mod;
        private PlayerWeather playerWeather;
        private bool isEnabled = false;

        public WindManager WindManager { get; private set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(nameof(VibrantWind));
            go.AddComponent<VibrantWind>();
        }

        void Awake()
        {
            VibrantWindConsoleCommands.RegisterCommands(this);
            mod.LoadSettingsCallback = LoadSettings;
        }

        void Start()
        {
            GameManager gameManager = GameManager.Instance;
            WeatherManager weatherManager = gameManager.WeatherManager;

            playerWeather = weatherManager.PlayerWeather;
            WindManager = new WindManager(gameManager.StreamingWorld, weatherManager.GetComponent<WindZone>());

            var modMessages = new VibrantWindModMessages(WindManager);
            mod.MessageReceiver = modMessages.MessageReceiver;

            mod.LoadSettings();
            ToggleMod(true, false);
            mod.IsReady = true;
        }

        private void OnDestroy()
        {
            ToggleMod(false);
        }

        public override string ToString()
        {
            return mod != null ? $"{mod.Title} v.{mod.ModInfo.ModVersion} (running: {isEnabled})." : base.ToString();
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
                    WindManager.Weather = playerWeather.WeatherType;
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
        /// Create wind values with user settings.
        /// </summary>
        private void LoadSettings(ModSettings settings, ModSettingsChange change)
        {
#if TEST_PERFORMANCE
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif

            int stepCount = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().Distinct().Count();

            IReadOnlyList<float> speed = GetPropertyValues(stepCount, settings, "Speed");
            IReadOnlyList<float> bending = GetPropertyValues(stepCount, settings, "Bending");
            IReadOnlyList<float> size = GetPropertyValues(stepCount, settings, "Size");
            IReadOnlyList<float> force = GetPropertyValues(stepCount, settings, "Force");

            var terrainWindValues = new WindStrength[stepCount];
            for (int i = 0; i < stepCount; i++)
                terrainWindValues[i] = new WindStrength(speed[i], bending[i], size[i]);

            WindManager.TerrainWindValues = terrainWindValues;
            WindManager.AmbientWindValues = force.ToArray();

#if TEST_PERFORMANCE
            stopwatch.Stop();
            Debug.Log($"VibrantWind: setup, elapsed {stopwatch.Elapsed.TotalMilliseconds} ms");
#endif

            WindManager.Weather = playerWeather.WeatherType;
        }

        private static WindPropertyValues GetPropertyValues(int stepCount, ModSettings settings, string section)
        {
            return new WindPropertyValues(
                stepCount,
                settings.GetValue<DaggerfallWorkshop.Utility.Tuple<float, float>>(section, "Range").ToValueTuple(),
                InterpolationTypes.Make((InterpolationType)settings.GetValue<int>(section, "Interpolation")));
        }

        /// <summary>
        /// Update weather on terrain creation.
        /// This is called at startup and when player teleports.
        /// </summary>
        private void StreamingWorld_OnInitWorld()
        {
            WindManager.Weather = playerWeather.WeatherType;
        }

        /// <summary>
        /// Apply wind strength on terrain promotion.
        /// This is called for every terrain.
        /// </summary>
        /// <param name="daggerTerrain"></param>
        /// <param name="terrainData">New terrain.</param>
        private void DaggerfallTerrain_OnPromoteTerrainData(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            WindManager.ApplyTerrainWind(terrainData);
        }

        /// <summary>
        /// Update and apply wind strength on new weather.
        /// </summary>
        /// <param name="weatherType">Current weather.</param>
        private void WeatherManager_OnWeatherChange(WeatherType weatherType)
        {
            WindManager.Weather = weatherType;
        }
    }
}
