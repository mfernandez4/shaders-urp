using System.Diagnostics;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class NoiseSettings
{
    
    
    public enum FilterType
    {
        Simple,
        Rigid
    }
    public FilterType filterType;
    
    
    // [ConditionalHide("filterType", 0)]
    public SimpleNoiseSettings simpleNoiseSettings;
    // [ConditionalHide("filterType", 1)]
    public RigidNoiseSettings rigidNoiseSettings;


    [System.Serializable]
    public class SimpleNoiseSettings
    {
        /// <summary>
        /// The strength of the noise.
        /// </summary>
        public float strength = 1;
    
        /// <summary>
        /// Number of layers of noise.
        /// </summary>
        [Range(1,8)] public int octaves = 3;
    
        /// <summary>
        /// The roughness of the first layer of noise.
        /// </summary>
        [FormerlySerializedAs("baseRoughness")] public float frequency = 1;
    
        /// <summary>
        /// How much detail is added or removed at each octave.
        /// </summary>
        public float roughness = 2;
    
        /// <summary>
        /// How much each octave contributes to the overall shape.
        /// </summary>
        public float persistence = 0.5f;
    
        /// <summary>
        /// Center of the noise.
        /// </summary>
        public Vector3 center;
    
        /// <summary>
        /// The minimum value of the noise.
        /// </summary>
        public float minValue;
    }

    
    [System.Serializable]
    public class RigidNoiseSettings: SimpleNoiseSettings
    {
        /// <summary>
        /// The weight multiplier for the noise.
        /// </summary>
        public float weightMultiplier = 0.8f;
    }
}
