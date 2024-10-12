using UnityEngine;

public class ColorGenerator
{
    
    
    SettingsColor settings;
    Texture2D texture;
    private const int textureResolution = 50;
    
    public void  UpdateSettings(SettingsColor settings)
    {
        this.settings = settings;
        if (texture != null) return;
        texture = new Texture2D(textureResolution, 1);
    }
    
    public void UpdateElevation(MinMax elevationMinMax)
    {
        // Set the color of the planet to the color of the planet in the settings
        settings.planetMaterial.SetVector( "_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max) );
    }
    
    public void UpdateColors()
    {
        // Create an array of colors to store the gradient colors
        Color[] colors = new Color[textureResolution];
        
        // Loop through the texture resolution and set the color of the texture to the gradient color at that point
        for (int i = 0; i < textureResolution; i++)
        {
            colors[i] = settings.gradient.Evaluate(i / (textureResolution - 1f));
        }
        
        // Set the pixels of the texture to the colors array and apply the changes
        texture.SetPixels(colors);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}
