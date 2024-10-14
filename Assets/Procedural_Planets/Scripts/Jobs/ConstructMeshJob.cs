using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

/*
 * This class is responsible for creating a job to construct a mesh for a terrain face using the Unity Job System.
 */
struct ConstructMeshJob : IJobParallelFor
{
    [ReadOnly] public int Resolution;
    [ReadOnly] public Vector3 LocalUp, LocalRight, LocalForward;
    public NativeArray<Vector3> Vertices;
    public NativeArray<int> Triangles;
    
    public void Execute(int index)
    {
        // Calculate the x and y values of the current vertex
        int x = index % Resolution;
        int y = index / Resolution;
        
        // Calculate the percentage of the current x and y values, which tells us how far along the face we are in both directions
        Vector2 percent = new Vector2(x, y) / (Resolution - 1);
        
        // Calculate the point on the unit cube that this vertex corresponds to
        Vector3 pointOnUnitCube = 
            LocalUp 
            + (LocalRight * ((percent.x - 0.5f) * 2)) 
            + (LocalForward * ((percent.y - 0.5f) * 2));
        
        // Turn into a point on the unit sphere
        Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
        
        

    }
}
