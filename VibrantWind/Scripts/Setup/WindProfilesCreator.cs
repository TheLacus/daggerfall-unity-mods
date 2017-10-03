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
using DaggerfallWorkshop.Game.Weather;

namespace VibrantWind
{
    public static class WindProfilesCreator
    {
        // Number of weathers.
        static readonly int precision = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().Distinct().Count();

        #region Methods

        public static WindStrength[] Get (StrengthSettings speedSettings, StrengthSettings bendingSettings, StrengthSettings sizeSettings)
        {
            float[] speed = Get(speedSettings);
            float[] bending = Get(bendingSettings);
            float[] size = Get(sizeSettings);

            var wind = new WindStrength[precision];
            for (int i = 0; i < precision; i++)
                wind[i] = new WindStrength(speed[i], bending[i], size[i]);
            return wind;
        }

        public static float[] Get (StrengthSettings settings)
        {
            var sV = new ScaledValues(settings.Range.First, settings.Range.Second, precision, settings.Interpolation);
            float[] values = sV.GetValues().ToArray();
            PrintValuesToLog(values, settings.Field);
            return values;
        }

        [Conditional("TEST_VALUES")]
        private static void PrintValuesToLog(float[] values, string field)
        {
            Func<float[], string> allValues = x => string.Join(",", x.Select(y => y.ToString()).ToArray());

            Debug.LogFormat("VibrantWind - {0}: {1}", field, allValues(values));
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
