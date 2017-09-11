// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:    

using System.IO;
using System.Collections.Generic;
using UnityEngine;
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
        public DetailPrototypesCreator()
        {
            var settings = RealGrass.Settings;
            Color detailColor = new Color(0.70f, 0.70f, 0.70f);

            // Texture import
            const string texturesSection = "Textures";
            import = settings.GetBool(texturesSection, "Import");
            if (import)
                texturesPath = Path.Combine(RealGrass.ResourcesFolder, settings.GetString(texturesSection, "Pack"));

            // Create a holder for our grass and plants
            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            int index = 0;

            // Grass settings
            const string grassSection = "Grass";
            Range<float> grassHeight = settings.GetTupleFloat(grassSection, "Height");
            Range<float> grassWidth = settings.GetTupleFloat(grassSection, "Width");
            float noiseSpread = settings.GetFloat(grassSection, "NoiseSpread");
            Color grassHealthyColor = settings.GetColor(grassSection, "HealthyColor");
            Color grassDryColor = settings.GetColor(grassSection, "DryColor");
            useGrassShader = settings.GetBool(grassSection, "UseGrassShader");

            // We use GrassBillboard or Grass rendermode
            var grassPrototypes = new DetailPrototype()
            {
                minHeight = grassHeight.Min,
                maxHeight = grassHeight.Max,
                minWidth = grassWidth.Min,
                maxWidth = grassWidth.Max,
                noiseSpread = noiseSpread,
                healthyColor = grassHealthyColor,
                dryColor = grassDryColor,
                renderMode = useGrassShader ? DetailRenderMode.Grass : DetailRenderMode.GrassBillboard,
                usePrototypeMesh = useGrassShader
            };
            detailPrototypes.Add(grassPrototypes);
            indices.Grass = index;

            if (RealGrass.Instance.WaterPlants)
            {
                const string waterPlantsSection = "WaterPlants";
                float noiseSpreadPlants = settings.GetFloat(waterPlantsSection, "NoiseSpread");

                // Near-water plants settings
                // Here we use the Grass shader which support meshes, and textures with transparency.
                // This allow us to have more realistic plants which still bend in the wind.
                var waterPlantsNear = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = noiseSpreadPlants,
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
                    noiseSpread = noiseSpreadPlants,
                    healthyColor = detailColor,
                    dryColor = detailColor,
                    renderMode = DetailRenderMode.Grass
                };
                detailPrototypes.Add(waterPlantsInside);
                indices.Waterlilies = ++index;
            }

            if (RealGrass.Instance.TerrainStones)
            {
                float noiseSpreadStones = settings.GetFloat("TerrainStones", "NoiseSpread");

                // Little stones
                // For stones we use VertexLit as we are placing 3d static models.
                var stonesPrototypes = new DetailPrototype()
                {
                    usePrototypeMesh = true,
                    noiseSpread = noiseSpreadStones,
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
        public void UpdateClimateSummer(int currentClimate)
        {
            if (!NeedsUpdate(UpdateType.Summer, currentClimate))
                return;

            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

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

                case Climate.Swamp:
                case Climate.Swamp2:

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

                case Climate.Temperate:
                case Climate.Temperate2:

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
        public void UpdateClimateWinter(int currentClimate)
        {
            if (!NeedsUpdate(UpdateType.Winter, currentClimate))
                return;

            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

                    // Mountain
                    detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsMountainWinter);
                    break;

                case Climate.Swamp:
                case Climate.Swamp2:

                    // Swamp
                    detailPrototype[indices.WaterPlants].prototype = LoadGameObject(PlantsSwampWinter);
                    break;

                case Climate.Temperate:
                case Climate.Temperate2:

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
            if (!NeedsUpdate(UpdateType.Desert, Climate.None))
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
        private bool NeedsUpdate(short updateType, int climate)
        {
            int key = (updateType << 16) + (short)climate;

            if (key == currentkey)
                return false;

            currentkey = key;
            return true;
        }

        #endregion
    }
}
