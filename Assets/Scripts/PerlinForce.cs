using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public class PerlinForce : AgentForce
{
    [Header("Perlin Noise Settings")]
    public float noise_scale;
    public float noise_speed;

     public override void CalculateForce()
     {
        if (!force_initialized) return;
        //ScheduleForceClearJob();

        //Debug.Log("---------> calculating perlin force");
        var PerlinJob = new PerlinForceJob
         {
             agents = job_agents,
             agent_forces = job_agent_force,

             n_scale = noise_scale,
             n_speed = noise_speed,
             time = Time.time
         };
         PerlinJob.Schedule(num_agents, 128).Complete();

     }





}
