/*
 * This script is used to define the Planet class, which is used to create a procedural planet.
 * Responsible for generating the planet's mesh, and applying the appropriate materials.
 * Creates 6 terrain faces, telling each of them which direction they are facing.
 */

using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 32;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask;
    
    
    public SettingsShape settingsShape;
    public SettingsColor settingsColor;
    
    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;


    private ShapeGenerator _shapeGenerator = new();
    private ColorGenerator _colorGenerator = new();
    
    // Array of mesh filters, for displaying the planet's mesh
    [SerializeField, HideInInspector] 
    MeshFilter[] _meshFilters;
    TerrainFace[] _terrainFaces;
    

    void Initialize()
    {
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

    void GenerateMesh()
    {
        for (var index = 0; index < _terrainFaces.Length; index++)
        {
            // Skip the terrain face if it is not active
            if (!_meshFilters[index].gameObject.activeSelf) continue;
            
            // Construct the mesh for the terrain face
            var face = _terrainFaces[index];
            face.ConstructMesh();
        }
        
        // Update the elevation of the planet
        _colorGenerator.UpdateElevation(_shapeGenerator.elevationMinMax);
        Debug.Log($"Elevation: min: {_shapeGenerator.elevationMinMax.Min}, max: {_shapeGenerator.elevationMinMax.Max}");
    }
    
    void GenerateColors()
    {
        _colorGenerator.UpdateColors();
    }
}
