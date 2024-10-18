/* ShapeGenerator.cs
 * This script is used to define the ShapeGenerator class, which is used to generate the shape of a procedural planet.
 */

using UnityEngine;

public class ShapeGenerator
{
    public SettingsShape settings;
    public INoiseFilter[] noiseLayer;
    public MinMax elevationMinMax;
    
    public void UpdateSettings(SettingsShape settings)
    {
        this.settings = settings;
        noiseLayer = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseLayer.Length; i++)
        {
            noiseLayer[i] = NoiseFilterFactory.CreateNoiseFilter( settings.noiseLayers[i].noiseSettings);
        }
        
        // Create a new MinMax object to store the elevation min and max
        elevationMinMax = new MinMax();
    }
    
    public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;
        float elevation = 0;
        
        // This is to store the value of the first layer,
        // in case it is needed as a mask for other layers
        if (noiseLayer.Length > 0)
        {
            firstLayerValue = noiseLayer[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }
        
        for (var i = 1; i < noiseLayer.Length; i++)
        {
            if (!settings.noiseLayers[i].enabled) continue; // Skip the noise layer if it is not enabled
            
            // If the current noise layer is set to use the first layer as a mask, use the first layer value as the mask
            float mask = settings.noiseLayers[i].useFirstLayerAsMask ? firstLayerValue : 1;
            
            // Add the noise value to the overall elevation. Also apply the mask to the noise value.
            elevation += noiseLayer[i].Evaluate(pointOnUnitSphere) * mask;
        }
        
        elevationMinMax.AddValue(elevation);
        
        // Return the point on the unit sphere, scaled by the elevation
        // 'pointOnUnitSphere' is a vertex on the sphere
        return elevation;
    }
    
    public float GetScaledElevation(float unscaledElevation)
    {
        // Clamp the elevation to a minimum of 0 and the unscaled elevation
        float elevation = Mathf.Max(0, unscaledElevation);
        // Scale the elevation by the planet radius
        elevation = settings.planetRadius * (1 + elevation);
        return elevation;
    }
    
    public (float, float) CalculateMinMaxElevation(Vector3[] vertices) {
        float minElevation = float.MaxValue;
        float maxElevation = float.MinValue;

        foreach (var vertex in vertices) {
            float elevation = vertex.magnitude;
            if (elevation < minElevation) minElevation = elevation;
            if (elevation > maxElevation) maxElevation = elevation;
        }

        return (minElevation, maxElevation);
    }
}