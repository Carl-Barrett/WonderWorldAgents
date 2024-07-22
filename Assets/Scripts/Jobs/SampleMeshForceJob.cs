using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public struct SampleMeshForceJob : IJobParallelFor
{
    public NativeArray<Agent> agents;
    [NativeDisableParallelForRestriction] public NativeArray<float3> field;
    public NativeArray<float3> forces;

    public float3 offset;
    public int res_x;
    public int res_z;
    public float cell_size;

    public void Execute(int index)
    {
        var pos = agents[index].position + offset;
        int y_coord = (int)(pos.y / cell_size);
        int z_coord = (int)(pos.z / cell_size);
        int x_coord = (int)(pos.x / cell_size);

        var field_index = y_coord * res_x * res_z + z_coord * res_x + x_coord;
        var force = field[field_index];
        forces[index] = force;
    }
}
