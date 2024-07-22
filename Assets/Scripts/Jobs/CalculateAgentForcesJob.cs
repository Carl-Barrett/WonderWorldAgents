using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct CalculateAgentForcesJob : IJobParallelFor
{
    public NativeArray<float3> agent_forces;
    public NativeArray<float3> overall_agent_forces;

    public float speed;
    public float delta_time;

    public void Execute(int index)
    {
        var f_part = agent_forces[index];
        var f_full = overall_agent_forces[index];
        f_full += f_part * speed * delta_time;
        overall_agent_forces[index] = f_full;
    }
}
