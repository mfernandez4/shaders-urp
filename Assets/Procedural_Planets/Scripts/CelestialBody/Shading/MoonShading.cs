using UnityEngine;

[CreateAssetMenu (menuName = "Celestial Body/Moon/Moon Shading")]
public class MoonShading : CelestialBodyShading
{
    
    
    public Color primaryColA = Color.white;
    public Color secondaryColA = Color.black;
    public Color primaryColB = Color.white;
    public Color secondaryColB = Color.black;
    public Color steepCol = Color.black;


    [Header("Shading Data")] 
    public NoiseSettings Noise;


    private ComputeBuffer _craterBuffer;
    private ComputeBuffer _pointsBuffer;
    private MoonShape _moonShape;

    public override void Initialize(CelestialBodyShape shape)
    {
        base.Initialize (shape);
        _moonShape = shape as MoonShape;
    }

    public override void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
        base.SetTerrainProperties(material, heightMinMax, bodyScale);

        if (CachedShadingData != null)
        {
            Debug.Log("Setting terrain properties");
        }
        else
        {
            Debug.LogError("CachedShadingData is null");
        }
    }

    public override void ReleaseBuffers()
    {
        base.ReleaseBuffers();
        _craterBuffer.Release();
        _pointsBuffer.Release();
    }
    
    protected override void OnValidate () {
        base.OnValidate ();
    }
}
