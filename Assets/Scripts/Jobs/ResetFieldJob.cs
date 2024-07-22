using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ResetFieldJob : IJobParallelFor
{
    public NativeArray<float> field_values;
    public void Execute(int index)
    {
        field_values[index] = 0;
    }
}
