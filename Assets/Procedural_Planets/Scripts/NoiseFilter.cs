using UnityEngine;

public class NoiseFilter
{
    public NoiseSettings settings;
    Noise noise = new();
    
    public NoiseFilter(NoiseSettings settings)
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

        for (int i = 0; i < settings.octaves; i++)
        {
            float v = noise.Evaluate(point * frequency + settings.center);
            noiseValue += (v + 1) * 0.5f * amplitude; // Clamp noise value from 0 to 1
            frequency *= settings.roughness; // Scale the frequency by the roughness
            amplitude *= settings.persistence; // Scale the amplitude by the persistence
        }
        
        // clamp the lowest noise values to the minimum value.
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        noiseValue *= settings.strength; // Scale the noise value by the strength
        // noiseValue = Mathf.Pow(noiseValue, settings.contrast); // Apply contrast to the noise value
        return noiseValue;
    }
}
