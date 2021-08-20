// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:    

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;

namespace RealGrass
{
    #region Structs

    public struct GrassColors
    {
        public Color
            SpringHealthy,
            SpringDry,
            SummerHealty,
            SummerDry,
            FallHealty,
            FallDry;

        public bool SeasonInterpolation;
    }

    public struct PrototypesProperties
    {
        public Range<float> GrassHeight;
        public Range<float> GrassWidth;
        public float NoiseSpread;
        public GrassColors GrassColors;
        public bool UseGrassShader;
        public float NoiseSpreadPlants;
        public bool TextureOverride;
    }

    public struct GrassDetail
    {
        public string Name;
        public float WidthModifier;
        public float HeightModifier;
    }

    #endregion

    /// <summary>
    /// Manages terrain detail prototypes.
    /// </summary>
    public class DetailPrototypesManager
    {
        private class SizeColor
        {
            private readonly GrassColors grassColors;
            private readonly Range<float> grassHeight;
            private readonly float noiseSpread;

            protected DetailPrototype Grass { get; }
            protected DetailPrototype GrassDetail { get; }
            protected DetailPrototype GrassAccent { get; }

            internal SizeColor(GrassColors grassColors, Range<float> grassHeight, float noiseSpread, DetailPrototype grass, DetailPrototype detail, DetailPrototype accent)
            {
                this.grassColors = grassColors;
                this.grassHeight = grassHeight;
                this.noiseSpread = noiseSpread;
                this.Grass = grass;
                this.GrassDetail = detail;
                this.GrassAccent = accent;
            }    

            internal void Refresh(int detail, int accent)
            {
                SetGrassColor(grassColors);
                SetGrassSize(grassHeight, detail, accent);
                Grass.noiseSpread = noiseSpread;
            }

            internal void RefreshDesert()
            {
                Grass.healthyColor = Color.white;
                Grass.dryColor = new Color(0.89f, 0.67f, 0.67f);
                Grass.minHeight = grassHeight.Min;
                Grass.maxHeight = grassHeight.Max;
                Grass.noiseSpread = 0.8f;
            }

            protected virtual void SetGrassColor(GrassColors grassColors)
            {
                DaggerfallDateTime daggerfallDateTime = DaggerfallUnity.Instance.WorldTime.Now;
                switch (daggerfallDateTime.SeasonValue)
                {
                    case DaggerfallDateTime.Seasons.Spring:
                        Grass.healthyColor = grassColors.SpringHealthy;
                        Grass.dryColor = grassColors.SpringDry;
                        break;
                    case DaggerfallDateTime.Seasons.Summer:
                        Grass.healthyColor = grassColors.SummerHealty;
                        Grass.dryColor = grassColors.SummerDry;
                        break;
                    case DaggerfallDateTime.Seasons.Fall:
                        Grass.healthyColor = grassColors.FallHealty;
                        Grass.dryColor = grassColors.FallDry;
                        break;
                    default:
                        Grass.healthyColor = Color.white;
                        Grass.dryColor = Color.white;
                        break;
                }
            }

            protected virtual void SetGrassSize(Range<float> grassHeight, int detail, int accent)
            {
                Grass.minHeight = grassHeight.Min;
                Grass.maxHeight = grassHeight.Max;

                if ((RealGrass.Instance.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed)
                    SetDetailGrassSize(detail, accent);
            }

            protected void SetDetailGrassSize(int detail, int accent)
            {
                ScaleGrassDetail(Grass, GrassDetail, grassDetails[detail]);
                ScaleGrassDetail(Grass, GrassAccent, grassAccents[accent]);
            }
        }

        private class InterpolatedSizeColor : SizeColor
        {
            internal InterpolatedSizeColor(GrassColors grassColors, Range<float> grassHeight, float noiseSpread, DetailPrototype grass, DetailPrototype detail, DetailPrototype accent)
                : base(grassColors, grassHeight, noiseSpread, grass, detail, accent)
            {
            }

