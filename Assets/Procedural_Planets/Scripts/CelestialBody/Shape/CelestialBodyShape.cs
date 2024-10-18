using UnityEngine;


public abstract class CelestialBodyShape : ScriptableObject
{
    public event System.Action OnSettingChanged;
    
    
    public bool randomize;
    public int seed;
    public ComputeShader heightMapCompute;

    public bool perturbVertices;
    public ComputeShader perturbCompute;
    [Range(0, 1)] public float perturbStrength = 0.69f;


    private ComputeBuffer heightBuffer;


    public virtual float[] CalculateHeights(ComputeBuffer vertexBuffer)
    {
        Debug.Log("Calculating heights...");
        // Debug.Log( System.Environment.StackTrace );
        
        // Set data
        SetShapeData();
        heightMapCompute.SetInt("num_vertices", vertexBuffer.count);
        heightMapCompute.SetBuffer(0, "vertices", vertexBuffer);
        
        // --- Create height buffer
        // Calculate the stride of the buffer (size of a float)
        int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(float));
        heightBuffer = new ComputeBuffer(vertexBuffer.count, stride);
        heightMapCompute.SetBuffer(0, "heights", heightBuffer);
        
        // --- Dispatch compute shader
        uint x, y, z; // thread group sizes
        heightMapCompute.GetKernelThreadGroupSizes(0, out x, out y, out z);
        Vector3Int numThreadGroups = new Vector3Int(
            Mathf.CeilToInt( vertexBuffer.count / (float) x ),
            Mathf.CeilToInt(1 / (float) y ),
            Mathf.CeilToInt(1 / (float) z )
        );
        
        Debug.Log("Dispatching compute shader");
        heightMapCompute.Dispatch(0, numThreadGroups.x, numThreadGroups.y, numThreadGroups.z);
        
        // --- Read back data
        var heights = new float[vertexBuffer.count];
        heightBuffer.GetData(heights);
        return heights;
    }

    public virtual void ReleaseBuffers()
    {
        heightBuffer?.Release();
    }

    protected virtual void SetShapeData()
    {
        Debug.Log("Setting shape data...");
    }

    protected virtual void OnValidate()
    {
        OnSettingChanged?.Invoke();
    }
}