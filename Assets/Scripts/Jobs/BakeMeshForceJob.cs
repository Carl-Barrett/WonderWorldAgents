using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public struct BakeMeshForceJob : IJobParallelFor
{
    public NativeArray<float3> grid_points;
    public NativeArray<float3> field_points;
    [NativeDisableParallelForRestriction] public NativeArray<Vector3> mesh_vertices;
    

    public int vertex_count;
    public float cell_size;
    public float max_force;
    public float min_force;


    public void Execute(int index)
    {
        var pos = grid_points[index] + (-new float3(cell_size * 0.5f));
        float3 vector = float3.zero;
        float shortest_dist = math.INFINITY;
        float largest_dist = 0;

        for (int i=0; i<vertex_count; i++)
        {
            var vert = mesh_vertices[i];
            var diff = (float3)vert - pos;
            var dist = math.length(diff);

            if (dist < shortest_dist)
            {
                vector = diff;
                shortest_dist = dist;
            }

            if (dist > largest_dist)
            {
                largest_dist = dist;
            }
        }

        var magnitude = remap(shortest_dist, 0, largest_dist, max_force, min_force);

        field_points[index] = math.length(vector) > 0 ? math.normalize(vector) * magnitude : float3.zero;
    }

    public float remap( float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
