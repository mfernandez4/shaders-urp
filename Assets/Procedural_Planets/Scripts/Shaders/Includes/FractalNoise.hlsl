// FractalNoise.hlsl
#include "SimplexNoise.hlsl"


struct noise_layer {
    int type; // 0 = Simple, 1 = Rigid
    int enabled; // 0 = false, 1 = true
    int use_first_layer_as_mask; // 0 = false, 1 = true
    float strength;
    int octaves;
    float frequency;
    float roughness;
    float persistence;
    float3 center;
    float min_value;
    float weight_multiplier; // Only used for Rigid Noise

    float _padding1;
};


float simple_noise(float3 vertex, noise_layer noise_layer)
{
    float noise_value = 0.f;
    float frequency = noise_layer.frequency;
    float amplitude = 1.f;

    // perform the noise calculation
    for (int i = 0; i < noise_layer.octaves; i++)
    {
        float v = snoise(vertex * frequency + noise_layer.center);
        noise_value += (v + 1.f) * 0.5f * amplitude; // Normalize the noise value to [0, 1]
        frequency *= noise_layer.roughness; // scale the frequency by the roughness for the next octave
        amplitude *= noise_layer.persistence; // scale the amplitude by the persistence for the next octave
    }

    // clamp the noise value to the min value
    noise_value = max(0, noise_value - noise_layer.min_value);
    // scale the noise value by the strength
    noise_value *= noise_layer.strength;

    // return the noise value
    return noise_value;
}

float rigid_noise(float3 vertex, noise_layer noise_layer)
{
    float noise_value = 0;
    float frequency = noise_layer.frequency;
    float amplitude = 1.f;
    float weight = 1.f;

    // perform the noise calculation
    for (int i = 0; i < noise_layer.octaves; i++)
    {
        float v = 1.0f - abs(snoise(vertex * frequency + noise_layer.center));
        v *= v; // Square the value to make it more rigid
        v *= weight; // Multiply the value by the weight
        weight = clamp(v * noise_layer.weight_multiplier, 0, 1); // clamp the weight to [0, 1]
        
        noise_value += v * amplitude; // Multiply the value by the amplitude
        frequency *= noise_layer.roughness; // scale the frequency by the roughness for the next octave
        amplitude *= noise_layer.persistence; // scale the amplitude by the persistence for the next octave
    }

    // clamp the noise value to the min value
    noise_value = max(0, noise_value - noise_layer.min_value);
    // scale the noise value by the strength
    noise_value *= noise_layer.strength;
    
    // return the noise value
    return noise_value;
	// return noise_value * noise_layer.strength;
}