using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu]
public class SettingsColor : ScriptableObject
{
    [Tooltip("The material that will be used on the planet.")]
    public Material planetMaterial;
    
    [Tooltip("The biome color settings that will be used to color the planet.")]
    public BiomeColorSettings biomeColorSettings;
    
    [Tooltip("The ocean gradient color of the planet.")]
    public Gradient oceanColor;

    
    
    [System.Serializable]
    public class BiomeColorSettings
    {
        [Tooltip("The biomes that will be used to color the planet.")]
        public Biome[] biomes;
        public NoiseSettings noise;
        public float noiseOffset;
        public float noiseStrength;
        
        [Range(0, 1)]
        public float blendAmount;
        
        
        
        
        [System.Serializable]
        public class Biome
        {
            [Tooltip("The gradient that will be used to color the biome.")]
            public Gradient gradient;
            
            [Tooltip("The tint of the biome.")]
            public Color tint;
            
            [Range(0, 1), Tooltip("The start height of the biome.")] 
            public float startHeight;
            
            [Range(0, 1), Tooltip("The tint percent of the biome.")]
            public float tintPercent;
        }
    }
}
