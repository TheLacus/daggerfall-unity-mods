// Project:         RealGrass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:

using UnityEngine;

namespace RealGrass
{
    public class Indices
    {
        public int

            Grass,
            WaterPlants,
            Waterlilies,
            Stones,
            Rocks,
            Bushes,
            Flowers,
            CommonFlowers;
    }

    public class GrassColors
    {
        public Color
            springHealthy,
            springDry,
            summerHealty,
            summerDry,
            fallHealty,
            fallDry;
    }

    public class PrototypesProperties
    {
        public bool import;
        public string packName;
        public Range<float> grassHeight;
        public Range<float> grassWidth;
        public float noiseSpread;
        public GrassColors grassColors;
        public bool useGrassShader;
        public float noiseSpreadPlants;
    }

    public class Density
    {
        public Range<int> grassThick;
        public Range<int> grassThin;
        public Range<int> waterPlants;
        public Range<int> desertPlants;
        public Range<int> stones;
        public int rocks;
        public int flowers;
        public int bushes;
        //public Range<int> flowersBush;
    }
}