            protected override void SetGrassColor(GrassColors grassColors)
            {
                DaggerfallDateTime daggerfallDateTime = DaggerfallUnity.Instance.WorldTime.Now;
                int day = daggerfallDateTime.DayOfYear;
                if (day < DaysOfYear.Spring)
                {
                    float t = Mathf.InverseLerp(DaysOfYear.GrowDay, DaysOfYear.Spring, day);
                    Grass.healthyColor = Color.Lerp(Color.white, grassColors.SpringHealthy, t);
                    Grass.dryColor = Color.Lerp(Color.white, grassColors.SpringDry, t);
                }
                else if (day <= DaysOfYear.MidYear)
                {
                    float t = Mathf.InverseLerp(DaysOfYear.Spring, DaysOfYear.MidYear, day);
                    Grass.healthyColor = Color.Lerp(grassColors.SpringHealthy, grassColors.SummerHealty, t);
                    Grass.dryColor = Color.Lerp(grassColors.SpringDry, grassColors.SummerDry, t);
                }
                else if (day < DaysOfYear.Winter)
                {
                    float t = Mathf.InverseLerp(DaysOfYear.MidYear, DaysOfYear.Winter, day);
                    Grass.healthyColor = Color.Lerp(grassColors.SummerHealty, grassColors.FallHealty, t);
                    Grass.dryColor = Color.Lerp(grassColors.SummerDry, grassColors.FallDry, t);
                }
                else
                {
                    float t = Mathf.InverseLerp(DaysOfYear.Winter, DaysOfYear.DieDay, day);
                    Grass.healthyColor = Color.Lerp(grassColors.FallHealty, Color.white, t);
                    Grass.dryColor = Color.Lerp(grassColors.FallDry, Color.white, t);
                }
            }

            protected override void SetGrassSize(Range<float> grassHeight, int detail, int accent)
            {
                const int seasonalModifier = 65;
                const float minScale = 1 - (float)seasonalModifier / 100;

                int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;

                if (day < DaysOfYear.Spring)
                {
                    Grass.minHeight = grassHeight.Min * minScale;
                    Grass.maxHeight = grassHeight.Max * minScale;
                }
                else if (day < DaysOfYear.Summer)
                {
                    float t = Mathf.InverseLerp(DaysOfYear.Spring, DaysOfYear.Summer, day);
                    float scale = Mathf.SmoothStep(minScale, 1, t);

                    Grass.minHeight = grassHeight.Min * scale;
                    Grass.maxHeight = grassHeight.Max * scale;
                }
                else if (day < DaysOfYear.Fall)
                {
                    Grass.minHeight = grassHeight.Min;
                    Grass.maxHeight = grassHeight.Max;
                }
                else if (day < DaysOfYear.Winter)
                {
                    float t = Mathf.InverseLerp(DaysOfYear.Fall, DaysOfYear.Winter, day);
                    float scale = Mathf.SmoothStep(minScale, 1, 1 - t);

                    Grass.minHeight = grassHeight.Min * scale;
                    Grass.maxHeight = grassHeight.Max * scale;
                }
                else
                {
                    Grass.minHeight = grassHeight.Min * minScale;
                    Grass.maxHeight = grassHeight.Max * minScale;
                }

                if ((RealGrass.Instance.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed)
                    SetDetailGrassSize(detail, accent);
            }
        }

        #region Constants

        const string realisticGrass = "Grass";
        const string brownGrass = "BrownGrass";
        const string greenGrass = "GreenGrass";
        const string desertGrass = "DesertGrass";
        const string plantsTemperate = "PlantsTemperate";
        const string plantsSwamp = "PlantsSwamp";
        const string plantsMountain = "PlantsMountain";
        const string plantsDesert = "PlantsDesert";
        const string plantsTemperateWinter = "PlantsTemperateWinter";
        const string plantsSwampWinter = "PlantsSwampWinter";
        const string rock = "Rock";
        const string rockWinter = "RockWinter";

        struct UpdateType { public const short Summer = 0, Winter = 1, Desert = 2; }

        #endregion

        #region Fields

        static GameObject grassDetailPrefab;
        static GameObject grassAccentPrefab;

