using System;
using UnityEngine;


/*
    Responsible for the shading of a celestial body.
    This is paired with a specific CelestialBodyShape.
*/
public abstract class CelestialBodyShading : ScriptableObject
{
    public event System.Action OnSettingChanged;
    
    
    public bool randomize;
    public int seed;

    public Material terrainMaterial;
    
    public ComputeShader shadingDataCompute;

    
    protected Vector4[] CachedShadingData;
    private ComputeBuffer _shadingBuffer;
    
    
    
    public virtual void Initialize(CelestialBodyShape shape) {}


    // Generate Vector4[] of shading data. This is stored in mesh uvs and used to help shade the body
    public Vector4[] GenerateShadingData(ComputeBuffer vertexBuffer)
    {
        Debug.Log("Generating shading data");
        return new Vector4[vertexBuffer.count];
    }
    
    
    // Set shading properties on terrain
    public virtual void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
        Debug.Log("Setting terrain properties");
    }
    
    
    // Override this to set properties on the shadingDataCompute before it is run
    protected virtual void SetShadingDataComputeProperties () {

    }
    
    public virtual void ReleaseBuffers()
    {
        _shadingBuffer.Release();
    }


    public static void TextureFromGradient(ref Texture2D texture, int width, Gradient gradient,
        FilterMode filterMode = FilterMode.Bilinear)
    {
        Debug.Log("Creating texture from gradient");
    }
    

    protected virtual void OnValidate()
    {
        /*
        Shader activeShader = (shader) ? shader : Shader.Find ("Unlit/Color");
        if (material == null || material.shader != activeShader) {
            if (material == null) {
                material = new Material (activeShader);
            } else {
                material.shader = activeShader;
            }
        }
        */
        if (OnSettingChanged == null) return;
        OnSettingChanged ();
        
    }
}
