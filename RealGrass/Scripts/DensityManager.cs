// Project:         Real Grass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:    

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using UnityEngine;

namespace RealGrass
{
    public class Density
    {
        public Range<int> GrassThick;
        public Range<int> GrassThin;
        public Range<int> WaterPlants;
        public Range<int> DesertPlants;
        public int Rocks;
    }

    internal class RealGrassParticles
    {
        private readonly Mod mod;

        private GameObject prefab;

        internal RealGrassParticles(Mod mod)
        {
            this.mod = mod;
        }

        /// <summary>
        /// Instantiate particle system insects at the given position.
        /// </summary>
        internal void DoInsects(Vector3 position)
        {
            if (!prefab)
                prefab = mod.GetAsset<GameObject>("Fireflies");

            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
            GameManager.Instance.StreamingWorld.TrackLooseObject(go, false, -1, -1, true);
        }
    }

    /// <summary>
    /// Manage terrain layers density.
    /// </summary>
    public class DensityManager
    {
        #region Fields

        const int tilemapSize = 128;

        private readonly bool realisticGrass;
        private readonly bool waterPlants;
        private readonly bool terrainStones;
        private readonly bool particles;
        private readonly Density density;
        private readonly RealGrassParticles particlesManager;

        int rocksDensity;

        #endregion

        #region Properties

        // Empty layer
        public static int[,] Empty { get { return EmptyMap(); } }

        // Layers maps
        public int[,] Grass { get; private set; }
        public int[,] GrassDetails { get; private set; }
        public int[,] GrassAccents { get; private set; }
        public int[,] WaterPlants { get; private set; }
        public int[,] Rocks { get; private set; }

        #endregion

        #region Constructor

        public DensityManager(Mod mod, RealGrassOptions options, Density density)
        {
            this.realisticGrass = (options.GrassStyle & GrassStyle.Mixed) == GrassStyle.Mixed;
            this.waterPlants = options.WaterPlants;
            this.terrainStones = options.TerrainStones;
            this.particles = options.FlyingInsects;
            this.density = density;
            this.particlesManager = new RealGrassParticles(mod);
        }

        #endregion

        #region Public Methods

        public void InitDetailsLayers()
        {
            Grass = EmptyMap();
            GrassDetails = EmptyMap(realisticGrass);
            GrassAccents = EmptyMap(realisticGrass);
            WaterPlants = EmptyMap(waterPlants);
            Rocks = EmptyMap(terrainStones);
            rocksDensity = GetTerrainDensityPerennial(density.Rocks);
        }

