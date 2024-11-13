// #include "SimplexNoise.hlsl"

// Pseudo-random function based on the position
float random(float3 position)
{
    return frac(sin(dot(position, float3(127.1, 311.7, 74.7)) * 43758.5453123));
}

// A simple hash function to generate a pseudo-random value
float random3d(int3 position)
{
    position = position * 1664525 + 1013904223;
    return frac(sin(dot(position, float3(12.9898, 78.223, 74.7))) * 43758.5453);
}

float rand_1_05(float3 position)
{
    float3 noise = (frac(sin(dot(position ,float3(12.9898,78.233,74.7)*2.0)) * 43758.5453));
    return abs(noise.x + noise.y + noise.z) * 0.5;
}

// Quintic interpolation function
float interp(float x)
{
    return x * x * x * (x * (x * 6.f - 15.f) + 10.f);
}

float interp_derivative(float x)
{
    return 30.f * x * x * (x - 1.f) * (x - 1.f);
}

// Returns 3D value noise and its 3 derivatives
float4 noised(float3 position)
{
    // Get the integer part of the position
    float3 p = floor(position);
    // Get the fractional part of the position
    float3 w = frac(position);

    // Interpolation functions ( quintic interpolation ) for the x, y, z components
    float3 u = float3(interp(w.x), interp(w.y), interp(w.z));
    float3 du = float3( interp_derivative(w.x), interp_derivative(w.y), interp_derivative(w.z) );

    // Random Values at the corners of the cube
    float a = rand_1_05(p + float3(0, 0, 0));
    float b = rand_1_05(p + float3(1, 0, 0));
    float c = rand_1_05(p + float3(0, 1, 0));
    float d = rand_1_05(p + float3(1, 1, 0));
    float e = rand_1_05(p + float3(0, 0, 1));
    float f = rand_1_05(p + float3(1, 0, 1));
    float g = rand_1_05(p + float3(0, 1, 1));
    float h = rand_1_05(p + float3(1, 1, 1));

    // Compute coefficients
    float k0 = a;
    float k1 = b - a;
    float k2 = c - a;
    float k3 = e - a;
    float k4 = a - b - c + d;
    float k5 = a - c - e + g;
    float k6 = a - b - e + f;
    float k7 = -a + b + c - d + e - f - g + h;

    // Compute the noise value
    float n = k0 + k1*u.x + k2*u.y + k3*u.z
        + k4*u.x*u.y + k5*u.y*u.z + k6*u.z*u.x + k7*u.x*u.y*u.z;

    // Derivatives
    float3 dn = float3(
        du.x * (k1 + k4 * u.y + k6 * u.z + k7 * u.y * u.z),
        du.y * (k2 + k5 * u.z + k4 * u.x + k7 * u.z * u.x),
        du.z * (k3 + k6 * u.x + k5 * u.y + k7 * u.x * u.y)
    );

    // Scale the result to [-1, 1]
    return float4(-1.f + (2.f * n), dn * 2.f);
}



// Fractal Brownian Motion
float fBm(float3 pos, noise_layer noise_layer)
{
    // Define angle and compute sine and cosine inside the function
    float angle = radians(noise_layer.weight_multiplier);
    float cosTheta = cos(angle);
    float sinTheta = sin(angle);

    // Define rotation matrix m3 and its inverse m3i
    float3x3 m3 = float3x3(
        cosTheta, -sinTheta, 0.0,
        sinTheta,  cosTheta, 0.0,
        0.0,       0.0,      1.0
    );

    float3x3 m3i = transpose(m3); // Inverse of rotation matrix

    
    // a - accumulate the noise value
    float noise_value = 0.f;
    // b - amplitude scaling factor
    float amplitude = 1.f;
    float frequency = noise_layer.frequency;
    // d - derivative sum
    float3 d = float3(0.f, 0.f, 0.f);

    for (int i=0; i<(noise_layer.octaves + 5); i++)
    {
        float4 n = noised(pos * frequency + noise_layer.center);
        d += n.yzw;
        noise_value += amplitude * n.x / (1.f + dot(d, d));
        // amplitude *= 0.5f;
        amplitude *= noise_layer.persistence;
        frequency *= noise_layer.roughness;
        pos = mul(m3, pos) * 2.f; 

        
        /*noise_value += v.x * amplitude;
        derivative_sum += amplitude * frequency * v.yzw;

        frequency *= noise_layer.roughness;
        amplitude *= noise_layer.persistence;*/
    }

    // clamp the noise value to the min value
    noise_value = max(0, noise_value - noise_layer.min_value);
    // scale the noise value by the strength
    noise_value *= noise_layer.strength;

    return noise_value;
}

