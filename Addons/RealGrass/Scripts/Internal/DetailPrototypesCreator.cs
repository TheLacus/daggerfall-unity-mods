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

namespace RealGrass
{
    /// <summary>
    /// Create detail protoypes taking into account user settings.
    /// </summary>
    public class DetailPrototypesCreator
    {
        #region Fields

        readonly DetailPrototype[] detailPrototype;
        readonly Indices indices = new Indices();

        readonly bool import;
        readonly string texturesPath;

        readonly bool useGrassShader;
        readonly GrassColors grassColors;

        Range<float> grassHeight;

        int currentkey = default(int);

        #endregion

        #region Constants

        // Textures for grass billboards
        const string brownGrass = "tex_BrownGrass";
        const string greenGrass = "tex_GreenGrass";

        // Meshes for grass shader
        const string brownGrassMesh = "BrownGrass";
        const string greenGrassMesh = "GreenGrass";

        // Models for water plants
        const string PlantsTemperate = "PlantsTemperate"; // water plants for temperate
        const string Waterlily = "Waterlily"; // waterlilies for temperate
        const string PlantsSwamp = "PlantsSwamp"; // water plants for swamp
        const string PlantsMountain = "PlantsMountain"; // grass for mountain near water
        const string WaterMountainGrass = "WaterMountainGrass"; // grass for mountain inside water
        const string PlantsDesert = "PlantsDesert"; // grass near water for desert

        // Winter models for water plants
        const string PlantsTemperateWinter = "PlantsTemperateWinter"; // water plants for temperate
        const string PlantsSwampWinter = "PlantsSwampWinter"; // water plants for swamp
        const string PlantsMountainWinter = "PlantsMountainWinter"; // grass for mountain near water

        // Little stones for farms
        const string Stone = "Stone";

        // Flowers
        const string FlowersMountain = "FlowersMountain";
        const string FlowersSwamp = "FlowersSwamp";
        const string FlowersTemperate = "FlowersTemperate";

        struct UpdateType { public const short Summer = 0, Winter = 1, Desert = 2; }

        #endregion

        #region Properties

        /// <summary>
        /// Assets used by the terrain for grass.
        /// </summary>
        public DetailPrototype[] DetailPrototypes
        {
            get { return detailPrototype; }
        }

        /// <summary>
        /// Indices of detail prototypes layers.
        /// </summary>
        public Indices Indices
        {
            get { return indices; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize detail protoypes.
        /// </summary>
        public DetailPrototypesCreator(PrototypesProperties properties)
        {
            Color detailColor = new Color(0.70f, 0.70f, 0.70f);

            // Texture import
            import = properties.import;
            if (import)
                texturesPath = Path.Combine(RealGrass.ResourcesFolder, properties.packName);

            // Create a holder for our grass and plants
            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            int index = 0;

            // Grass settings
            grassHeight = properties.grassHeight;
            grassColors = properties.grassColors;
            useGrassShader = properties.useGrassShader;

            // We use GrassBillboard or Grass rendermode
            var grassPrototypes = new DetailPrototype()
            {
                minWidth = properties.grassWidth.Min,
                maxWidth = properties.grassWidth.Max,
                noiseSpread = properties.noiseSpread,
                renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.GrassBillboard,
                usePrototypeMesh = useGrassShader
            };
            detailPrototypes.Add(grassPrototypes);
            indices.Grass = index;

            if (RealGrass.Instance.WaterPlants)
            {
                // Near-water plants settings
                // Here we use the Grass shader which support meshes, and textures with transparency.
                // This allow us to have more realistic plants which still bend in the wind.
                var waterPlantsNear = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = properties.noiseSpreadPlants,
                    healthyColor = detailColor,
                    dryColor = detailColor,
                    renderMode = DetailRenderMode.Grass
                };
                detailPrototypes.Add(waterPlantsNear);
                indices.WaterPlants = ++index;

                // In-water plants settings
                // We use Grass as above
                var waterPlantsInside = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = properties.noiseSpreadPlants,
                    healthyColor = detailColor,
                    dryColor = detailColor,
                    renderMode = DetailRenderMode.Grass
                };
                detailPrototypes.Add(waterPlantsInside);
                indices.Waterlilies = ++index;
            }

            if (RealGrass.Instance.TerrainStones)
            {
                // Little stones
                // For stones we use VertexLit as we are placing 3d static models.
                var stonesPrototypes = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = properties.noiseSpreadStones,
                    healthyColor = detailColor,
                    dryColor = detailColor,
                    renderMode = DetailRenderMode.VertexLit,
                    prototype = LoadGameObject(Stone)
                };
                detailPrototypes.Add(stonesPrototypes);
                indices.Stones = ++index;
            }