        /// <summary>
        /// Set density for Summer.
        /// </summary>
        public void SetDensitySummer(Terrain terrain, Color32[] tilemap, ClimateBases currentClimate)
        {
            float seasonalDetailsChance = GetGrassDetailsChance();
            bool isNight = DaggerfallUnity.Instance.WorldTime.Now.IsNight;

            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            if (terrainStones)
                            {
                                int rocks = RandomRocks();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;

                        // Four corner tiles
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            SetGrassDensity(y * 2, x * 2, RandomThick(), seasonalDetailsChance);
                            SetGrassDensity(y * 2, (x * 2) + 1, RandomThick(), seasonalDetailsChance);
                            SetGrassDensity((y * 2) + 1, x * 2, RandomThick(), seasonalDetailsChance);
                            SetGrassDensity((y * 2) + 1, (x * 2) + 1, RandomThick(), seasonalDetailsChance);
                            if (terrainStones)
                            {
                                int rocks = RandomRocks();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;

                        // Upper left corner 
                        case 40:
                        case 224:
                        case 164:
                        case 176:
                        case 181:
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // Lower left corner 
                        case 41:
                        case 221:
                        case 165:
                        case 177:
                        case 182:
                            Grass[y * 2, x * 2] = RandomThin();
                            break;

                        // Lower right corner 
                        case 42:
                        case 222:
                        case 166:
                        case 178:
                        case 183:
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Upper right corner 
                        case 43:
                        case 223:
                        case 167:
                        case 179:
                        case 180:
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // Left side
                        case 44:
                        case 66:
                        case 160:
                        case 168:
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[y * 2, x * 2] = RandomThin();
                            break;

                        // Left side: grass and plants
                        case 84:
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[y * 2, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones && Random.value < 0.3f)
                            {
                                Rocks[(y * 2) + 1, x * 2] = Random.Range(0, 4);
                                Rocks[y * 2, x * 2] = Random.Range(0, 4);
                            }
                            break;

                        // Lower side
                        case 45:
                        case 67:
                        case 161:
                        case 169:
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[y * 2, x * 2] = RandomThin();
                            break;

                        // Lower side: grass and plants
                        case 85:
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[y * 2, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[y * 2, (x * 2)] = RandomWaterPlants();
                            }
                            if (terrainStones && Random.value < 0.3f)
                            {
                                Rocks[y * 2, (x * 2) + 1] = Random.Range(0, 4);
                                Rocks[y * 2, (x * 2)] = Random.Range(0, 4);
                            }
                            break;

                        // Right side
                        case 46:
                        case 64:
                        case 162:
                        case 170:
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Right side: grass and plants
                        case 86:
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            }
                            if (terrainStones && Random.value < 0.3f)
                            {
                                Rocks[(y * 2) + 1, (x * 2) + 1] = Random.Range(0, 4);
                                Rocks[y * 2, (x * 2) + 1] = Random.Range(0, 4);
                            }
                            break;

                        // Upper side
                        case 47:
                        case 65:
                        case 163:
                        case 171:
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // Upper side: grass and plants
                        case 87:
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones && Random.value < 0.3f)
                            {
                                Rocks[(y * 2) + 1, (x * 2) + 1] = Random.Range(0, 4);
                                Rocks[(y * 2) + 1, x * 2] = Random.Range(0, 4);
                            }
                            break;

                        // All expect lower right
                        case 48:
                        case 62:
                        case 156:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect lower right: grass and plants
                        case 88:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            break;

                        // All expect upper right
                        case 49:
                        case 63:
                        case 157:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            break;

                        // All expect upper right: grass and plants
                        case 89:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            break;


                        // All expect upper left
                        case 50:
                        case 60:
                        case 158:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect upper left: grass and plants
                        case 90:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            break;

                        // All expect lower left
                        case 51:
                        case 61:
                        case 159:
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // All expect lower left: grass and plants
                        case 91:
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            break;

                        // Left to right
                        case 204:
                        case 206:
                        case 214:
                            Grass[y * 2, x * 2] = RandomThin();
                            Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            break;

                        // Right to left
                        case 205:
                        case 207:
                        case 213:
                            Grass[(y * 2) + 1, x * 2] = RandomThin();
                            Grass[y * 2, (x * 2) + 1] = RandomThin();
                            break;

                        // Swamp upper right corner
                        case 81:
                            if (waterPlants)
                                WaterPlants[(y * 2), (x * 2)] = RandomWaterPlants();
                            break;

                        // Swamp lower left corner
                        case 83:
                            if (waterPlants)
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            break;

                        // In-water grass
                        // case 0 is not enabled because is used for the sea
                        case 1:
                        case 2:
                        case 3:
                            if (waterPlants && Random.value < 0.6f)
                            {
                                switch (currentClimate)
                                {
                                    case ClimateBases.Mountain:
                                        WaterPlants[(y * 2) + 1, (x * 2) + 1] = Random.Range(5, 10);
                                        break;

                                    case ClimateBases.Swamp:
                                        WaterPlants[(y * 2) + 1, (x * 2) + 1] = Random.Range(0, 3);
                                        WaterPlants[(y * 2), (x * 2) + 1] = Random.Range(0, 3);
                                        WaterPlants[(y * 2) + 1, (x * 2)] = Random.Range(0, 3);
                                        WaterPlants[(y * 2), (x * 2)] = Random.Range(0, 3);
                                        break;

                                    case ClimateBases.Temperate:
                                        WaterPlants[y * 2, x * 2] = Random.Range(2, 5);
                                        WaterPlants[(y * 2) + 1, (x * 2) + 1] = Random.Range(2, 5);
                                        WaterPlants[(y * 2) + 1, x * 2] = Random.Range(2, 5);
                                        WaterPlants[y * 2, (x * 2) + 1] = Random.Range(2, 5);
                                        break;
                                }
                            }

                            if (particles && isNight && Random.value < 0.2f)
                                particlesManager.DoInsects(GetTileWorldPosition(terrain, x, y));
                            break;

                        case 116:
                        case 117:
                        case 118:
                        case 119:
                            if (waterPlants && currentClimate == ClimateBases.Swamp)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = Random.Range(0, 2);
                                WaterPlants[(y * 2), (x * 2) + 1] = Random.Range(0, 2);
                                WaterPlants[(y * 2) + 1, (x * 2)] = Random.Range(0, 2);
                                WaterPlants[(y * 2), (x * 2)] = Random.Range(0, 2);
                            }
                            break;

                        // Little stones
                        case 216:
                        case 217:
                        case 218:
                        case 219:
                            if (terrainStones && Random.value > 0.8f)
                            {
                                Rocks[y * 2, x * 2] = Random.Range(1, 3);
                                Rocks[(y * 2) + 1, (x * 2) + 1] = Random.Range(1, 3);
                            }
                            break;
                    }