        static readonly GrassDetail[] grassDetails = new GrassDetail[]
        {
            new GrassDetail()
            {
                Name = "GrassDetails_01",
                WidthModifier = 1.0f,
                HeightModifier = 2.0f
            },
            new GrassDetail()
            {
                Name = "GrassDetails_02",
                WidthModifier = 1.0f,
                HeightModifier = 2.0f
            },
            new GrassDetail()
            {
                Name = "GrassDetails_04",
                WidthModifier = 1.0f,
                HeightModifier = 2.0f
            },
            new GrassDetail()
            {
                Name = "GrassDetails_05",
                WidthModifier = 1.0f,
                HeightModifier = 2.0f
            }
        };

        static readonly GrassDetail[] grassAccents = new GrassDetail[]
        {
            new GrassDetail()
            {
                Name = "GrassDetails_03",
                WidthModifier = 0.65f,
                HeightModifier = 0.65f
            },
            new GrassDetail()
            {
                Name = "GrassDetails_06",
                WidthModifier = 0.65f,
                HeightModifier = 0.65f
            }
        };

        private readonly SizeColor sizeColor;
        private readonly bool useGrassShader;
        private readonly bool textureOverride;
        private int currentGrassDetail;
        private int currentGrassAccent;
        int currentkey = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Detail prototypes used by the terrain.
        /// </summary>
        public DetailPrototype[] DetailPrototypes { get; }

        public int Grass { get; private set; }
        public int GrassDetails { get; private set; }
        public int GrassAccents { get; private set; }
        public int WaterPlants { get; private set; }
        public int WaterPlantsAlt { get; private set; }
        public int Rocks { get; private set; }

        #endregion

        #region Constructor

        public DetailPrototypesManager(PrototypesProperties properties)
        {
            Color healthyColor = new Color(0.70f, 0.70f, 0.70f);
            Color dryColor = new Color(0.40f, 0.40f, 0.40f);

            textureOverride = properties.TextureOverride;
            float noiseSpread = properties.NoiseSpread;
            useGrassShader = properties.UseGrassShader;

            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            int index = 0;

            var grassPrototypes = new DetailPrototype()
            {
                minWidth = properties.GrassWidth.Min,
                maxWidth = properties.GrassWidth.Max,
                noiseSpread = properties.NoiseSpread,
                renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.GrassBillboard,
                usePrototypeMesh = useGrassShader
            };
            detailPrototypes.Add(grassPrototypes);
            Grass = index;

            if ((RealGrass.Instance.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed)
            {
                detailPrototypes.Add(new DetailPrototype()
                {
                    minWidth = properties.GrassWidth.Min,
                    maxWidth = properties.GrassWidth.Max,
                    noiseSpread = properties.NoiseSpread,
                    healthyColor = healthyColor,
                    dryColor = dryColor,
                    renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.GrassBillboard,
                    usePrototypeMesh = useGrassShader
                });
                GrassDetails = ++index;

                detailPrototypes.Add(new DetailPrototype()
                {
                    minWidth = properties.GrassWidth.Min,
                    maxWidth = properties.GrassWidth.Max,
                    noiseSpread = properties.NoiseSpread,
                    healthyColor = healthyColor,
                    dryColor = dryColor,
                    renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.GrassBillboard,
                    usePrototypeMesh = useGrassShader
                });
                GrassAccents = ++index;
            }

            if (RealGrass.Instance.WaterPlants)
            {
                var waterPlantsNear = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = properties.NoiseSpreadPlants,
                    healthyColor = healthyColor,
                    dryColor = dryColor,
                    renderMode = DetailRenderMode.Grass
                };
                detailPrototypes.Add(waterPlantsNear);
                WaterPlants = ++index;
            }

            if (RealGrass.Instance.TerrainStones)
            {
                detailPrototypes.Add(new DetailPrototype()
                {
                    minWidth = 0.4f,
                    maxWidth = 1,
                    minHeight = 0.25f,
                    maxHeight = 1.5f,
                    usePrototypeMesh = true,
                    noiseSpread = 1,
                    renderMode = DetailRenderMode.VertexLit
                });
                Rocks = ++index;
            }

            DetailPrototypes = detailPrototypes.ToArray();

            GrassColors grassColors = properties.GrassColors;
            Range<float> grassHeight = properties.GrassHeight;
            sizeColor = grassColors.SeasonInterpolation ?
                new InterpolatedSizeColor(grassColors, grassHeight, noiseSpread, detailPrototypes[Grass], detailPrototypes[GrassDetails], detailPrototypes[GrassAccents]) :
                new SizeColor(grassColors, grassHeight, noiseSpread, detailPrototypes[Grass], detailPrototypes[GrassDetails], detailPrototypes[GrassAccents]);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load assets for Summer.
        /// </summary>
        public void UpdateClimateSummer(ClimateBases currentClimate)
        {
            RefreshGrassDetails();
            sizeColor.Refresh(currentGrassDetail, currentGrassAccent);

            if ((RealGrass.Instance.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed)
            {
                SetGrassDetail(GrassDetails, grassDetails[currentGrassDetail], ref grassDetailPrefab);
                SetGrassDetail(GrassAccents, grassAccents[currentGrassAccent], ref grassAccentPrefab);
            }

            if (!NeedsUpdate(UpdateType.Summer, currentClimate))
                return;

            switch (currentClimate)
            {
                case ClimateBases.Mountain:

                    // Mountain
                    SetGrass(brownGrass, realisticGrass);

                    if (RealGrass.Instance.WaterPlants)

                        DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsMountain);
                    break;

                case ClimateBases.Swamp:

                    // Swamp
                    SetGrass(brownGrass, realisticGrass);

                    if (RealGrass.Instance.WaterPlants)
                        DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsSwamp);
                    break;

                case ClimateBases.Temperate:

                    // Temperate
                    SetGrass(greenGrass, realisticGrass);

                    if (RealGrass.Instance.WaterPlants)
                        DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsTemperate);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Summer)", currentClimate));
                    RealGrass.Instance.ToggleMod(false);
                    break;
            }