float fBm2(float3 pos, noise_layer noise_layer)
{
    // Define angle and compute sine and cosine inside the function
    float angle = radians(noise_layer.weight_multiplier);
    float cosTheta = cos(angle);
    float sinTheta = sin(angle);

    // Define rotation matrix m3 and its inverse m3i
    float3x3 m3 = float3x3(
        cosTheta, -sinTheta, 0.0,
        sinTheta,  cosTheta, 0.0,
        0.0,       0.0,      1.0
    );

    float3x3 m3i = transpose(m3); // Inverse of rotation matrix


    
    float noise_value = 0;
    float frequency = noise_layer.frequency;
    float amplitude = 1.f;
    float weight = 1.f;
    float3 d = float3(0.f, 0.f, 0.f);

    // perform the noise calculation
    for (int i = 0; i < noise_layer.octaves; i++)
    {
        float4 n = 1.0f - abs(noised(pos * frequency + noise_layer.center));
        d += n.yzw;
        n.x *= n.x; // Square the value to make it more rigid
        n.x *= weight; // Multiply the value by the weight
        weight = clamp(n.x * noise_layer.weight_multiplier, 0, 1); // clamp the weight to [0, 1]
        
        noise_value += amplitude * n.x / (1.f + dot(d, d));
        frequency *= noise_layer.roughness; // scale the frequency by the roughness for the next octave
        amplitude *= noise_layer.persistence; // scale the amplitude by the persistence for the next octave
        pos = mul(m3, pos) * 2.f;
    }

    // clamp the noise value to the min value
    noise_value = max(0, noise_value - noise_layer.min_value);
    // scale the noise value by the strength
    noise_value *= noise_layer.strength;
    
    // return the noise value
    return noise_value;
}

float simple_fBm_c(float3 pos, noise_layer noise_layer)
{
    // int type; // 0 = Simple, 1 = Rigid
    // int enabled; // 0 = false, 1 = true
    // int use_first_layer_as_mask; // 0 = false, 1 = true
    // float strength;
    // int octaves;
    // float frequency;
    // float roughness;
    // float persistence;
    // float3 center;
    // float min_value;
    // float weight_multiplier; // Only used for Rigid Noise

    
    float G = pow(2.0f, -noise_layer.roughness); // Gain
    float amplitude = 1.0f; // Amplitude
    float frequency = noise_layer.frequency; // Frequency
    float normalization = 0.0f;
    float t_noise_value = 0.0f;

    for (int i = 0; i < noise_layer.octaves; i++)
    {
        // outputs single float value, for height of the noise (-1 to 1)
        float n = snoise(pos * frequency + noise_layer.center);

        // calculate derivatives
        float dx = snoise(pos + float3(0.01, 0, 0) * frequency + noise_layer.center).x - n;
        float dy = snoise(pos + float3(0, 0.01, 0) * frequency + noise_layer.center).x - n;
        float dz = snoise(pos + float3(0, 0, 0.01) * frequency + noise_layer.center).x - n;
        float3 dn = float3(dx, dy, dz) * 100.0f; // scale the derivative
        
        
        t_noise_value += n.x * amplitude;
        
        normalization += amplitude;
        amplitude *= G;
        frequency *= noise_layer.persistence;
    }
    t_noise_value /= normalization;
    
    // clamp the noise value to the min value
    t_noise_value = max(0, t_noise_value - noise_layer.min_value);
    t_noise_value = pow(abs(t_noise_value), noise_layer.center.x) * noise_layer.strength;
    
    return t_noise_value;
}

float rigid_fBm_c(float3 pos, noise_layer noise_layer)
{
    // int type; // 0 = Simple, 1 = Rigid
    // int enabled; // 0 = false, 1 = true
    // int use_first_layer_as_mask; // 0 = false, 1 = true
    // float strength;
    // int octaves;
    // float frequency;
    // float roughness;
    // float persistence;
    // float3 center;
    // float min_value;
    // float weight_multiplier; // Only used for Rigid Noise

    
    float G = pow(2.0f, -noise_layer.roughness); // Gain
    float amplitude = 1.0f; // Amplitude
    float frequency = noise_layer.frequency; // Frequency
    float normalization = 0.0f;
    float t_noise_value = 0.0f;

    for (int i = 0; i < noise_layer.octaves; i++)
    {
        // float4 n = noised(pos * frequency + noise_layer.center) * 0.5f + 0.5f; // Normalize the noise value to [0, 1]
        // float4 n = noised(pos * frequency + noise_layer.center);
        // float n = snoise(pos * frequency + noise_layer.center); // outputs single float value, for height of the noise (-1 to 1)
        float n = 1.f - abs(snoise(pos * frequency + noise_layer.center));

        // calculate derivatives
        float dx = snoise(pos + float3(0.01, 0, 0) * frequency + noise_layer.center).x - n;
        float dy = snoise(pos + float3(0, 0.01, 0) * frequency + noise_layer.center).x - n;
        float dz = snoise(pos + float3(0, 0, 0.01) * frequency + noise_layer.center).x - n;
        // scale the derivative
        float3 dn = float3(dx, dy, dz) * 100.0f;
        
        t_noise_value += amplitude * n / (1.f + dot(dn.xyz, dn.xyz));
        t_noise_value += n.x * amplitude;
        // t_noise_value += (n.x + 1.f) * 0.5f * amplitude;
        normalization += amplitude;
        amplitude *= G;
        frequency *= noise_layer.persistence;
    }
    t_noise_value /= normalization;

    // clamp the noise value to the min value
    t_noise_value = max(0, t_noise_value - noise_layer.min_value);
    t_noise_value = pow(abs(t_noise_value), noise_layer.weight_multiplier) * noise_layer.strength;
    
    return t_noise_value;
}