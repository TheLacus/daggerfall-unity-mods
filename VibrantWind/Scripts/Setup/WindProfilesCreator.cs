// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

// #define TEST_VALUES

using System;
using System.Linq;
using System.Collections.Generic;
using Conditional = System.Diagnostics.ConditionalAttribute;
using UnityEngine;

namespace VibrantWind
{
    public static class WindProfilesCreator
    {
        #region Methods

        /// <summary>
        /// Get wind profiles.
        /// </summary>
        /// <param name="range">Min and max value.</param>
        /// <param name="interpolation">Interpolation to use.</param>
        public static WindProfiles Get (StrengthSettings speedSettings, StrengthSettings bendingSettings, StrengthSettings sizeSettings)
        {
            List<float> speed = NewList(speedSettings);
            List<float> bending = NewList(bendingSettings);
            List<float> size = NewList(sizeSettings);
            PrintValuesToLog(speed, bending, size);

            var wind = new WindProfiles();
            for (int i = 0; i < WindProfiles.Items; i++)
                wind[i] = new WindStrength(speed[i], bending[i], size[i]);
            return wind;
        }

        /// <summary>
        /// Create a list with all values interpolated as per settings.
        /// </summary>
        private static List<float> NewList(StrengthSettings settings)
        {
            var sV = new ScaledValues(settings.Range.First, settings.Range.Second, WindProfiles.Items, settings.Interpolation);
            return sV.GetValues().ToList();
        }

        [Conditional("TEST_VALUES")]
        private static void PrintValuesToLog(List<float> speed, List<float> bending, List<float> size)
        {
            Func<List<float>, string> allValues = x => string.Join(",", x.Select(y => y.ToString()).ToArray());

            Debug.LogFormat("VibrantWind interpolated values:\nSpeed {0}\nBending {1}\nSize {2}",
                allValues(speed), allValues(bending), allValues(size));
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Scales a group of values between min and max.
        /// </summary>
        private class ScaledValues
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
            public ScaledValues(float min, float max, int count, int interpolation)
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
            public IEnumerable<float> GetValues()
            {
                return Enumerable.Range(0, count).Select(x => GetValue(x));
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
                    case Interpolation.Lerp:
                    default:
                        return Mathf.Lerp(min, max, t);

                    case Interpolation.Sinerp:
                        t = Mathf.Sin(t * Mathf.PI * 0.5f);
                        return Mathf.Lerp(min, max, t);

                    case Interpolation.Coserp:
                        t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                        return Mathf.Lerp(min, max, t);

                    case Interpolation.SmoothStep:
                        return Mathf.SmoothStep(min, max, t);
                }
            }
        }
        
        #endregion
    }
}
