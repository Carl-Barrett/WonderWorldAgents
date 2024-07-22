using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public struct StigmergyGradientForceJob : IJobParallelFor
{
    public NativeArray<Agent> agents;
    [NativeDisableParallelForRestriction] public NativeArray<float> field;
    public NativeArray<float3> forces;

    public float3 offset;
    public int res_x;
    public int res_z;
    public int res_y;
    public float cell_size;

    public void Execute(int index)
    {
        var pos = agents[index].position  + offset;
        
        int y_coord = (int)(pos.y / cell_size);
        int z_coord = (int)(pos.z / cell_size);
        int x_coord = (int)(pos.x / cell_size);
        

        var field_index = y_coord * res_x * res_z + z_coord * res_x + x_coord;
        var scalar = field[field_index];

        var scalar_right = x_coord < res_x - 1 ? field[field_index + 1]: 0;
        var scalar_left = x_coord > 0 ? field[field_index - 1] : 0;
        var scalar_top = y_coord < res_y - 1 ? field[field_index + res_x * res_z] : 0;
        var scalar_bottom = y_coord > 0 ? field[field_index - res_x * res_z] : 0;
        var scalar_front = z_coord < res_z - 1 ? field[field_index + res_x] : 0;
        var scalar_back = z_coord > 0 ? field[field_index - res_x] : 0;

        float3 force = new float3(scalar_right - scalar_left, scalar_top - scalar_bottom, scalar_front - scalar_back);


        forces[index] = force;
    }
}
