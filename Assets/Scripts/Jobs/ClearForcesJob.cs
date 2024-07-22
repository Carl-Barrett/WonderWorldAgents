using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ClearForcesJob : IJobParallelFor
{
    public NativeArray<float3> forces;
    public void Execute(int index)
    {
        forces[index] = float3.zero; 
    }
}
