// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

using System;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;

namespace VibrantWind
{
    public static class InterpolationType
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
        #region Properties

        /// <summary>
        /// Limits of the interpolation.
        /// </summary>
        public DaggerfallWorkshop.Utility.Tuple<float, float> Range { get; set; }

        /// <summary>
        /// Number of values to generate.
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Interpolation method defined in <see cref="InterpolationType"/>.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets interpolated value at the given step.
        /// </summary>
        public float this[int step]
        {
            get { return GetValue(step); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Get values between min and max using interpolation.
        /// </summary>
        public Interpolation()
        {
        }

        /// <summary>
        /// Get values between min and max using interpolation.
        /// </summary>
        /// <param name="range">Limits of the interpolation.</param>
        /// <param name="precision">Number of values to generate.</param>
        /// <param name="type">Interpolation method defined in <see cref="InterpolationType"/>.</param>
        public Interpolation(DaggerfallWorkshop.Utility.Tuple<float, float> range, int precision, int type = 0)
        {
            this.Range = range;
            this.Precision = precision;
            this.Type = type;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Join(",", GetValues().Select(x => x.ToString()).ToArray());
        }

        /// <summary>
        /// Gets interpolated value at the given step.
        /// </summary>
        public float GetValue(int step)
        {
            if (step < 0 || step >= Precision)
                throw new ArgumentException("Step is not in range!", "step");

            float t = (step != 0) ? ((float)step / (Precision - 1)) : 0;
            return (float)Math.Round(Interpolate(t), 2);
        }

        /// <summary>
        /// Gets all interpolated values.
        /// </summary>
        public IEnumerable<float> GetValues()
        {
            return Enumerable.Range(0, Precision).Select(x => GetValue(x));
        }

        /// <summary>
        /// Gets all interpolated values.
        /// </summary>
        public float[] ToArray()
        {
            return GetValues().ToArray();
        }

        #endregion

        #region Private Methods

        private float Interpolate(float t)
        {
            if (Range == null)
                throw new Exception("Interpolation range is not set!");

            switch (Type)
            {
                case InterpolationType.Lerp:
                    return Mathf.Lerp(Range.First, Range.Second, t);

                case InterpolationType.Sinerp:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    return Mathf.Lerp(Range.First, Range.Second, t);

                case InterpolationType.Coserp:
                    t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                    return Mathf.Lerp(Range.First, Range.Second, t);

                case InterpolationType.SmoothStep:
                    return Mathf.SmoothStep(Range.First, Range.Second, t);
            }

            throw new Exception(string.Format("Unknown interpolation type: {0}", Type));
        }

        #endregion
    }
}
