// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:  

using DaggerfallWorkshop.Utility;

namespace VibrantWind
{
    public struct Interpolation
    {
        public const int

            Lerp = 0,
            Sinerp = 1,
            Coserp = 2,
            SmoothStep = 3;
    }

    public struct StrengthSettings
    {
        public Tuple<float, float> Range;
        public int Interpolation;
    }
}