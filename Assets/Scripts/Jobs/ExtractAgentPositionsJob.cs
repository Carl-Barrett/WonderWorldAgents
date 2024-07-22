using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct ExtractAgentPositionsJob : IJobParallelFor
{
    public NativeArray<Agent> agents;
    public NativeArray<float3> positions;

    public void Execute(int index)
    {
        positions[index] = agents[index].position;
    }
}
