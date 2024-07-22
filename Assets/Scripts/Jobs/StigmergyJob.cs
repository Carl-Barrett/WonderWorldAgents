using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

//[BurstCompile]
public struct StigmergyJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<float> field_values;
    [ReadOnly] public NativeArray<float3> agent_positions;

    public float value;
    public float decay;

    public float3 offset;
    public float cell_size;

    public int x_res;
    public int y_res;
    public int z_res;


    public void Execute(int index)
    {
        var pos = agent_positions[index] + offset;
        
        var grid_x = (int)(pos.x / cell_size);
        var grid_y = (int)(pos.y / cell_size);
        var grid_z = (int)(pos.z / cell_size);

        int cell_id = grid_y * x_res * z_res + grid_z * x_res + grid_x;

        var cell_value = field_values[cell_id];
        cell_value += value;
        cell_value *= decay;
        field_values[cell_id] = cell_value;

        //Debug.Log(cell_value);
    }
}
