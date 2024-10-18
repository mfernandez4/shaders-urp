using UnityEngine;

[CreateAssetMenu(fileName = "CelestialBodySettings", menuName = "Celestial Body/CelestialBodySettings")]
public class CelestialBodySettings : ScriptableObject
{
    public CelestialBodyShape shape;
    public CelestialBodyShading shading;
}
