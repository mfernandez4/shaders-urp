using UnityEngine;

public class RigidNoiseFilter : INoiseFilter
{
    public NoiseSettings.RigidNoiseSettings settings;
    Noise noise = new();
    
    public RigidNoiseFilter(NoiseSettings.RigidNoiseSettings settings)
    {
        this.settings = settings;
    }

    
    // Evaluate the noise at a given point
    public float Evaluate(Vector3 point)
    {
        // returns a value between -1 and 1
        float noiseValue = 0;
        float frequency = settings.frequency;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float v = 1-Mathf.Abs( noise.Evaluate(point * frequency + settings.center) );
            v *= v; // Square the noise value to make it more rigid
            v *= weight; // Apply the weight to the noise value
            weight = Mathf.Clamp01(v * settings.weightMultiplier); // Clamp the weight to 0 or 1
            
            noiseValue += v * amplitude; // Multiply the noise value by the amplitude
            frequency *= settings.roughness; // Scale the frequency by the roughness
            amplitude *= settings.persistence; // Scale the amplitude by the persistence
        }
        
        // clamp the lowest noise values to the minimum value.
        // noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        noiseValue -= settings.minValue;
        noiseValue *= settings.strength; // Scale the noise value by the strength
        // noiseValue = Mathf.Pow(noiseValue, settings.contrast); // Apply contrast to the noise value
        return noiseValue;
    }
}
