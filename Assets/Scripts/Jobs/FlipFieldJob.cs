using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct FlipFieldJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> field_IN;
    [NativeDisableParallelForRestriction] public NativeArray<float> field_OUT;

    public int res_x;
    public int res_y;
    public int res_z;
  


    public void Execute(int index)
    {
        var value = field_IN[index];

        int coord_y = index / (res_x * res_z);
        int mod = index % (res_x * res_z);
        int coord_z = mod / res_x;
        int coord_x = mod % res_x;

        int new_y = res_y - 1 - coord_y;
        int new_z = res_z - 1 - coord_z;
        int new_x = res_x - 1 - coord_x;

        //var new_index = new_y * res_z * res_x + coord_z * res_x + new_x;
        var new_index = coord_z * res_z * res_x + coord_y * res_x + coord_x;

        field_OUT[new_index] = value;

    }
}
