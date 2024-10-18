using UnityEngine;

[CreateAssetMenu (menuName = "Celestial Body/Moon/Moon Shape")]
public class MoonShape : CelestialBodyShape
{
    public NoiseSettings shapeNoise;
    public NoiseSettings ridgeNoise;
    public NoiseSettings ridgeNoise2;


    private ComputeBuffer _noiseSettingsBuffer;
    
    
    // Struct to store the noise layer settings
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct noise_layer {
        public int type; // 0 = Simple, 1 = Rigid   // 4 bytes
        public int enabled;                         // 4 bytes
        public int use_first_layer_as_mask;         // 4 bytes
        public float strength;                      // 4 bytes
        public int octaves;                         // 4 bytes
        public float frequency;                     // 4 bytes
        public float roughness;                     // 4 bytes
        public float persistence;                   // 4 bytes
        public Vector3 center;                      // 12 bytes
        public float min_value;                     // 4 bytes
        public float weight_multiplier;             // 4 bytes
        
        // Padding to align the struct to 16-byte boundaries
        public float _padding1;                    // 4 bytes
    };

    protected override void SetShapeData()
    {
        int filterType = shapeNoise.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;
        noise_layer shapeNoiseLayer = new noise_layer {
            type = filterType,
            enabled = 1,
            use_first_layer_as_mask = 0,
            strength = filterType == 0 ? shapeNoise.simpleNoiseSettings.strength : shapeNoise.rigidNoiseSettings.strength,
            octaves = filterType == 0 ? shapeNoise.simpleNoiseSettings.octaves : shapeNoise.rigidNoiseSettings.octaves,
            frequency = filterType == 0 ? shapeNoise.simpleNoiseSettings.frequency : shapeNoise.rigidNoiseSettings.frequency,
            roughness = filterType == 0 ? shapeNoise.simpleNoiseSettings.roughness : shapeNoise.rigidNoiseSettings.roughness,
            persistence = filterType == 0 ? shapeNoise.simpleNoiseSettings.persistence : shapeNoise.rigidNoiseSettings.persistence,
            center = filterType == 0 ? shapeNoise.simpleNoiseSettings.center : shapeNoise.rigidNoiseSettings.center,
            min_value = filterType == 0 ? shapeNoise.simpleNoiseSettings.minValue : shapeNoise.rigidNoiseSettings.minValue,
            weight_multiplier = shapeNoise.rigidNoiseSettings.weightMultiplier,
        };
        
        _noiseSettingsBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(noise_layer)), ComputeBufferType.Structured);
        heightMapCompute.SetBuffer(0, "noise_settings", _noiseSettingsBuffer);
        _noiseSettingsBuffer.SetData(new noise_layer[] { shapeNoiseLayer });
        
        
        base.SetShapeData();
    }

    public override void ReleaseBuffers()
    {
        base.ReleaseBuffers();
        _noiseSettingsBuffer?.Release();
    }
}
