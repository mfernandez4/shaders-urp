/*
 * TODO: 
 */

using UnityEngine;

public class ColorGenerator
{
    
    
    SettingsColor settings;
    Texture2D texture;
    private const int textureResolution = 50;
    INoiseFilter biomeNoiseFilter;
    
    
    public void  UpdateSettings(SettingsColor settings)
    {
        Debug.Log("Updating settings...");
        
        this.settings = settings;
        // get the number of biomes from the settings
        int numBiomes = settings.biomeColorSettings.biomes.Length;
        // Only update if the texture is valid and/or the height of the texture is not equal to the number of biomes
        if (texture == null || texture.height != numBiomes)
        {
            texture = new Texture2D(textureResolution, numBiomes);
        }
        
        // Create a new biome noise filter
        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColorSettings.noise);
    }
    
    
    public void UpdateElevation(MinMax elevationMinMax)
    {
        // Set the color of the planet to the color of the planet in the settings
        settings.planetMaterial.SetVector( "_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max) );
    }
    
    
    /// This function is used to get the biome percent from a point on the unit sphere. 
    /// A value of 0 means the point is at the first biome, and a value of 1 means the point is last biome.
    /// Everything between 0 and 1 are the other biomes.
    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        
        // Get the elevation of the point on the unit sphere, unit sphere goes from -1 to 1
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f; // Normalize the height to a value between 0 and 1
        // Add noise to the height percent, then offset/scale the noise value
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColorSettings.noiseOffset) * settings.biomeColorSettings.noiseStrength;
        
        float biomeIndex = 0;
        int numBiomes = settings.biomeColorSettings.biomes.Length;
        float blendRange = settings.biomeColorSettings.blendAmount / 2f + 0.001f; // Add a small value to avoid division by 0
        
        for (int i=0; i<numBiomes; i++)
        {
            // distance of the height percent from the start height of the biome 
            float dst = heightPercent - settings.biomeColorSettings.biomes[i].startHeight;
            // weight, depends on distance within the range of the blending distance
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }
        
        // remap the biome index to a value between 0 and 1, but if there's only one biome we don't want to divide by 0.
        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }
    
    
    public void UpdateColors()
    {
        if (texture == null)
        {
            Debug.Log($"Updating colors... Texture: {texture == null}");
            texture = new Texture2D(textureResolution, settings.biomeColorSettings.biomes.Length);
        }
        
        // Create an array of colors to store the gradient colors
        Color[] colors = new Color[texture.width * texture.height];
        int colIndex = 0;
        foreach (var biome in settings.biomeColorSettings.biomes)
        {
            // Loop through the pixels and set the color of the texture to the gradient color at that pixel
            for (int i = 0; i < textureResolution; i++)
            {
                // remap the value of i(index/pixel) to a value between 0 and 1 to map the pixel to the gradient
                Color gradientCol = biome.gradient.Evaluate(i / (textureResolution - 1f));
                Color tintCol = biome.tint;
                
                colors[colIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
                colIndex++;
            }
        }
        
        // Set the pixels of the texture to the colors array and apply the changes
        texture.SetPixels(colors);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}
