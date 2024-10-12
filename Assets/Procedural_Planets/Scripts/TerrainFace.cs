/*
 * This script is used to define the TerrainFace class, which is used to create a single terrain face for a procedural planet.
 * Responsible for generating a terrain face's mesh, and applying the appropriate materials.
 */

using UnityEngine;

public class TerrainFace
{
    int Resolution;

    private ShapeGenerator shapeGenerator;
    Mesh _mesh;
    Vector3 _localUp;
    Vector3 _localForward;
    Vector3 _localRight;
    
    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        _mesh = mesh;
        this.Resolution = resolution;
        _localUp = localUp;
        // Calculate the local forward and right vectors
        // _localRight = new Vector3(localUp.y, localUp.z, localUp.x);
        _localRight = Vector3.Cross(localUp, Vector3.forward);
        if (_localRight == Vector3.zero)
        {
            _localRight = Vector3.Cross(localUp, Vector3.right);
        }
        _localForward= Vector3.Cross(localUp, _localRight);
    }

    public void ConstructMesh()
    {
        // Array of vertices, with a length of Resolution squared
        Vector3[] vertices = new Vector3[Resolution * Resolution];
        // Array of triangles, with a length of 6 times Resolution squared
        int[] triangles = new int[(Resolution - 1) * (Resolution - 1) * 6];
        int triIndex = 0; // Index of the current triangle in the triangles array

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
        
        // Assign the vertices and triangles to the mesh
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        
        // Recalculate the normals of the mesh to ensure lighting is correct
        Vector3[] normals = new Vector3[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].normalized;
            
            // Calculate the tangents of the mesh
            Vector4 tangent;
            tangent.x = -normals[i].y;
            tangent.y = normals[i].z;
            tangent.z = normals[i].x;
            tangent.w = -1f;
            tangents[i] = tangent;
        }
        // _mesh.normals = normals;
        _mesh.tangents = tangents;
        _mesh.RecalculateNormals();
        
        
        // Assign the UVs of the mesh
        Vector2[] uv = new Vector2[vertices.Length];
        for (int y = 0; y < Resolution; y++)
        {
            for (int x = 0; x < Resolution; x++)
            {
                // Set the UVs of the mesh to be a percentage of the current x and y values, relative to the resolution.
                // This will ensure that the texture is mapped correctly to the mesh.
                uv[y * Resolution + x] = new Vector2((float)x / (Resolution - 1), (float)y / (Resolution - 1));
            }
        }
        _mesh.uv = uv;
    }
}
