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
    /// Values for the terrainData properties that affects wind.
    /// Note that there is a naming discrepancy between Unity API and Unity Editor.
    /// Here we use the latter ones because they are more meaningful.
    /// </summary>
    public class WindValues
    {
        /// <summary>
        /// Values for the terrain wind.
        /// </summary>
        public WindValues() {}
        
        /// <summary>
        /// Values for the terrain wind.
        /// Uses <paramref name="strength"/> for Speed, Bending and Size.
        /// </summary>
        /// <param name="strength"></param>
        public WindValues(float strength) : this(strength, strength, strength) { }

        /// <summary>
        /// Values for the terrain wind.
        /// </summary>
        public WindValues(float speed, float bending, float size)
        {
            this.Speed = speed;
            this.Bending = bending;
            this.Size = size;
        }

        /// <summary>
        /// The speed of the wind as it blows grass.
        /// </summary>
        /// <value>
        /// terrainData.Strength
        /// </value>
        public float Speed { get; set; }

        /// <summary>
        /// The degree to which grass objects are bent over by the wind.
        /// </summary>
        /// <value>
        /// terrainData.Amount
        /// </value>
        public float Bending { get; set; }

        /// <summary>
        /// The size of the “ripples” on grassy areas as the wind blows over them.
        /// </summary>
        /// <value>
        /// terrainData.Speed
        /// </value>
        public float Size { get; set; }

        public override string ToString()
        {
            return string.Format("Speed: {0}, Bending: {1}, Size {2}", Speed, Bending, Size);
        }

        public static implicit operator Vector3(WindValues wind)
        {
            return new Vector3(wind.Speed, wind.Bending, wind.Size);
        }

        public static implicit operator WindValues(Vector3 wind)
        {
            return new WindValues(wind.x, wind.y, wind.z);
        }
    }
}
