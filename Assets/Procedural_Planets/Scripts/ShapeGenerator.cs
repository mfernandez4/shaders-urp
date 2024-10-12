using UnityEngine;

public class ShapeGenerator
{
    SettingsShape settings;
    INoiseFilter[] noiseFilters;
    
    public ShapeGenerator(SettingsShape settings)
    {
        this.settings = settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter( settings.noiseLayers[i].noiseSettings );
        }
    }
    
    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;
        float elevation = 0;
        
        // This is to store the value of the first layer,
        // in case it is needed as a mask for other layers
        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }
        
        for (var i = 1; i < noiseFilters.Length; i++)
        {
            if (!settings.noiseLayers[i].enabled) continue; // Skip the noise layer if it is not enabled
            
            // If the current noise layer is set to use the first layer as a mask, use the first layer value as the mask
            float mask = settings.noiseLayers[i].useFirstLayerAsMask ? firstLayerValue : 1;
            
            // Add the noise value to the overall elevation. Also apply the mask to the noise value.
            elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
        }

        return pointOnUnitSphere * (settings.planetRadius * (1 + elevation));
    }
}