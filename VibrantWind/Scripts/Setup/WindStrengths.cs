// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:    

// #define TEST_VALUES

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VibrantWind
{
    public class WindStrengths
    {
        List<float> speed, bending, size;

        int n = -1;

        /// <summary>
        /// Get all wind values.
        /// </summary>
        /// <param name="range">Min and max value.</param>
        /// <param name="interpolation">Interpolation to use.</param>
        public WindStrength GetStrengths (StrengthSettings speedSettings, StrengthSettings bendingSettings, StrengthSettings sizeSettings)
        {
            speed = InitList(speedSettings);
            bending = InitList(bendingSettings);
            size = InitList(sizeSettings);

#if TEST_VALUES
            Debug.Log(string.Format("VibrantWind: Speed {0}\nBending {1}\nSize {2}", 
                AllValues(speed), AllValues(bending), AllValues(size)));
#endif

            var windStrength = new WindStrength();
            for (int i = 0; i < WindStrength.Items; i++)
                windStrength[i] = NextValues();

            return windStrength;
        }

        /// <summary>
        /// Get all values for one property.
        /// </summary>
        private List<float> InitList(StrengthSettings settings)
        {
            const uint times = WindStrength.Items - 1;

            List<float> list = new List<float>();
            var sV = new ScaledValues(settings.Range.First, settings.Range.Second, times, settings.Interpolation);

            for (int i = 0; i < WindStrength.Items; i++)
                list.Add(sV.NextValue());

            return list;
        }

        /// <summary>
        /// Create a set of strength, amount and speed values.
        /// </summary>
        private WindValues NextValues()
        {
            return new WindValues(speed[++n], bending[n], size[n]);
        }

#if TEST_VALUES
        private string AllValues(List<float> list)
        {
            return string.Join(",", list.Select(x => x.ToString()).ToArray());
        }
#endif

        /// <summary>
        /// Scales a group of values between min and max.
        /// </summary>
        public class ScaledValues
        {
            readonly float min, max;
            readonly uint times;
            readonly int interpolation;

            uint current = 0;

            /// <summary>
            /// Get values between min and max using interpolation.
            /// </summary>
            /// <param name="min">0 on 0-1</param>
            /// <param name="max">1 on 0-1</param>
            /// <param name="times">Total number of values.</param>
            /// <param name="interpolation">Type of interpolation to use.</param>
            public ScaledValues(float min, float max, uint times, int interpolation)
            {
                this.min = min;
                this.max = max;
                this.times = times;
                this.interpolation = interpolation;

#if TEST_VALUES
                Debug.Log(string.Format("VibrantWind: interpolation {0}", interpolation));
#endif
            }

            public float NextValue()
            {
                float t = (current != 0) ? (current / (float)times) : 0;
                float value = (float)Math.Round(GetValue(t), 2);

#if TEST_VALUES
                Debug.Log(string.Format("VibrantWind: item {0}, rel {1}, value {2}", current, t, value));
#endif

                current++;
                return value;
            }

            private float GetValue(float t)
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
    }
}
