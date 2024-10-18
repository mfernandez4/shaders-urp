using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a spherical mesh generator.
/// </summary>
public class SphereMesh
{
    
    
    /// <summary>
    /// The vertices of the sphere mesh.
    /// </summary>
    public readonly Vector3[] Vertices;
    
    /// <summary>
    /// The triangles of the sphere mesh.
    /// </summary>
    public readonly int[] Triangles;
    
    
    // Internal:
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private int _numDivisions;
    private int _numVertsPerFace;
    
    
    // Indices of the vertex paris that make up the initial 12 faces of the octahedron
    private static readonly int[] vertexPairs =
    {
        0,1, 0,2, 0,3, 0,4,
        1,2, 2,3, 3,4, 4,1,
        5,1, 5,2, 5,3, 5,4,
    };
    // Indices of the edge triplets that make up the initial 8 faces
    static readonly int[] edgeTriplets =
    {
        0,1,4, 1,2,5, 2,3,6, 3,0,7, 
        8,9,4, 9,10,5, 10,11,6, 11,8,7,
    };
    // The six initial vertices
    static readonly Vector3[] baseVertices = { Vector3.up, Vector3.left, Vector3.back, Vector3.right, Vector3.forward, Vector3.down };

    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SphereMesh"/> class with the specified resolution.
    /// </summary>
    /// <param name="resolution">The resolution of the sphere mesh.</param>
    public SphereMesh(int resolution)
    {
        // Number of divisions per face --- e.g 
        _numDivisions = Mathf.Max(0, resolution);
        
        // Calculate the number of vertices per face ---
        // (_numDivisions + 3) -> number of vertices per face
        // (_numDivisions + 3) * (_numDivisions + 3) -> total number of vertices
        // (_numDivisions + 3) * (_numDivisions + 3) - (_numDivisions + 3) -> total number of vertices excluding the last row
        // min _numDivisions is 0; (0 + 3) * (0 + 3) - (0 + 3) / 2 :: 3 * 3 - 3 / 2 :: 9 - 3 / 2 :: 6 / 2 = 3 vertices per face minimum
        _numVertsPerFace = ( (_numDivisions + 3) * (_numDivisions + 3) - (_numDivisions + 3) ) / 2;
        
        // Calculate the total number of vertices in the sphere
        // _numVertsPerFace * 8 -> total number of vertices in the sphere (8 faces)
        // (_numDivisions + 2) * 12 -> number of vertices that are shared between faces (12 edges)
        // (_numDivisions + 2) * 12 - 6 -> number of vertices that are shared between 3 faces (6 vertices)
        // min; 3 * 8 - (0 + 2) * 12 - 6 :: 24 - 24 + 6 :: 6 vertices minimum
        int numVerts = _numVertsPerFace * 8 - (_numDivisions + 2) * 12 + 6;
        
        // Calculate number of triangles per face ( octahedron has 8 faces ) ( +1 in case of 0 divisions = minimum 1 triangle )
        int numTrisPerFace = (_numDivisions + 1) * (_numDivisions + 1);

        // Initialize the lists
        _vertices = new List<Vector3>(numVerts);
        _triangles = new List<int>(numTrisPerFace * 8 * 3);
        
        _vertices.AddRange(baseVertices); // Add the initial 6 vertices
        
        // Create 12 edges, with N vertices added along them (N = _numDivisions)
        Edge[] edges = new Edge[12];
        for (int i = 0; i < vertexPairs.Length; i += 2)
        {
            Vector3 startVertex = _vertices[vertexPairs[i]];
            Vector3 endVertex = _vertices[vertexPairs[i + 1]];
            
            // Create the edge
            int[] edgeVertexIndices = new int[_numDivisions + 2];
            edgeVertexIndices[0] = vertexPairs[i];
            
            // Add vertices along the edge
            for (int divisionIndex = 0; divisionIndex < _numDivisions; divisionIndex++)
            {
                float t = (divisionIndex + 1) / (float)(_numDivisions + 1); // 0.0 - 1.0;
                edgeVertexIndices[divisionIndex + 1] = _vertices.Count; // Index of the new vertex
                _vertices.Add(Vector3.Slerp(startVertex, endVertex, t)); // Add the new vertex, interpolated between the start and end vertices
            }
            edgeVertexIndices[_numDivisions + 1] = vertexPairs[i + 1]; // Add the end vertex
            int edgeIndex = i / 2; // Index of the edge
            edges[edgeIndex] = new Edge(edgeVertexIndices); // Add the edge to the list
        }
        
        // Create the faces
        for (int i = 0; i < edgeTriplets.Length; i += 3)
        {
            int faceIndex = i / 3; // Index of the face
            bool reverse = faceIndex >= 4; // Reverse the order of the vertices for the second half of the faces
            CreateFace(
                edges[edgeTriplets[i]], // Edge 1
                edges[edgeTriplets[i + 1]], // Edge 2
                edges[edgeTriplets[i + 2]], // Edge 3
                reverse // Whether to reverse the order of the vertices
            );
        }
        
        // Copy the vertices and triangles to the readonly arrays
        Vertices = _vertices.ToArray();
        Triangles = _triangles.ToArray();
    }

    
    /// <summary>
    /// Creates a face of the sphere mesh.
    /// </summary>
    /// <param name="sideA">The first edge of the face.</param>
    /// <param name="sideB">The second edge of the face.</param>
    /// <param name="bottom">The bottom edge of the face.</param>
    /// <param name="reverse">Whether to reverse the order of the vertices.</param>
    private void CreateFace(Edge sideA, Edge sideB, Edge bottom, bool reverse)
    {
        int numPointsInEdge = sideA.vertexIndices.Length;
        var vertexMap = new List<int>(_numVertsPerFace);
        vertexMap.Add(sideA.vertexIndices[0]); // top of the triangle
        
        // Create the vertices of the face
        for (int i = 1; i < numPointsInEdge - 1; i++)
        {
            // Side A vertex
            vertexMap.Add(sideA.vertexIndices[i]);
            
            // Add vertices between sideA and sideB
            Vector3 sideAVertex = _vertices[sideA.vertexIndices[i]];
            Vector3 sideBVertex = _vertices[sideB.vertexIndices[i]];
            int numInnerPoints = i - 1;

            for (int j = 0; j < numInnerPoints; j++)
            {
                float t = (j + 1f) / (numInnerPoints + 1f);
                vertexMap.Add(_vertices.Count);
                // Add the new vertex along the edge of sideA and sideB
                _vertices.Add(Vector3.Slerp(sideAVertex, sideBVertex, t));
            }
            
            // Side B vertex
            vertexMap.Add(sideB.vertexIndices[i]);
        }
        
        // Add the bottom edge vertices
        for (int i = 0; i < numPointsInEdge; i++)
        {
            vertexMap.Add(bottom.vertexIndices[i]);   
        }
        
        // Triangulate the face
        int numRows = _numDivisions + 1;
        for (int row = 0; row < numRows; row++)
        {
            // vertices down left edge follow quadratic sequence: 0, 1, 3, 6, 10, 15, ...
            // the nth term can be calculated with: (n^2 - n) / 2
            int topVertex = ((row+1) * (row+1) - row - 1) / 2;
            int bottomVertex = ((row+2) * (row+2) - row - 2) / 2;

            int numTrianglesInRow = 1 + 2 * row;
            for (int column = 0; column < numTrianglesInRow; column++)
            {
                int v0,v1,v2; // Triangle vertices

                if (column % 2 == 0)
                {
                    v0 = topVertex;
                    v1 = bottomVertex + 1;
                    v2 = bottomVertex;
                    topVertex++;
                    bottomVertex++;
                }
                else
                {
                    v0 = topVertex;
                    v1 = bottomVertex;
                    v2 = topVertex - 1;
                }
                
                _triangles.Add(vertexMap[v0]);
                _triangles.Add(vertexMap[reverse ? v2 : v1]);
                _triangles.Add(vertexMap[reverse ? v1 : v2]);
            }
        }
    }
    
    
    /// <summary>
    /// Represents an edge of the sphere mesh.
    /// </summary>
    public class Edge {
        
        
        /// <summary>
        /// The indices of the vertices that make up the edge.
        /// </summary>
        public int[] vertexIndices;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> class with the specified vertex indices.
        /// </summary>
        /// <param name="vertexIndices">The indices of the vertices that make up the edge.</param>
        public Edge (int[] vertexIndices) {
            this.vertexIndices = vertexIndices;
        }
    }

}
