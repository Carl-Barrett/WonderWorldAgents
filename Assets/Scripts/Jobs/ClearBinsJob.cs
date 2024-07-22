using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ClearBinsJob : IJobParallelFor
{
    public NativeArray<int> bin_counters;
    public void Execute(int index)
    {
        bin_counters[index] = 0;
    }
}
