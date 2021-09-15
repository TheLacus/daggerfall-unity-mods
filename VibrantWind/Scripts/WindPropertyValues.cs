// Project:         Vibrant Wind for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace VibrantWind
{
    public enum InterpolationType
    {
        Lerp, Sinerp, Coserp, SmoothStep
    }

    public delegate float GetValue(float min, float max, float t);

    public static class InterpolationTypes
    {
        public static GetValue Make(InterpolationType interpolationType)
        {
            switch (interpolationType)
            {
                case InterpolationType.Lerp:
                    return Mathf.Lerp;
                case InterpolationType.Sinerp:
                    return (min, max, t) => Mathf.Lerp(min, max, Mathf.Sin(t * Mathf.PI * 0.5f));
                case InterpolationType.Coserp:
                    return (min, max, t) => Mathf.Lerp(min, max, 1f - Mathf.Cos(t * Mathf.PI * 0.5f));
                case InterpolationType.SmoothStep:
                    return Mathf.SmoothStep;
            }

            throw new ArgumentException("Unknown interpolation type.", nameof(interpolationType));
        }
    }

    /// <summary>
    /// Values interpolated in a range for a wind property.
    /// </summary>
    public class WindPropertyValues : IReadOnlyList<float>
    {
        private readonly (float Min, float Max) range;
        private readonly GetValue getValue;

        /// <summary>
        /// The number of steps.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets interpolated value at the given step.
        /// </summary>
        public float this[int step]
        {
            get { return GetValue(step); }
        }

        public WindPropertyValues(int stepCount, (float Min, float Max) range, GetValue getValue)
        {
            this.Count = stepCount;
            this.range = range;
            this.getValue = getValue;
        }

        public override string ToString()
        {
            return string.Join(",", this.Select(x => x.ToString()));
        }

        public IEnumerator<float> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private float GetValue(int step)
        {
            if (step < 0 || step >= Count)
                throw new ArgumentOutOfRangeException(nameof(step));

            float t = ((float)step / (Count - 1));
            return (float)Math.Round(getValue(range.Min, range.Max, t), 2);
        }
    }
}
