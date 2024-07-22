using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct AssignAgentsToBinsJob : IJobParallelFor
{
    public NativeArray<Agent> agents;
    [NativeDisableParallelForRestriction] public NativeArray<int> bins;
    [NativeDisableParallelForRestriction] public NativeArray<int> bin_counters;

    public float bin_size;
    public int bin_x_res;
    public int bin_z_res;
    public int bin_capacity;
    public float3 offset;

    public void Execute(int index)
    {
        var agent = agents[index];
        var pos = agent.position + offset;
        //Debug.Log(pos);

        var bin_x = (int)(pos.x / bin_size);
        var bin_y = (int)(pos.y / bin_size);
        var bin_z = (int)(pos.z / bin_size);

        int bin_id = bin_y * bin_x_res * bin_z_res + bin_z * bin_x_res + bin_x;
        //Debug.Log($"index {index} --- to bin {bin_id}");

        // update agent bin id
        agent.bin_id = bin_id;
        agents[index] = agent;

        // write agent id to corresponding bin
        int current_bin_count = bin_counters[bin_id];

        // ------ exit if max bin count has been reached
        if (current_bin_count >= bin_capacity) return;

        // ------ find bin array index in flat array
        int bin_array_index = bin_id * bin_capacity + current_bin_count;

        // ------ assign agent id to bin
        bins[bin_array_index] = agent.id;

        // ------ update the corresponding bin counter
        current_bin_count ++;
        bin_counters[bin_id] = current_bin_count;

    }
}