                    if (particles && isNight && Random.value < 0.001f)
                        particlesManager.DoInsects(GetTileWorldPosition(terrain, x, y));
                }
            }
        }

        /// <summary>
        /// Set density for Winter.
        /// </summary>
        public void SetDensityWinter(Terrain terrain, Color32[] tilemap, ClimateBases currentClimate)
        {
            float grassChance = GetWinterGrassChance();
            bool shores = currentClimate != ClimateBases.Mountain;

            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            if (terrainStones)
                            {
                                int rocks = RandomRocks();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;

                        // Grass growing/dying
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            if (grassChance != 0)
                            {
                                if (Random.value < grassChance)
                                {
                                    Grass[y * 2, x * 2] = RandomThick();
                                    Grass[y * 2, (x * 2) + 1] = RandomThick();
                                }
                                if (Random.value < grassChance)
                                {
                                    Grass[(y * 2) + 1, x * 2] = RandomThick();
                                    Grass[(y * 2) + 1, (x * 2) + 1] = RandomThick();
                                }
                            }
                            if (terrainStones)
                            {
                                int rocks = RandomRocks();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;

                        // Left side
                        case 84:
                            if (waterPlants && shores)
                            {
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        // Lower side
                        case 85:
                            if (waterPlants && shores)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[y * 2, (x * 2)] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        // Right side
                        case 86:
                            if (waterPlants && shores)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[(y * 2) + 1, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        // Upper side
                        case 87:
                            if (waterPlants && shores)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[(y * 2) + 1, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        // Corners
                        case 88:
                            if (waterPlants && shores)
                            {
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        case 89:
                            if (waterPlants && shores)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        case 90:
                            if (waterPlants && shores)
                            {
                                WaterPlants[y * 2, x * 2] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        case 91:
                            if (waterPlants && shores)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomWaterPlants();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomWaterPlants();
                            }
                            if (terrainStones)
                                Rocks[y * 2, (x * 2) + 1] = RandomRocksDesert();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set density for Desert locations.
        /// Desert has grass around water but not on mainland, also desert regions don't have winter season.
        /// </summary>
        public void SetDensityDesert(Color32[] tilemap)
        {
            const float grassChance = 0.3f;

            for (int y = 0; y < tilemapSize; y++)
            {
                for (int x = 0; x < tilemapSize; x++)
                {
                    switch (tilemap[(y * tilemapSize) + x].r)
                    {
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            if (terrainStones)
                            {
                                int rocks = RandomRocksDesert();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;

                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            if (Random.value < grassChance)
                            {
                                SetGrassDensity(y * 2, x * 2, RandomThin());
                                SetGrassDensity(y * 2, (x * 2) + 1, RandomThin());
                            }
                            if (Random.value < grassChance)
                            {
                                SetGrassDensity((y * 2) + 1, x * 2, RandomThin());
                                SetGrassDensity((y * 2) + 1, (x * 2) + 1, RandomThin());
                            }
                            if (terrainStones)
                            {
                                int rocks = RandomRocksDesert();
                                if (rocks != 0)
                                {
                                    var index = RandomPosition(y, x);
                                    Rocks[index.First, index.Second] = rocks;
                                }
                            }
                            break;
                        // Left side
                        case 84:
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, x * 2] = RandomDesert();
                                WaterPlants[y * 2, x * 2] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        // Lower side
                        case 85:
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomDesert();
                                WaterPlants[y * 2, (x * 2)] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        // Right side
                        case 86:
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                                WaterPlants[y * 2, (x * 2) + 1] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[(y * 2) + 1, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        // Upper side
                        case 87:
                            if (waterPlants)
                            {
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[(y * 2) + 1, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        // Corners
                        case 88:
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, x * 2] = RandomDesert();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        case 89:
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomDesert();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        case 90:
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, x * 2] = RandomDesert();
                                WaterPlants[(y * 2) + 1, (x * 2) + 1] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, x * 2] = RandomRocksDesert();
                            break;
                        case 91:
                            if (waterPlants)
                            {
                                WaterPlants[y * 2, (x * 2) + 1] = RandomDesert();
                                WaterPlants[(y * 2) + 1, x * 2] = RandomDesert();
                            }
                            if (terrainStones)
                                Rocks[y * 2, (x * 2) + 1] = RandomRocksDesert();
                            break;
                        // Left to right
                        case 204:
                        case 206:
                        case 214:
                            if (Random.value < grassChance)
                            {
                                Grass[y * 2, x * 2] = RandomThin();
                                Grass[(y * 2) + 1, (x * 2) + 1] = RandomThin();
                            }
                            break;

                        // Right to left
                        case 205:
                        case 207:
                        case 213:
                            if (Random.value < grassChance)
                            {
                                Grass[(y * 2) + 1, x * 2] = RandomThin();
                                Grass[y * 2, (x * 2) + 1] = RandomThin();
                            }
                            break;
                    }
                }
            }
        }

        #endregion

        #region Random Generators

        /// <summary>
        /// Generate random values for the placement of thin grass. 
        /// </summary>
        private int RandomThin()
        {
            return density.GrassThin.Random();
        }

        /// <summary>
        /// Generate random values for the placement of thick grass.
        /// </summary>
        private int RandomThick()
        {
            return density.GrassThick.Random();
        }

        /// <summary>
        /// Generate random values for the placement of water plants.
        /// </summary>
        private int RandomWaterPlants()
        {
            return density.WaterPlants.Random();
        }

        /// <summary>
        /// Generate random values for the placement of desert grass.
        /// </summary>
        private int RandomDesert()
        {
            return density.DesertPlants.Random();
        }

        private int RandomRocks()
        {
            return Random.Range(0, 100) < rocksDensity ? 5 : 0;
        }

        private int RandomRocksDesert()
        {
            return (int)(Random.value * 2);
        }

        /// <summary>
        /// Get a random position on tile.
        /// </summary>
        /// <param name="y">First index.</param>
        /// <param name="x">Second index.</param>
        /// <returns>One of the four possible position on tile.</returns>
        private static Tuple<int, int> RandomPosition(int y, int x)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return new Tuple<int, int>(y * 2, x * 2);
                case 1:
                    return new Tuple<int, int>(y * 2, (x * 2) + 1);
                case 2:
                    return new Tuple<int, int>((y * 2) + 1, x * 2);
                case 3:
                default:
                    return new Tuple<int, int>((y * 2) + 1, (x * 2) + 1);
            }
        }

        #endregion

        /// <summary>
        /// Sets density to grass layers.
        /// </summary>
        /// <param name="density"> The total grass density.</param>
        /// <param name="seasonalChance">Chance that details layers are populated in range 0-1.</param>
        private void SetGrassDensity(int x, int y, int density, float seasonalChance = 0)
        {
            if (realisticGrass)
            {
                // Accents are small tufts that add variety to grass
                if (density > 1 && Random.value > 0.7f)
                    density -= (GrassAccents[x, y] = Random.Range(0, density));

                // Details are tall flowers whose density is affected by season
                if (seasonalChance > 0 && Random.value < Mathf.Lerp(0, 0.25f, seasonalChance))
                    density -= (GrassDetails[x, y] = Random.Range(0, density));
            }

            // Main grass layer
            Grass[x, y] = density;
        }

        #region Static Methods

        private static int[,] EmptyMap(bool isValid = true)
        {
            const int size = 256;
            return isValid ? new int[size, size] : null;
        }

        private static int GetTerrainDensityPerennial(int maxDensity)
        {
            return Mathf.RoundToInt(Mathf.Lerp(0, maxDensity, Random.value));
        }

        private static float GetWinterGrassChance()
        {
            const float maxChance = 0.5f;

            int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;
            if (day > DaysOfYear.GrowDay && day <= DaysOfYear.Spring)
                return Mathf.Clamp(Mathf.InverseLerp(DaysOfYear.GrowDay, DaysOfYear.Spring, day), 0, maxChance);
            if (day >= DaysOfYear.Winter && day < DaysOfYear.DieDay)
                return Mathf.Clamp(1 - Mathf.InverseLerp(DaysOfYear.Winter, DaysOfYear.DieDay, day), 0, maxChance);
            return 0;
        }

        /// <summary>
        /// Additional grass with flowers grows in density during spring, dies during fall.
        /// </summary>
        /// <returns>A value between 0 and 1</returns>
        private static float GetGrassDetailsChance()
        {
            int day = DaggerfallUnity.Instance.WorldTime.Now.DayOfYear;
            if (day < DaysOfYear.Summer)
                return Mathf.InverseLerp(DaysOfYear.Spring, DaysOfYear.Summer, day);
            if (day < DaysOfYear.Fall)
                return 1;
            if (day < DaysOfYear.Winter)
                return 1 - Mathf.InverseLerp(DaysOfYear.Fall, DaysOfYear.Winter, day);
            return 0;
        }

        private static Vector3 GetTileWorldPosition(Terrain terrain, int x, int y)
        {
            Vector3 pos = terrain.GetPosition();
            pos.x += terrain.terrainData.size.x / tilemapSize * x;
            pos.z += terrain.terrainData.size.z / tilemapSize * y;
            pos.y = terrain.terrainData.GetHeight(x, y);
            return pos;
        }

        #endregion
    }
}
