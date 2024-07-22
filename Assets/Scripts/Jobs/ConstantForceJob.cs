using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public struct ConstantForceJob : IJobParallelFor
{
    public NativeArray<float3> agent_forces;
    public float3 constant_force;

    public void Execute(int index)
    {
        agent_forces[index] = math.normalize(constant_force);
    }
}
