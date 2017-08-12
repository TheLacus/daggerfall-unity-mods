// Project:         Vibrant Wind for Daggerfall Unity
// Web Site:        http://forums.dfworkshop.net/viewtopic.php?f=14&t=532
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/TheLacus/vibrantwind-du-mod
// Original Author: TheLacus
// Contributors:  

using UnityEngine;

namespace VibrantWind
{
    /// <summary>
    /// Possible values for the wind strength. 
    /// Every strength is composed of three values, defined as <see cref="WindStrength"/>.
    /// </summary>
    public class WindProfiles
    {
        /// <summary>
        /// Number of possible wind strengths.
        /// </summary>
        public const int Items = 6;

        public WindStrength

            None,
            VeryLight,
            Light,
            Medium,
            Strong,
            VeryStrong;

        /// <summary>
        /// Gets or sets the <see cref="WindValues"/> for the specified index.
        /// </summary>
        public WindStrength this[int i]
        {
            get { return GetValue(i); }
            set { SetValue(i, value); }
        }

        /// <summary>
        /// Gets the <see cref="WindValues"/> for the specified relative index in range 0-1.
        /// </summary>
        public WindStrength this[float i]
        {
            get { return GetValue(RelToIndex(i)); }
        }

        /// <summary>
        /// Returns a string with the range of this wind strength.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Wind is in Range [{0} - {1}]", None, VeryStrong);
        }

        #region Private Methods

        private WindStrength GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return None;
                case 1:
                    return VeryLight;
                case 2:
                    return Light;
                case 3:
                    return Medium;
                case 4:
                    return Strong;
                case 5:
                    return VeryStrong;
            }
            Debug.LogWarningFormat("WindStrength index out of range: {0}!", i);
            return GetValue(Mathf.Clamp(i, 0, 5));
        }

        private void SetValue(int i, WindStrength values)
        {
            switch (i)
            {
                case 0:
                    None = values;
                    break;
                case 1:
                    VeryLight = values;
                    break;
                case 2:
                    Light = values;
                    break;
                case 3:
                    Medium = values;
                    break;
                case 4:
                    Strong = values;
                    break;
                case 5:
                    VeryStrong = values;
                    break;
                default:
                    Debug.LogWarningFormat("WindStrength index out of range: {0}!", i);
                    break;
            }
        }

        private int RelToIndex(float i)
        {
            i = Mathf.Clamp01(i) * Items;
            return Mathf.RoundToInt(i);
        }

        #endregion
    }
}
