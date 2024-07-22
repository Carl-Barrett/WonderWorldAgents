using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct AttractForceJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Agent> agents;
    public NativeArray<float3> agent_forces;
    [ReadOnly][NativeDisableParallelForRestriction] public NativeArray<float3> attractor_positions;

    public int num_attractors;

    public void Execute(int index)
    {
        var agent = agents[index];
        var a_pos = agent.position;

        float3 closest = a_pos;
        float shortest_dist = math.INFINITY;

        for(int i=0; i<num_attractors; i++)
        {
            var position = attractor_positions[i];
            var diff = position - a_pos;
            if (math.length(diff) < shortest_dist)
            {
                closest = diff;
                shortest_dist = math.length(diff);
            }
        }

        agent_forces[index] = math.normalize(closest);

    }
}
