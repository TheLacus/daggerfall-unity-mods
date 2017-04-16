// Project:         RealGrass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: Uncanny_Valley
// Contributors:    Midopa, TheLacus

using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace RealGrass
{
    /// <summary>
    /// Adds grass to the terrain based on the tiles.
    ///
    /// The density of grass is set randomly based on the type of tile. Tiles that are all grass will have a higher
    /// grass billboard density. Tiles that are partially grass will have lower density.
    ///
    /// The position, height, and width of the grass billboards are randomly generated.
    /// </summary>
    public class RealGrass : MonoBehaviour
    {
        #region Fields

        // Textures
        const string brownGrass = "tex_BrownGrass";
        const string greenGrass = "tex_GreenGrass";

        // Description of the grass billboards to render
        DetailPrototype[] _detailPrototype;

        // Intentionally empty grass map to clear grass tiles when we move from a grassy area to a barren one
        int[,] _noGrassMap;

        // Ranges for number of grass billboards per terrain tile
        static int MinGrassThick;
        static int MaxGrassThick;
        static int MinGrassSparse;
        static int MaxGrassSparse;
        // Ranges for shape of grass billboards
        static float MinGrassHeight;
        static float MaxGrassHeight;
        static float MinGrassWidth;
        static float MaxGrassWidth;
        // The "spread" of the variety of grass, if you view it as a distribution of heights & widths.
        // The higher this is, the more varied the grass billboards will look.
        // This does not control the density of the grass, but it does control the positions.
        static float GrassVarietyFactor;

        // Convenience/readability consts for how a tile should be filled with grass
        public struct Fill
        {
            public const int

                All = 0,
                UpperSide = 1,
                LowerSide = 2,
                RightSide = 3,
                LeftSide = 4,
                OnlyUpperRight = 5,
                OnlyUpperLeft = 6,
                OnlyLowerRight = 7,
                OnlyLowerLeft = 8,
                RightToLeft = 9,
                LeftToRight = 10,
                NotUpperRight = 11,
                NotLowerRight = 12,
                NotLowerLeft = 13,
                NotUpperLeft = 14,
                None = 15;
        }

        #endregion

        #region Start Mod

        void Start()
        {
            // Load settings
            ModSettings settings = RealGrassLoader.Settings;
            const string grassSection = "Grass";
            MinGrassThick = settings.GetInt(grassSection, "thickLower");
            MaxGrassThick = settings.GetInt(grassSection, "thickHigher");
            MinGrassSparse = settings.GetInt(grassSection, "thinLower");
            MaxGrassSparse = settings.GetInt(grassSection, "thinHigher");
            MinGrassHeight = settings.GetFloat(grassSection, "MinGrassHeight");
            MaxGrassHeight = settings.GetFloat(grassSection, "MaxGrassHeight");
            MinGrassWidth = settings.GetFloat(grassSection, "MinGrassWidth");
            MaxGrassWidth = settings.GetFloat(grassSection, "MaxGrassWidth");
            GrassVarietyFactor = settings.GetFloat(grassSection, "NoiseSpread");

            //Subscribe to the onPromoteTerrainData
            DaggerfallTerrain.OnPromoteTerrainData += AddGrass;

            //Create a holder for our grass
            _detailPrototype = new[]
            {
                new DetailPrototype
                {
                    minHeight = MinGrassHeight,
                    maxHeight = MaxGrassHeight,
                    minWidth = MinGrassWidth,
                    maxWidth = MaxGrassWidth,
                    noiseSpread = GrassVarietyFactor,
                    healthyColor = new Color(0.70f, 0.70f, 0.70f),
                    dryColor = new Color(0.70f, 0.70f, 0.70f),
                    renderMode = DetailRenderMode.GrassBillboard
                }
            };

            _noGrassMap = new int[256, 256];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a random grass count for thick patches of grass.
        /// </summary>
        /// <returns>Number of grass billboards to draw</returns>
        static int GetThick()
        {
            return Random.Range(MinGrassThick, MaxGrassThick);
        }

        /// <summary>
        /// Get a random grass count for sparse patches of grass.
        /// </summary>
        /// <returns>Number of grass billboards to draw</returns>
        static int GetSparse()
        {
            return Random.Range(MinGrassSparse, MaxGrassSparse);
        }

        /// <summary>
        /// Determines the quadrants that need to be filled. Broken out for readability.
        /// </summary>
        /// <param name="tileCode">The terrain tile information</param>
        /// <returns>The fill type for the terrain tile</returns>
        static int GetGrassFillType(byte tileCode)
        {
            switch (tileCode)
            {
                case 8:
                case 9:
                case 10:
                case 11:
                    return Fill.All;

                case 40:
                case 164:
                case 176:
                case 181:
                case 224:
                    return Fill.OnlyUpperLeft;

                case 41:
                case 165:
                case 177:
                case 182:
                case 221:
                    return Fill.OnlyLowerLeft;

                case 42:
                case 166:
                case 178:
                case 183:
                case 222:
                    return Fill.OnlyLowerRight;

                case 43:
                case 167:
                case 179:
                case 180:
                case 223:
                    return Fill.OnlyUpperRight;

                case 44:
                case 66:
                case 84:
                case 160:
                case 168:
                    return Fill.LeftSide;

                case 45:
                case 67:
                case 85:
                case 161:
                case 169:
                    return Fill.LowerSide;

                case 46:
                case 64:
                case 86:
                case 162:
                case 170:
                    return Fill.RightSide;

                case 47:
                case 65:
                case 87:
                case 163:
                case 171:
                    return Fill.UpperSide;

                case 48:
                case 62:
                case 88:
                case 156:
                    return Fill.NotLowerRight;

                case 49:
                case 63:
                case 89:
                case 157:
                    return Fill.NotUpperRight;

                case 50:
                case 60:
                case 90:
                case 158:
                    return Fill.NotUpperLeft;

                case 51:
                case 61:
                case 91:
                case 159:
                    return Fill.NotLowerLeft;

                case 204:
                case 206:
                case 214:
                    return Fill.LeftToRight;

                case 205:
                case 207:
                case 213:
                    return Fill.RightToLeft;
            }
            return Fill.None;
        }

        /// <summary>
        /// Set the grass density into the given grass detail map
        /// </summary>
        /// <param name="grassFill">Quadrants to fill</param>
        /// <param name="y">Y position on terrain detail map to fill</param>
        /// <param name="x">X position on terrain detail map to fill</param>
        /// <param name="grassMap">Terrain detail map to fill with grass counts</param>
        static void SetGrassDensity(int grassFill, int y, int x, ref int[,] grassMap)
        {
            switch (grassFill)
            {
                case Fill.All:
                    grassMap[y, x] = GetThick();
                    grassMap[y, x + 1] = GetThick();
                    grassMap[y + 1, x] = GetThick();
                    grassMap[y + 1, x + 1] = GetThick();
                    break;

                case Fill.UpperSide:
                    grassMap[y + 1, x + 1] = GetSparse();
                    grassMap[y + 1, x] = GetSparse();
                    break;

                case Fill.LowerSide:
                    grassMap[y, x + 1] = GetSparse();
                    grassMap[y, x] = GetSparse();
                    break;

                case Fill.RightSide:
                    grassMap[y + 1, x + 1] = GetSparse();
                    grassMap[y, x + 1] = GetSparse();
                    break;

                case Fill.LeftSide:
                    grassMap[y + 1, x] = GetSparse();
                    grassMap[y, x] = GetSparse();
                    break;

                case Fill.OnlyUpperRight:
                    grassMap[y + 1, x + 1] = GetSparse();
                    break;

                case Fill.OnlyUpperLeft:
                    grassMap[y + 1, x] = GetSparse();
                    break;

                case Fill.OnlyLowerRight:
                    grassMap[y, x + 1] = GetSparse();
                    break;

                case Fill.OnlyLowerLeft:
                    grassMap[y, x] = GetSparse();
                    break;

                case Fill.RightToLeft:
                    grassMap[y + 1, x] = GetSparse();
                    grassMap[y, x + 1] = GetSparse();
                    break;

                case Fill.LeftToRight:
                    grassMap[y, x] = GetSparse();
                    grassMap[y + 1, x + 1] = GetSparse();
                    break;

                case Fill.NotUpperRight:
                    grassMap[y, x] = GetSparse();
                    grassMap[y, x + 1] = GetSparse();
                    grassMap[y + 1, x] = GetSparse();
                    break;

                case Fill.NotLowerRight:
                    grassMap[y, x] = GetSparse();
                    grassMap[y + 1, x] = GetSparse();
                    grassMap[y + 1, x + 1] = GetSparse();
                    break;

                case Fill.NotLowerLeft:
                    grassMap[y, x + 1] = GetSparse();
                    grassMap[y + 1, x] = GetSparse();
                    grassMap[y + 1, x + 1] = GetSparse();
                    break;

                case Fill.NotUpperLeft:
                    grassMap[y, x] = GetSparse();
                    grassMap[y, x + 1] = GetSparse();
                    grassMap[y + 1, x + 1] = GetSparse();
                    break;

                case Fill.None:
                default:
                    break;
            }
        }

        /// <summary>
        /// Add Grass.
        /// </summary>
        void AddGrass(DaggerfallTerrain daggerTerrain, TerrainData terrainData)
        {
            //            // Used to check performance
            //            Stopwatch stopwatch = new Stopwatch();
            //            stopwatch.Start();

            // Terrain settings
            RealGrassLoader.InitTerrain(terrainData);
            var currentSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;

            // If it's winter or a climate with no grass, then pass an empty density map so the grass gets cleared
            if (currentSeason == DaggerfallDateTime.Seasons.Winter
                || daggerTerrain.MapData.worldClimate <= 225
                || daggerTerrain.MapData.worldClimate == Climate.Desert3)
            {
                terrainData.detailPrototypes = _detailPrototype;
                terrainData.SetDetailLayer(0, 0, 0, _noGrassMap);
                return;
            }

            // Switch the grass texture based on the climate
            if (daggerTerrain.MapData.worldClimate == Climate.Mountain || daggerTerrain.MapData.worldClimate == Climate.Swamp ||
                daggerTerrain.MapData.worldClimate == Climate.Swamp2 || daggerTerrain.MapData.worldClimate == Climate.Mountain2)
                _detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(brownGrass);
            else
                _detailPrototype[0].prototypeTexture = RealGrassLoader.LoadTexture(greenGrass);

            terrainData.detailPrototypes = _detailPrototype;

            var grassMap = new int[256, 256];

            // The red channel specifies the kind of terrain.
            // This tell us which quadrants of the tile grass can be drawn on.
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    byte tileCode = daggerTerrain.TileMap[i * 128 + j].r;
                    var fillType = GetGrassFillType(tileCode);
                    SetGrassDensity(fillType, i * 2, j * 2, ref grassMap);
                }
            }

            terrainData.SetDetailLayer(0, 0, 0, grassMap);

            //            stopwatch.Stop();
            //            // Write result
            //            Debug.LogWarning("Time elapsed: " + stopwatch.Elapsed.TotalMilliseconds + " ms");
        }

        #endregion
    }
}
