using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class StigmergyGradientForce : AgentForce
{
    [Header("Referenced Elements")]
    public AgentEnvironment a_environment;
    public StigmergyManager a_stigmergy;

    // private variables
    private int field_res;
    private int res_x;
    private int res_y;
    private int res_z;
    private float cell_size;
    private int grid_res;

    private float3 offset;


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void CalculateForce()
    {
        if (!force_initialized) return;

        GetFieldGradient();
    }

    #region Initialiazation

    public void Init()
    {
        res_x = a_environment.x_res;
        res_y = a_environment.y_res;
        res_z = a_environment.z_res;

        field_res = a_environment.grid_resolution;
        offset = a_environment.axis_offset_raw;
        cell_size = a_environment.cell_size;
        grid_res = a_environment.grid_resolution;

    }


    #endregion

    #region Jobs

    public void GetFieldGradient()
    {
        var sampleGradient = new StigmergyGradientForceJob
        {
            agents = job_agents,
            forces = job_agent_force,
            field = a_stigmergy.job_scalar_field,

            res_x = res_x,
            res_y = res_y,
            res_z = res_z,
            cell_size = cell_size,
            offset = offset
        };

        sampleGradient.Schedule(num_agents, 128).Complete();
    }

    #endregion
}
