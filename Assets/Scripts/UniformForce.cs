using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

public class UniformForce : AgentForce
{
    public float3 force;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void CalculateForce()
    {
        if (!force_initialized) return;

        var ApplyConstantJob = new ConstantForceJob
        {
            agent_forces = job_agent_force,
            constant_force = force
        };

        ApplyConstantJob.Schedule(num_agents, 128).Complete();
    }
}
