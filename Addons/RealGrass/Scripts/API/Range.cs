// Project:         RealGrass for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=17
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/realgrass-du-mod
// Original Author: TheLacus
// Contributors:

using DaggerfallWorkshop.Utility;

namespace RealGrass
{
    /// <summary>
    /// Defines a range with Min and Max values.
    /// </summary>
    /// <typeparam name="T">Numeric type of Min and Max.</typeparam>
    public class Range<T> where T : struct
    {
        public T Min;
        public T Max;

        public Range(T Min, T Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public static implicit operator Range<T>(Tuple<T,T> t)
        {
            return new Range<T>(t.First, t.Second);
        }

        public static implicit operator Tuple<T,T>(Range<T> r)
        {
            return new Tuple<T,T>(r.Min, r.Max);
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Get a random number in this range.
        /// </summary>
        public static int Random(this Range<int> r)
        {
            return UnityEngine.Random.Range(r.Min, r.Max);
        }

        /// <summary>
        /// Get a random number in this range.
        /// </summary>
        public static float Random(this Range<float> r)
        {
            return UnityEngine.Random.Range(r.Min, r.Max);
        }
    }
}