            if (RealGrass.Instance.TerrainStones)
            {
                DetailPrototypes[Rocks].prototype = LoadGameObject(rock);
                DetailPrototypes[Rocks].healthyColor = new Color(0.70f, 0.70f, 0.70f);
                DetailPrototypes[Rocks].dryColor = new Color(0.40f, 0.40f, 0.40f);
            }
        }

        /// <summary>
        /// Load assets for Winter.
        /// </summary>
        public void UpdateClimateWinter(ClimateBases currentClimate)
        {
            bool drawGrass = IsGrassTransitioning();
            if (drawGrass)
                sizeColor.Refresh(currentGrassDetail, currentGrassAccent);

            if (!NeedsUpdate(UpdateType.Winter, currentClimate))
                return;

            switch (currentClimate)
            {
                case ClimateBases.Mountain:
                    if (drawGrass)
                        SetGrass(brownGrass, realisticGrass);
                    break;

                case ClimateBases.Swamp:
                    if (drawGrass)
                        SetGrass(brownGrass, realisticGrass);

                    if (RealGrass.Instance.WaterPlants)
                        DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsSwampWinter);
                    break;

                case ClimateBases.Temperate:
                    if (drawGrass)
                        SetGrass(greenGrass, realisticGrass);

                    if (RealGrass.Instance.WaterPlants)
                        DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsTemperateWinter);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Winter)", currentClimate));
                    RealGrass.Instance.ToggleMod(false);
                    break;
            }

            if (RealGrass.Instance.TerrainStones)
            {
                DetailPrototypes[Rocks].prototype = LoadGameObject(rockWinter);
                DetailPrototypes[Rocks].healthyColor = new Color(0.70f, 0.70f, 0.70f);
                DetailPrototypes[Rocks].dryColor = new Color(0.40f, 0.40f, 0.40f);
            }
        }

        /// <summary>
        /// Load assets for Desert, which doesn't support seasons.
        /// </summary>
        public void UpdateClimateDesert()
        {
            if (!NeedsUpdate(UpdateType.Desert, ClimateBases.Desert))
                return;

            SetGrass(desertGrass, desertGrass);
            sizeColor.RefreshDesert();
            DetailPrototype detailPrototype = DetailPrototypes[Grass];
            detailPrototype.healthyColor = Color.white;
            detailPrototype.dryColor = new Color(0.89f, 0.67f, 0.67f);
            detailPrototype.noiseSpread = 0.8f;

            if (RealGrass.Instance.WaterPlants)
                DetailPrototypes[WaterPlants].prototype = LoadGameObject(plantsDesert);

            if (RealGrass.Instance.TerrainStones)
            {
                DetailPrototypes[Rocks].prototype = LoadGameObject(rock);
                DetailPrototypes[Rocks].healthyColor = Color.white;
                DetailPrototypes[Rocks].dryColor = new Color(0.85f, 0.85f, 0.85f);
            }
        }

        #endregion

        #region Private Methods

        private void SetGrass(string classic, string realistic)
        {
            string assetName = (RealGrass.Instance.GrassStyle & GrassStyle.Full) == GrassStyle.Full ? realistic : classic;

            if (!useGrassShader)
                DetailPrototypes[Grass].prototypeTexture = LoadTexture(assetName + "_tex");
            else
                DetailPrototypes[Grass].prototype = LoadGameObject(assetName);
        }

        private void SetGrassDetail(int layer, GrassDetail grassDetail, ref GameObject prefab)
        {
            Texture2D tex = LoadTexture(grassDetail.Name);

            if (!useGrassShader)
            {
                DetailPrototypes[layer].prototypeTexture = tex;
            }
            else
            {
                if (!prefab)
                {
                    prefab = GameObject.Instantiate(LoadGameObject("GrassDetails"), RealGrass.Instance.transform);
                    prefab.SetActive(false);
                }

                GameObject go = prefab;
                go.GetComponent<Renderer>().material.mainTexture = tex;
                DetailPrototypes[layer].prototype = go;
            }
        }

        /// <summary>
        /// Import texture from loose files or from mod.
        /// </summary>
        /// <param name="name">Name of texture.</param>
        private Texture2D LoadTexture(string name)
        {
            Texture2D tex;
            if (TryImportTextureFromLooseFiles(name, out tex))
                return tex;

            return RealGrass.Mod.GetAsset<Texture2D>(name);
        }

        /// <summary>
        /// Import gameobject from mod and override material with texture from loose files.
        /// </summary>
        /// <param name="name">Name of gameobject.</param>
        private GameObject LoadGameObject(string name)
        {
            GameObject go = RealGrass.Mod.GetAsset<GameObject>(name);

            Texture2D tex;
            if (TryImportTextureFromLooseFiles(name, out tex))
                go.GetComponent<MeshRenderer>().material.mainTexture = tex;

            return go;
        }

        private bool TryImportTextureFromLooseFiles(string name, out Texture2D tex)
        {
            if (textureOverride)
                return TextureReplacement.TryImportTextureFromLooseFiles(Path.Combine(RealGrass.TexturesFolder, name), true, false, false, out tex);

            tex = null;
            return false;
        }

        /// <summary>
        /// True if season or climate changed and detail prototypes should be updated.
        /// </summary>
        private bool NeedsUpdate(short updateType, ClimateBases climate)
        {
            int key = (updateType << 16) + (short)climate;

            if (key == currentkey)
                return false;

            currentkey = key;
            return true;
        }

        private bool IsGrassTransitioning()
        {
            int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;
            return day > DaysOfYear.GrowDay || day < DaysOfYear.DieDay;
        }

        private void RefreshGrassDetails()
        {
            currentGrassDetail = Random.Range(0, grassDetails.Length);
            currentGrassAccent = Random.Range(0, grassAccents.Length);
        }

        private static void ScaleGrassDetail(DetailPrototype reference, DetailPrototype prototype, GrassDetail detail)
        {
            prototype.minHeight = reference.minHeight * detail.HeightModifier;
            prototype.maxHeight = reference.maxHeight * detail.HeightModifier;
            prototype.minWidth = reference.minWidth * detail.WidthModifier;
            prototype.maxWidth = reference.maxWidth * detail.WidthModifier;
        }

        #endregion
    }
}
