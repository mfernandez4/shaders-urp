/*
 * This script is used to define the Planet class, which is used to create a procedural planet.
 * Responsible for generating the planet's mesh, and applying the appropriate materials.
 * Creates 6 terrain faces, telling each of them which direction they are facing.
 */

using UnityEngine;
using UnityEngine.Profiling;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 32; // Number of vertices along each edge of the terrain face
    public bool autoUpdate = true; // Automatically update the planet when settings are changed
    public bool useComputeShader = true; // Use a compute shader to generate the planet's mesh
    public ComputeShader noiseComputeShader; // Compute shader to use for generating the planet's mesh
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back } // Faces of the planet to render
    public FaceRenderMask faceRenderMask; // Which faces of the planet to render
    
    
    public SettingsShape settingsShape; // Shape settings for the planet
    public SettingsColor settingsColor; // Color settings for the planet
    
    [HideInInspector] public bool shapeSettingsFoldout; // Whether the shape settings are folded out in the inspector
    [HideInInspector] public bool colorSettingsFoldout; // Whether the color settings are folded out in the inspector


    private ShapeGenerator _shapeGenerator = new(); // Shape generator for the planet
    private ColorGenerator _colorGenerator = new(); // Color generator for the planet
    
    // Array of mesh filters, for displaying the planet's mesh
    [SerializeField, HideInInspector] 
    MeshFilter[] _meshFilters;
    TerrainFace[] _terrainFaces;
    

    void Initialize()
    {
        Profiler.BeginSample("Initialize/GeneratePlanet");
        _shapeGenerator.UpdateSettings(settingsShape);
        _colorGenerator.UpdateSettings(settingsColor);
        
        // Create a new array of mesh filters and terrain faces
        if (_meshFilters == null || _meshFilters.Length == 0)
        {
            _meshFilters = new MeshFilter[6];
        }
        _terrainFaces = new TerrainFace[6];
        
        // Create a new terrain face for each direction
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        
        for (int i = 0; i < 6; i++)
        {
            if (_meshFilters[i] == null)
            {
                // Create a new game object for the terrain face
                GameObject meshObj = new GameObject("mesh");
                // Set the parent of the terrain face to the planet
                meshObj.transform.parent = transform;

                // Add a mesh renderer
                meshObj.AddComponent<MeshRenderer>();
                // terrain face to the mesh filters array
                _meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                // Create a new mesh for the terrain face
                _meshFilters[i].sharedMesh = new Mesh();
            }
            _meshFilters[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial = settingsColor.planetMaterial;
            
            // Create a new terrain face, passing in the mesh, resolution, and direction
            _terrainFaces[i] = new TerrainFace(_shapeGenerator, _meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int) faceRenderMask - 1 == i;
            _meshFilters[i].gameObject.SetActive(renderFace);
        }
        Profiler.EndSample();
    }
    
    void GenerateMesh()
    {
        Profiler.BeginSample("GenerateMesh/GeneratePlanet");
        for (var index = 0; index < 6; index++)
        {
            // Skip the terrain face if it is not active
            if (!_meshFilters[index].gameObject.activeSelf) continue;
            
            // Construct the mesh for the terrain face
            var face = _terrainFaces[index];
            if (noiseComputeShader != null && useComputeShader)
            {
                face.ConstructMeshCompute(noiseComputeShader);
            }
            else
            {
                face.ConstructMesh();
            }
        }
        
        // Update the elevation of the planet
        _colorGenerator.UpdateElevation(_shapeGenerator.elevationMinMax);
        Debug.Log($"Elevation: min: {_shapeGenerator.elevationMinMax.Min}, max: {_shapeGenerator.elevationMinMax.Max}");
        Profiler.EndSample();
    }
    
    public void GenerateColors()
    {
        _colorGenerator.UpdateColors();

        for (var index = 0; index < 6; index++)
        {
            // Skip the terrain face if it is not active
            if (!_meshFilters[index].gameObject.activeSelf) continue;

            // Construct the mesh for the terrain face
            var face = _terrainFaces[index];
            // Update the UVs of the terrain face
            face.UpdateUVs(_colorGenerator);
        }
    }
    
    
    
    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColors();
    }
    
    public void OnShapeSettingsUpdated()
    {
        if (!autoUpdate) return;
        Initialize();
        GenerateMesh();
    }

    public void OnColorSettingsUpdated()
    {
        if (!autoUpdate) return;
        Initialize();
        GenerateColors();
    }
}
