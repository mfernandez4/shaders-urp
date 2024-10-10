using UnityEngine;

public class ShapeGenerator
{
    SettingsShape settings;
    
    public ShapeGenerator(SettingsShape settings)
    {
        this.settings = settings;
    }
    
    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
        return pointOnUnitSphere * settings.planetRadius;
    }
}
