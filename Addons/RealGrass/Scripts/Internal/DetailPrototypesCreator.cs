// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:    

using UnityEngine;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace RealGrass
{
    /// <summary>
    /// Create detail protoypes taking into account user settings.
    /// </summary>
    public class DetailPrototypesCreator
    {
        // Fields
        readonly bool useGrassShader;
        readonly bool waterPlants;
        readonly DetailPrototype[] detailPrototype;

        #region Constants

        // Textures for grass billboards
        const string brownGrass = "tex_BrownGrass";
        const string greenGrass = "tex_GreenGrass";

        // Meshes for grass shader
        const string brownGrassMesh = "BrownGrass";
        const string greenGrassMesh = "GreenGrass";

        // Models for water plants
        const string TemperateGrass = "TemperateGrass"; // water plants for temperate
        const string Waterlily = "Waterlily"; // waterlilies for temperate
        const string SwampGrass = "SwampGrass"; // water plants for swamp
        const string MountainGrass = "MountainGrass"; // grass for mountain near water
        const string WaterMountainGrass = "WaterMountainGrass"; // grass for mountain inside water
        const string DesertGrass = "DesertGrass"; // grass near water for desert

        // Winter models for water plants
        const string TemperateGrassWinter = "TemperateGrassWinter"; // water plants for temperate
        const string SwampGrassWinter = "SwampGrassWinter"; // water plants for swamp
        const string MountainGrassWinter = "MountainGrassWinter"; // grass for mountain near water

        // Little stones for farms
        const string Stone = "Stone";

        // Flowers
        const string Flowers = "Flowers";

        #endregion

        #region Properties

        /// <summary>
        /// Assets used by the terrain for grass.
        /// </summary>
        public DetailPrototype[] DetailPrototypes
        {
            get { return detailPrototype; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize detail protoypes.
        /// </summary>
        /// <param name="settings">Mod settings.</param>
        public DetailPrototypesCreator(ModSettings settings, bool waterPlants, bool terrainStones, bool flowers)
        {
            this.waterPlants = waterPlants;

            Color detailColor = new Color(0.70f, 0.70f, 0.70f);

            // Grass
            const string grassSection = "Grass";
            Range<float> grassHeight = settings.GetTupleFloat(grassSection, "GrassHeight");
            Range<float> grassWidth = settings.GetTupleFloat(grassSection, "GrassWidth");
            float noiseSpread = settings.GetFloat(grassSection, "NoiseSpread");
            useGrassShader = settings.GetBool(grassSection, "UseGrassShader");
            Color grassHealthyColor = settings.GetColor(grassSection, "HealthyColor");
            Color grassDryColor = settings.GetColor(grassSection, "DryColor");

            // Water plants
            const string waterPlantsSection = "WaterPlants";
            float noiseSpreadPlants = settings.GetFloat(waterPlantsSection, "NoiseSpread");

            // Stones
            float noiseSpreadStones = settings.GetFloat("TerrainStones", "NoiseSpread");

            // Create a holder for our grass and plants
            detailPrototype = new DetailPrototype[5];

            // Grass settings
            // We use GrassBillboard or Grass rendermode
            detailPrototype[0] = new DetailPrototype()
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

            // Near-water plants settings
            // Here we use the Grass shader which support meshes, and textures with transparency.
            // This allow us to have more realistic plants which still bend in the wind.
            detailPrototype[1] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = noiseSpreadPlants,
                healthyColor = detailColor,
                dryColor = detailColor,
                renderMode = DetailRenderMode.Grass
            };

            // In-water plants settings
            // We use Grass as above
            detailPrototype[2] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = noiseSpreadPlants,
                healthyColor = detailColor,
                dryColor = detailColor,
                renderMode = DetailRenderMode.Grass
            };

            // Little stones settings
            // For stones we use VertexLit as we are placing 3d static models.
            detailPrototype[3] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = noiseSpreadStones,
                healthyColor = detailColor,
                dryColor = detailColor,
                renderMode = DetailRenderMode.VertexLit,
                prototype = LoadGameObject(Stone)
            };

            // Flowers
            detailPrototype[4] = new DetailPrototype()
            {
                usePrototypeMesh = true,
                noiseSpread = noiseSpreadStones,
                healthyColor = detailColor,
                dryColor = detailColor,
                renderMode = DetailRenderMode.Grass,
                prototype = LoadGameObject(Flowers)
            };
        }

        /// <summary>
        /// Load assets for Summer.
        /// </summary>
        public void UpdateClimateSummer(int currentClimate)
        {
            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

                    // Mountain
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = LoadTexture(brownGrass);
                    else
                        detailPrototype[0].prototype = LoadGameObject(brownGrassMesh);
                    if (waterPlants)
                    {
                        detailPrototype[1].prototype = LoadGameObject(MountainGrass);
                        detailPrototype[2].prototype = LoadGameObject(WaterMountainGrass);
                    }
                    break;

                case Climate.Swamp:
                case Climate.Swamp2:

                    // Swamp
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = LoadTexture(brownGrass);
                    else
                        detailPrototype[0].prototype = LoadGameObject(brownGrassMesh);
                    if (waterPlants)
                        detailPrototype[1].prototype = LoadGameObject(SwampGrass);
                    break;

                case Climate.Temperate:
                case Climate.Temperate2:

                    // Temperate
                    if (!useGrassShader)
                        detailPrototype[0].prototypeTexture = LoadTexture(greenGrass);
                    else
                        detailPrototype[0].prototype = LoadGameObject(greenGrassMesh);
                    if (waterPlants)
                    {
                        detailPrototype[1].prototype = LoadGameObject(TemperateGrass);
                        detailPrototype[2].prototype = LoadGameObject(Waterlily);
                    }
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Summer)", currentClimate));
                    RealGrassLoader.Mod.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Load assets for Winter.
        /// </summary>
        public void UpdateClimateWinter(int currentClimate)
        {
            switch (currentClimate)
            {
                case Climate.Mountain:
                case Climate.Mountain2:

                    // Mountain
                    detailPrototype[1].prototype = LoadGameObject(MountainGrassWinter);
                    break;

                case Climate.Swamp:
                case Climate.Swamp2:

                    // Swamp
                    detailPrototype[1].prototype = LoadGameObject(SwampGrassWinter);
                    break;

                case Climate.Temperate:
                case Climate.Temperate2:

                    // Temperate
                    detailPrototype[1].prototype = LoadGameObject(TemperateGrassWinter);
                    break;

                default:
                    Debug.LogError(string.Format("RealGrass: {0} is not a valid climate (Winter)", currentClimate));
                    RealGrassLoader.Mod.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Load assets for Desert, which doesn't support seasons.
        /// </summary>
        public void UpdateClimateDesert()
        {
            detailPrototype[1].prototype = LoadGameObject(DesertGrass);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get texture from disk or from mod.
        /// </summary>
        /// <param name="name">Name of texture.</param>
        private static Texture2D LoadTexture(string name)
        {
            Texture2D tex;

            if (!TextureReplacement.ImportTextureFromDisk(RealGrassLoader.ResourcesFolder, name, out tex))
                tex = RealGrassLoader.Mod.GetAsset<Texture2D>(name);

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
        private static GameObject LoadGameObject(string name)
        {
            var go = RealGrassLoader.Mod.GetAsset<GameObject>(name);

            if (go != null)
            {
                Texture2D tex;
                Material material = go.GetComponent<MeshRenderer>().material;

                if (TextureReplacement.ImportTextureFromDisk(RealGrassLoader.ResourcesFolder, material.mainTexture.name, out tex))
                    material.mainTexture = tex;

                return go;
            }

            Debug.LogError("Real Grass: Failed to load model " + name);
            return null;
        }

        #endregion
    }
}