            if (RealGrass.Instance.Flowers)
            {
                // Flowers
                var flowerPrototypes = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = 0.4f,
                    healthyColor = detailColor,
                    dryColor = detailColor,
                    renderMode = DetailRenderMode.Grass,
                };
                detailPrototypes.Add(flowerPrototypes);
                indices.Flowers = ++index;
            }

            detailPrototype = detailPrototypes.ToArray();
        }

        /// <summary>
        /// Load assets for Summer.
        /// </summary>
        public void UpdateClimateSummer(ClimateBases currentClimate)
        {
            SetGrassColor(detailPrototype[indices.Grass]);
            SetGrassSize(detailPrototype[indices.Grass]);

            if (!NeedsUpdate(UpdateType.Summer, currentClimate))
                return;

            switch (currentClimate)
            {
                case ClimateBases.Mountain:

                    // Mountain
                    if (!useGrassShader)
                        detailPrototype[indices.Grass].prototypeTexture = LoadTexture(brownGrass);
                    else
                        detailPrototype[indices.Grass].prototype = LoadGameObject(brownGrassMesh);

                    if (RealGrass.Instance.WaterPlants)
                    {
                        detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsMountain);
                        detailPrototype[indices.Waterlilies].prototype = LoadGameObject(WaterMountainGrass);
                    }

                    if (RealGrass.Instance.Flowers)
                        detailPrototype[indices.Flowers].prototype = LoadGameObject(FlowersMountain);
                    break;

                case ClimateBases.Swamp:

                    // Swamp
                    if (!useGrassShader)
                        detailPrototype[indices.Grass].prototypeTexture = LoadTexture(brownGrass);
                    else
                        detailPrototype[indices.Grass].prototype = LoadGameObject(brownGrassMesh);

                    if (RealGrass.Instance.WaterPlants)
                        detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsSwamp);

                    if (RealGrass.Instance.Flowers)
                        detailPrototype[indices.Flowers].prototype = LoadGameObject(FlowersSwamp);
                    break;

                case ClimateBases.Temperate:

                    // Temperate
                    if (!useGrassShader)
                        detailPrototype[indices.Grass].prototypeTexture = LoadTexture(greenGrass);
                    else
                        detailPrototype[indices.Grass].prototype = LoadGameObject(greenGrassMesh);

                    if (RealGrass.Instance.WaterPlants)
                    {
                        detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsTemperate);
                        detailPrototype[indices.Waterlilies].prototype = LoadGameObject(Waterlily);
                    }

                    if (RealGrass.Instance.Flowers)
                        detailPrototype[indices.Flowers].prototype = LoadGameObject(FlowersTemperate);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Summer)", currentClimate));
                    RealGrass.Instance.ToggleMod(false);
                    break;
            }
        }

        /// <summary>
        /// Load assets for Winter.
        /// </summary>
        public void UpdateClimateWinter(ClimateBases currentClimate)
        {
            if (!NeedsUpdate(UpdateType.Winter, currentClimate))
                return;

            switch (currentClimate)
            {
                case ClimateBases.Mountain:

                    // Mountain
                    detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsMountainWinter);
                    break;

                case ClimateBases.Swamp:

                    // Swamp
                    detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsSwampWinter);
                    break;

                case ClimateBases.Temperate:

                    // Temperate
                    detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsTemperateWinter);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Winter)", currentClimate));
                    RealGrass.Instance.ToggleMod(false);
                    break;
            }
        }

        /// <summary>
        /// Load assets for Desert, which doesn't support seasons.
        /// </summary>
        public void UpdateClimateDesert()
        {
            if (!NeedsUpdate(UpdateType.Desert, ClimateBases.Desert))
                return;

            detailPrototype[1].prototype = LoadGameObject(PlantsDesert);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get texture from disk or from mod.
        /// </summary>
        /// <param name="name">Name of texture.</param>
        private Texture2D LoadTexture(string name)
        {
            Texture2D tex;

            if (!import || !TextureReplacement.ImportTextureFromDisk(texturesPath, name, out tex))
                tex = RealGrass.Mod.GetAsset<Texture2D>(name);

            if (tex != null)
                return tex;

            Debug.LogError("Real Grass: Failed to load texture " + name);
            return null;
        }

        /// <summary>
        /// Get gameobject from mod and customize
        /// texture from disk asset (if present).
        /// </summary>
        /// <param name="name">Name of gameobject.</param>
        private GameObject LoadGameObject(string name)
        {
            var go = RealGrass.Mod.GetAsset<GameObject>(name);

            if (go != null)
            {
                if (import)
                {
                    Texture2D tex;
                    Material material = go.GetComponent<MeshRenderer>().material;

                    if (TextureReplacement.ImportTextureFromDisk(texturesPath, material.mainTexture.name, out tex))
                        material.mainTexture = tex;
                }

                return go;
            }

            Debug.LogError("Real Grass: Failed to load model " + name);
            return null;
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

        /// <summary>
        /// Set grass color according to day of year.
        /// </summary>
        private void SetGrassColor(DetailPrototype detailPrototype)
        {
            const int
                spring = 2 * 30 + 1,
                midSummer = 6 * 30 + 15,
                fall = 10 * 30 + 30;

            int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;

            if (day <= midSummer)
            {
                // Sprint to Summer
                float t = Mathf.InverseLerp(spring, midSummer, day);

                detailPrototype.healthyColor = Color.Lerp(grassColors.springHealthy, grassColors.summerHealty, t);
                detailPrototype.dryColor = Color.Lerp(grassColors.springDry, grassColors.summerDry, t);
            }
            else
            {
                // Summer to Fall
                float t = Mathf.InverseLerp(midSummer, fall, day);

                detailPrototype.healthyColor = Color.Lerp(grassColors.summerHealty, grassColors.fallHealty, t);
                detailPrototype.dryColor = Color.Lerp(grassColors.summerDry, grassColors.fallDry, t);
            }
        }

        /// <summary>
        /// Set grass size according to day of year.
        /// </summary>
        private void SetGrassSize(DetailPrototype detailPrototype)
        {
            // Settings size is size on summer (max size).
            // Height increase on spring and decrease on fall up to this amount(%).
            const int seasonalModifier = 65;

            const int
                spring = 2 * 30 + 1,
                summer = 5 * 30 + 1,
                fall = 8 * 30 + 1,
                winter = 11 * 30 + 1;
            const float minScale = 1 - (float)seasonalModifier / 100;

            int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;

            if (day < summer)
            {
                // Spring
                float t = Mathf.InverseLerp(spring, summer, day);
                float scale = Mathf.SmoothStep(minScale, 1, t);

                detailPrototype.minHeight = grassHeight.Min * scale;
                detailPrototype.maxHeight = grassHeight.Max * scale;
            }
            else if (day < fall)
            {
                // Summer
                detailPrototype.minHeight = grassHeight.Min;
                detailPrototype.maxHeight = grassHeight.Max;
            }
            else
            {
                // Fall
                float t = Mathf.InverseLerp(fall, winter, day);
                float scale = Mathf.SmoothStep(minScale, 1, 1 - t);

                detailPrototype.minHeight = grassHeight.Min * scale;
                detailPrototype.maxHeight = grassHeight.Max * scale;
            }
        }

        #endregion
    }
}
