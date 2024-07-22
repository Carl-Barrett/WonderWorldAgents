using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class StigmergyManager : MonoBehaviour
{
    [Header("Referenced Elements")]
    public AgentPopulation a_population;
    public AgentEnvironment a_environment;

    [Header("Stigmergy Settings")]
    public float chemical_value;
    [Range(0.800f,1.0f)]
    public float chemical_decay;

    // native arrays
    public NativeArray<float> job_scalar_field;
    public NativeArray<float> job_flipped_scalar_field;


    // private variables
    private int num_agents;
    [HideInInspector] public int num_cells;

    [HideInInspector] public int res_x;
    [HideInInspector] public int res_y;
    [HideInInspector] public int res_z;

    [HideInInspector] public float cell_size;
    private float3 offset;

    // Start is called before the first frame update
    void Start()
    {
        InitStigmergy();
        AllocateJobMemory();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            ResetField();
        }*/
    }
    private void LateUpdate()
    {
        WriteAgentValuesToField();
        FlipField();
    }

    private void OnDestroy()
    {
        ClearJobMemory();
    }

    #region Stigmergy Initialization

    private void InitStigmergy()
    {
        res_x = a_environment.x_res;
        res_y = a_environment.y_res;
        res_z = a_environment.z_res;

        offset = a_environment.axis_offset_raw;
        cell_size = a_environment.cell_size;

        num_cells = res_x * res_y * res_z;
        num_agents = a_population.num_agents;
    }

    public void ResetField()
    {
        var resetJob = new ResetFieldJob
        {
            field_values = job_scalar_field
        };
        resetJob.Schedule(num_cells, 128).Complete();
    }

    #endregion

    #region Jobs

    private void AllocateJobMemory()
    {
        job_scalar_field = new NativeArray<float>(num_cells, Allocator.Persistent);
        job_flipped_scalar_field = new NativeArray<float>(num_cells, Allocator.Persistent);
    }

    private void ClearJobMemory()
    {
        if (job_scalar_field.IsCreated) job_scalar_field.Dispose();
        if (job_flipped_scalar_field.IsCreated) job_flipped_scalar_field.Dispose();
    }

    private void WriteAgentValuesToField()
    {
        var WriteValuesJob = new StigmergyJob
        {
            agent_positions = a_population.job_agent_positions,
            field_values = job_scalar_field,

            value = chemical_value,
            decay = chemical_decay,

            offset = offset,
            cell_size = cell_size,

            x_res = res_x,
            y_res = res_y,
            z_res = res_z
        };

        WriteValuesJob.Schedule(num_agents, 128).Complete();
    }

    private void FlipField()
    {
        var flipJob = new FlipFieldJob
        {
            field_IN = job_scalar_field,
            field_OUT = job_flipped_scalar_field,

            res_x = res_x,
            res_y = res_y,
            res_z = res_z
        };

        flipJob.Schedule(num_cells, 128).Complete();
    }

    #endregion
}
