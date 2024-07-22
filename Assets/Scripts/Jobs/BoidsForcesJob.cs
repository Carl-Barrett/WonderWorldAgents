using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


[BurstCompile]
public struct BoidsForcesJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<Agent> agents;
    [NativeDisableParallelForRestriction] public NativeArray<int> bins;
    [NativeDisableParallelForRestriction] public NativeArray<int> bin_counters;
    public NativeArray<float3> boids_forces;

    public int agents_per_bin;
    public int bin_x_res;
    public int bin_y_res;
    public int bin_z_res;
    public float bin_size;

    public float s_strength;
    public float c_strength;
    public float a_strength;

    public float s_thres;
    public float c_thres;
    public float a_thres;

    public float delta_time;

    // private variables

    private int current_bin;
    private int num_agents_in_bin;

    private int s_steps;
    private int c_steps;
    private int a_steps;

    private float3 sv;
    private float3 cv;
    private float3 av;
    private float3 av_pt;

    private Agent agent;
     

    public void Execute(int index)
    {
        // get agent bin
        agent = agents[index];
        current_bin = agent.bin_id;
        num_agents_in_bin = bin_counters[current_bin];

        // init forces and counters as zero
        sv = float3.zero;
        cv = float3.zero;
        av = float3.zero;
        av_pt = float3.zero;

        s_steps = 0;
        c_steps = 0;
        a_steps = 0;

        // check forces for agents in the same bin
        CheckAgentsInCurrentBin(current_bin, index);

        // if any of the thresholds is larger than the bin size then we will check agents in neighbouring bins as well
        if(a_thres > bin_size || s_thres > bin_size || c_thres > bin_size)
        {
            CheckAgentsInNeighbourBins(current_bin, index);
        }

        var force = CombinedBoidsForce(agent);
        //if (index == 5) Debug.Log(force);
        boids_forces[index] = force;

    }

    private void CheckAgentsInCurrentBin(in int bin_id,in int index)
    {
        int b_index;
        int a_index;

        for(int i=0; i<num_agents_in_bin; i++)
        {
            b_index = bin_id * agents_per_bin + i;
            a_index = bins[b_index];

            var agent_2 = agents[a_index];
            if (a_index == index) continue;

            CalculateBoidsForces(agent, agent_2, ref s_steps, ref c_steps, ref a_steps, ref sv, ref av_pt, ref av);

        }
    }

    private void CheckAgentsInNeighbourBins(in int bin_id, in int index)
    {
        var coords = ExtractBinCoords(bin_id);
        float yy;
        float zz;
        float xx;

        for(int y = -1; y<2; y++)
        {
            yy = coords.y + y;
            if (yy < 0 || yy >= bin_y_res) continue;

            for(int z = -1; z<2; z++)
            {
                zz = coords.z + z;
                if (zz < 0 || zz >= bin_z_res) continue;

                for (int x = -1; x<2; x++)
                {
                    xx = coords.x + x;
                    if (xx < 0 || xx >= bin_x_res) continue;

                    var n_bin_id =(int) (yy * bin_x_res * bin_z_res + zz * bin_x_res + xx);
                    if (n_bin_id == bin_id) continue;

                    // extract agents from bin
                    num_agents_in_bin = bin_counters[n_bin_id];

                    CheckAgentsInCurrentBin(n_bin_id, index);
                    
                }
            }
        }
    }

    private int3 ExtractBinCoords(in int bin_id)
    {
        var plane_res = bin_x_res * bin_z_res;
        var y = bin_id / plane_res;
        var mod = bin_id % plane_res;
        var z = mod / bin_x_res;
        var x = mod % bin_x_res;

        return new int3(x, y, z);

    }

    private void CalculateBoidsForces(in Agent agent_1, in Agent agent_2, ref int s_count, ref int c_count, ref int a_count, 
        ref float3 s_force, ref float3 c_point, ref float3 a_force)
    {
        var vec = agent_1.position - agent_2.position;
        var dist = math.length(vec);

        // 1. separation
        if(dist <= s_thres)
        {
            s_force += vec;
            s_count++;
        }

        // 2. cohesion
        if(dist <= c_thres)
        {
            c_point += agent_2.position;
            c_count++;
        }

        // 3. alignment
        if(dist<= a_thres)
        {
            if (math.length(agent_2.velocity) > 0)
            {
                a_force += math.normalize(agent_2.velocity);
                a_count++;
            }
        }
    }

    private Vector3 CombinedBoidsForce(in Agent agent)
    {
        //Debug.Log($" sep {sv} + steps {s_steps} , coh {cv} + av {av_pt} + a_pos {agent.position} + steps {c_steps}, al {av} + steps {a_steps} ");
        // 1. separation
        if (s_steps > 0)
        {
            sv /= s_steps;
        }
        // 2. cohesion
        if (c_steps > 0)
        {
            av_pt /= c_steps;
            cv = av_pt - agent.position;
        }
        // 3. separation
        if (a_steps > 0)
        {
            av /= a_steps;
        }

        sv = math.length(sv) > 0 ? math.normalize(sv) : float3.zero;
        cv = math.length(cv) > 0 ? math.normalize(cv) : float3.zero;
        av = math.length(av) > 0 ? math.normalize(av) : float3.zero;

        float3 boid_force = sv * s_strength// * delta_time
                          + cv * c_strength// * delta_time
                          + av * a_strength; //* delta_time;

        //Debug.Log( $" sep {sv} , coh {cv} + steps {c_steps}, al {av} ---> {math.length(boid_force)}");
        return boid_force;
    }
}
