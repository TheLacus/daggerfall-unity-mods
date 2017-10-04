// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using System.Linq;
using UnityEngine;

namespace VibrantWind
{
    public struct InterpolationType
    {
        public const int

            Lerp = 0,
            Sinerp = 1,
            Coserp = 2,
            SmoothStep = 3;
    }

    /// <summary>
    /// Interpolate a group of values between min and max.
    /// </summary>
    public class Interpolation
    {
        readonly float min, max;
        readonly int count, maxStep;
        readonly int interpolation;

        /// <summary>
        /// Get values between min and max using interpolation.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">maximum value.</param>
        /// <param name="count">Number of values to generate.</param>
        /// <param name="interpolation">Type of interpolation to use.</param>
        public Interpolation(float min, float max, int count, int interpolation)
        {
            this.min = min;
            this.max = max;
            this.count = count;
            this.maxStep = count - 1;
            this.interpolation = interpolation;
        }

        /// <summary>
        /// Get interpolated values.
        /// </summary>
        public float[] GetValues()
        {
            return Enumerable.Range(0, count).Select(x => GetValue(x)).ToArray();
        }

        private float GetValue(int step)
        {
            float t = (step != 0) ? (step / (float)maxStep) : 0;
            return (float)Math.Round(Interpolate(t), 2);
        }

        private float Interpolate(float t)
        {
            switch (interpolation)
            {
                case InterpolationType.Lerp:
                default:
                    return Mathf.Lerp(min, max, t);

                case InterpolationType.Sinerp:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    return Mathf.Lerp(min, max, t);

                case InterpolationType.Coserp:
                    t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                    return Mathf.Lerp(min, max, t);

                case InterpolationType.SmoothStep:
                    return Mathf.SmoothStep(min, max, t);
            }
        }
    }
}
