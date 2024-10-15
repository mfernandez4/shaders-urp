/* TerrainFace.cs
 * This script is used to define the TerrainFace class, which is used to create a single terrain face for a procedural planet.
 * Responsible for generating a terrain face's mesh, and applying the appropriate materials.
 */

using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;


public class TerrainFace
{
    int Resolution;

    private ShapeGenerator shapeGenerator;
    Mesh _mesh;
    Vector3 _localUp;
    Vector3 _localForward;
    Vector3 _localRight;
    
    
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
    
    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        _mesh = mesh;
        this.Resolution = resolution;
        _localUp = localUp;
        // Calculate the local forward and right vectors
        _localRight = Vector3.Cross(localUp, Vector3.forward);
        if (_localRight == Vector3.zero)
        {
            _localRight = Vector3.Cross(localUp, Vector3.right);
        }
        _localForward= Vector3.Cross(localUp, _localRight);
    }

    public void ConstructMesh()
    {
        Profiler.BeginSample("ConstructMesh/TerrainFace");
        
        Vector2[] uv = _mesh.uv;
        // Array of vertices, with a length of Resolution squared
        Vector3[] vertices = new Vector3[Resolution * Resolution];
        // Array of triangles, with a length of 6 times Resolution squared
        int[] triangles = new int[(Resolution - 1) * (Resolution - 1) * 6];

        ConstructVerticesSequentially(ref vertices, ref triangles);
        
        // Assign the vertices and triangles to the mesh
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        
        // Recalculate the normals of the mesh to ensure lighting is correct
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        
        // Assign the UVs of the mesh
        _mesh.uv = uv;
        
        Profiler.EndSample();
    }

    private void ConstructVerticesSequentially(ref Vector3[] vertices, ref int[] triangles)
    {
        int triIndex = 0; // Index of the current triangle in the triangles array
        // Loop through each vertex in the vertices array
        for (int y = 0; y < Resolution; y++)
        {
            for (int x = 0; x < Resolution; x++)
            {
                // index of the current vertex in the vertices array
                int index = x + y * Resolution;
                // Calculate the percentage of the current x and y values, which tells us how far along the face we are in both directions
                Vector2 percent = new Vector2(x, y) / (Resolution - 1);
                // Calculate the point on the unit cube that this vertex corresponds to
                Vector3 pointOnUnitCube = 
                    _localUp 
                    + (_localRight * ((percent.x - 0.5f) * 2)) 
                    + (_localForward * ((percent.y - 0.5f) * 2));

                // Turn into a point on the unit sphere
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[index] = shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);
                
                /*
                 * The vertices are arranged in the following way:
                 * 0---0---0---0
                 * |  /|  /|  /|
                 * | / | / | / |
                 * |/  |/  |/  |
                 * 0---0---0---0
                 * (0)----(1)----(2)----(3)
                 *  | \    | \    | \    |
                 *  |  \   |  \   |  \   |
                 *  |   \  |   \  |   \  |
                 * (4)----(5)----(6)----(7)
                 */
                
                // If we are not at the edge of the face
                if (x == Resolution - 1 || y == Resolution - 1) continue;
                
                // Add the indices of the vertices that make up the current face
                // First triangle ---------------------
                triangles[triIndex] = index; // vertex 0
                triangles[triIndex + 1] = index + Resolution + 1; // vertex 5
                triangles[triIndex + 2] = index + Resolution; // vertex 4
                // Second triangle ---------------------
                triangles[triIndex + 3] = index; // vertex 0
                triangles[triIndex + 4] = index + 1; // vertex 1
                triangles[triIndex + 5] = index + Resolution + 1; // vertex 5
                triIndex += 6;
            }
        }
    }
    
    public void ConstructMeshCompute(ComputeShader computeShader)
    {
        Profiler.BeginSample("ConstructMeshCompute/TerrainFace");
        
        Vector2[] uv = _mesh.uv;
        // Array of vertices, with a length of Resolution squared
        Vector3[] vertices = new Vector3[Resolution * Resolution];
        // Array of triangles, with a length of 6 times Resolution squared
        int[] triangles = new int[(Resolution - 1) * (Resolution - 1) * 6];

        
        if (!computeShader) return;
        
        // Create a new array of noise layers to store the noise settings
        int numLayers = shapeGenerator.noiseLayer.Length;
        noise_layer[] noiseLayers = new noise_layer[numLayers];
        for (int i = 0; i < numLayers; i++)
        {
            int noiseType = shapeGenerator.settings.noiseLayers[i].noiseSettings.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;
            noiseLayers[i] = new noise_layer
            {
                type = noiseType,
                enabled = shapeGenerator.settings.noiseLayers[i].enabled ? 1 : 0,
                use_first_layer_as_mask = shapeGenerator.settings.noiseLayers[i].useFirstLayerAsMask ? 1 : 0,
                strength = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.strength,
                octaves = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.octaves,
                frequency = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.frequency,
                roughness = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.roughness,
                persistence = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.persistence,
                center = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.center,
                min_value = shapeGenerator.settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.minValue,
                weight_multiplier = shapeGenerator.settings.noiseLayers[i].noiseSettings.rigidNoiseSettings.weightMultiplier,
                _padding1 = 0  // Ensure alignment
            };
        }
        
        
        // Initialize buffers for vertices and triangles
        ComputeBuffer vertexBuffer = new ComputeBuffer(Resolution * Resolution, sizeof(float) * 3);
        ComputeBuffer triangleBuffer = new ComputeBuffer((Resolution - 1) * (Resolution - 1) * 6, sizeof(int));
        ComputeBuffer noiseLayerBuffer = new ComputeBuffer(shapeGenerator.noiseLayer.Length, Marshal.SizeOf(typeof(noise_layer)), ComputeBufferType.Structured);
        // ComputeBuffer minMaxBuffer = new ComputeBuffer(2, sizeof(float));
        
        // Set initial min/max values
        // minMaxBuffer.SetData(new[] {shapeGenerator.elevationMinMax.Min, shapeGenerator.elevationMinMax.Max});
        
        // Send data to the compute shader
        vertexBuffer.SetData(vertices);
        triangleBuffer.SetData(triangles);
        noiseLayerBuffer.SetData(noiseLayers);
        
        // Set the compute shader properties
        computeShader.SetBuffer(0, "vertices", vertexBuffer);
        computeShader.SetBuffer(0, "triangles", triangleBuffer);
        computeShader.SetBuffer(0, "noise_layers", noiseLayerBuffer);
        // computeShader.SetBuffer(0, "minMaxElevation", minMaxBuffer);
        computeShader.SetInt("resolution", Resolution);
        computeShader.SetInt("num_layers", shapeGenerator.noiseLayer.Length);
        computeShader.SetFloat("planet_radius", shapeGenerator.settings.planetRadius);
        // computeShader.SetVector("local_up", _localUp);
        computeShader.SetFloats("local_up", new[] {_localUp.x, _localUp.y, _localUp.z});
        computeShader.SetFloats("local_right", new[] {_localRight.x, _localRight.y, _localRight.z});
        computeShader.SetFloats("local_forward", new[] {_localForward.x, _localForward.y, _localForward.z});
        
        // Dispatch the compute shader
        int numThreadGroups = Mathf.CeilToInt(Resolution / 8.0f);
        computeShader.Dispatch(0, numThreadGroups, numThreadGroups, 1);
        
        // Get the data back from the compute shader
        vertexBuffer.GetData(vertices);
        triangleBuffer.GetData(triangles);
        
        float[] minMax = new float[2];
        // minMaxBuffer.GetData(minMax);
        // // Debug.Log($"Min Elevation: {minMax[0]}, Max Elevation: {minMax[1]}");

        (minMax[0], minMax[1]) = shapeGenerator.CalculateMinMaxElevation(vertices);
        shapeGenerator.elevationMinMax.AddValue(minMax[0]);
        shapeGenerator.elevationMinMax.AddValue(minMax[1]);
        
        // Release buffers
        vertexBuffer.Release();
        triangleBuffer.Release();
        noiseLayerBuffer.Release();
        // minMaxBuffer.Release();
        

        // Assign the vertices and triangles to the mesh
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        
        // Recalculate the normals of the mesh to ensure lighting is correct
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        _mesh.uv = uv;
        
        Profiler.EndSample();
    }


    public void UpdateUVs(ColorGenerator colorGenerator)
    {
        Vector2[] uv = new Vector2[Resolution * Resolution];

        // Loop through each uv in the uv array
        for (int y = 0; y < Resolution; y++)
        {
            for (int x = 0; x < Resolution; x++)
            {
                // index of the current vertex in the vertices array
                int index = x + y * Resolution;
                // Calculate the percentage of the current x and y values, which tells us how far along the face we are in both directions
                Vector2 percent = new Vector2(x, y) / (Resolution - 1);
                // Calculate the point on the unit cube that this vertex corresponds to
                Vector3 pointOnUnitCube =
                    _localUp
                    + (_localRight * ((percent.x - 0.5f) * 2))
                    + (_localForward * ((percent.y - 0.5f) * 2));

                // Turn into a point on the unit sphere
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                
                uv[index] = new Vector2( colorGenerator.BiomePercentFromPoint(pointOnUnitSphere), 0 );
            }
        }
        
        // Assign the UVs of the mesh
        _mesh.uv = uv;
    }
}
