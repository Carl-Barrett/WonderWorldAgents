using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;


public class BoidsForce : AgentForce
{
    [Header("Force Strengths")]
    public float cohesion_strength;
    public float separation_strength;
    public float alignment_strength;

    [Header("Force Thresholds")]
    public float cohesion_threshold;
    public float separation_threshold;
    public float alignment_threshold;

    /*[Header("Force Speeds")]
    public float cohesion_speed;
    public float separation_speed;
    public float alignment_speed;*/


    public override void CalculateForce()
    {
        if (!force_initialized) return;

        var BoidsForce = new BoidsForcesJob
        {
            agents = job_agents,
            boids_forces = job_agent_force,
            bins = population.job_bins,
            bin_counters = population.job_bin_counters,

            agents_per_bin = population.num_agents_per_bin,
            bin_x_res = population.bin_x_res,
            bin_y_res = population.bin_y_res,
            bin_z_res = population.bin_z_res,
            bin_size = population.bin_cell_size,

            s_strength = separation_strength,
            c_strength = cohesion_strength,
            a_strength = alignment_strength,

            s_thres = separation_threshold,
            c_thres = cohesion_threshold,
            a_thres = alignment_threshold,

            delta_time = Time.deltaTime
        };

        BoidsForce.Schedule(num_agents, 128).Complete();
    }
}
