// Project:         Vibrant Wind for Daggerfall Unity
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;

namespace VibrantWind
{
    /// <summary>
    /// Values for the terrainData properties that affects wind.
    /// </summary>
    public readonly struct WindStrength
    {
        /// <summary>
        /// The speed of the wind as it blows grass.
        /// </summary>
        /// <value>
        /// <see cref="TerrainData.wavingGrassStrength"/>
        /// </value>
        public float Speed { get; }

        /// <summary>
        /// The degree to which grass objects are bent over by the wind.
        /// </summary>
        /// <value>
        /// <see cref="TerrainData.wavingGrassAmount"/>
        /// </value>
        public float Bending { get; }

        /// <summary>
        /// The size of the “ripples” on grassy areas as the wind blows over them.
        /// </summary>
        /// <value>
        /// <see cref="TerrainData.wavingGrassSpeed"/>
        /// </value>
        public float Size { get; }

        /// <summary>
        /// Values for the terrain wind.
        /// </summary>
        public WindStrength(float speed, float bending, float size)
        {
            this.Speed = speed;
            this.Bending = bending;
            this.Size = size;
        }

        /// <summary>
        /// Set Wind Strength to TerrainData.
        /// </summary>
        public void Assign(TerrainData terrainData)
        {
            terrainData.wavingGrassStrength = Speed;
            terrainData.wavingGrassAmount = Bending;
            terrainData.wavingGrassSpeed = Size;
        }

        /// <summary>
        /// Returns a nicely formatted string with all wind values.
        /// </summary>
        public override string ToString()
        {
            return $"Speed: {Speed}, Bending: {Bending}, Size {Size}";
        }

        public static implicit operator Vector3(WindStrength wind)
        {
            return new Vector3(wind.Speed, wind.Bending, wind.Size);
        }

        public static implicit operator WindStrength(Vector3 wind)
        {
            return new WindStrength(wind.x, wind.y, wind.z);
        }
    }
}
