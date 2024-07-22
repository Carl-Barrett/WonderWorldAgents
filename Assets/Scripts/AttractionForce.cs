using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class AttractionForce : AgentForce
{
    public List<Transform> attractors;
    private NativeArray<float3> job_attractor_positions;

    int num_attractors;


    public override void Start()
    {
        num_attractors = attractors.Count;
        job_attractor_positions = new NativeArray<float3>(num_attractors, Allocator.Persistent);

        for(int i=0; i<num_attractors; i++)
        {
            job_attractor_positions[i] = attractors[i].position;
        }
    }

    public override void CalculateForce()
    {
        if (!force_initialized) return;

        for (int i = 0; i < num_attractors; i++)
        {
            job_attractor_positions[i] = attractors[i].position;
        }

        var attractJob = new AttractForceJob
        {
            agents = job_agents,
            agent_forces = job_agent_force,
            attractor_positions = job_attractor_positions,

            num_attractors = num_attractors
        };

        attractJob.Schedule(num_agents, 128).Complete();
    }



    public override void OnDestroy()
    {
        base.OnDestroy();
        if (job_attractor_positions.IsCreated) job_attractor_positions.Dispose();
    }

}
