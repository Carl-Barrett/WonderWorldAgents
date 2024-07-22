using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class AgentPopulation : MonoBehaviour
{   
    [Header("Population Settings")]
    public int num_agents;

    public bool PLAY_AGENTS;

    [Header("Generation Settings")]
    public GenerationType generation_type;
    public BoxCollider generation_region;
    public GameObject mesh_region;

    [Header("Referenced Elements")]
    public AgentEnvironment a_environment;
    public GameObject agent_object;

    [Header("System Forces")]
    public SystemForce[] forces;

    [Header("Population Rendering")] [Tooltip("The mesh to be instanced")]
    public bool display_agents; 
    public Mesh instanceMesh;
    public float agent_scale;

    [Tooltip("The material with which to render the instanced meshes")]
    public Material instanceMaterial;

    [Tooltip("The submesh of the original mesh to render")]
    private int subMeshIndex = 0;



    // Native Arrays
    private NativeArray<Agent> job_agents;
    public NativeArray<int> job_bins;
    public NativeArray<int> job_bin_counters;
    private NativeArray<float3> job_agent_overall_forces;
    public NativeArray<float3> job_agent_positions;

    // private variables
    private int num_bins;
    private int num_cells;
    [HideInInspector] public int num_agents_per_bin;

    [HideInInspector] public int bin_x_res;
    [HideInInspector] public int bin_z_res;
    [HideInInspector] public int bin_y_res;
    [HideInInspector] public int bin_cell_size;
    private Vector3 axis_offset;

    private Vector3 min_pt;
    private Vector3 max_pt;
    private Vector3 env_min_pt;
    private Vector3 env_max_pt;

    private int num_forces;
    private Unity.Mathematics.Random random;

    // The total number of particles
    private int instanceCount = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;

    // GPU Buffers
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    // Instanced Shader Arguments array
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private Mesh mesh;
    [HideInInspector] public Vector3[] mesh_verts;
    private int num_verts;


    #region MonoBehaviour

    // Start is called before the first frame update
    void Start()
    {
        Init();
        AllocateJobMemory();
        InitPopulation();

        InitializePopulationForces();

        InitInstancing();

        //CreateAgentObjects();
    }

    // Update is called once per frame
    void Update()
    {

        if (PLAY_AGENTS == false)

        {
            return;
        }
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPopulation();
        }*/

        AssignToBins();
        UpdatePopulationForces();
        MoveAgents();

        ClearAgentForces();

        

        //UpdateAgentObjects();
    }

    private void LateUpdate()
    {
        
        if (PLAY_AGENTS == false)

        {
            return;
        }
        // Update the Buffers with the new data for this frame
        positionBuffer.SetData(job_agent_positions);

        if (display_agents == false)
        {
            return;
        }
        // Pass the Buffers to the shader
        instanceMaterial.SetBuffer("_positionBuffer", positionBuffer);
        instanceMaterial.SetFloat("_Scale", agent_scale);

        // Render instance meshes
        Graphics.DrawMeshInstancedIndirect
            (
            instanceMesh,
            subMeshIndex,
            instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(1000.0f, 1000.0f, 1000.0f)),
            argsBuffer,
            0,
            new MaterialPropertyBlock(),
            UnityEngine.Rendering.ShadowCastingMode.Off);
    }

    private void OnDestroy()
    {
        ClearJobMemory();
    }

    #endregion

    #region Initialization

    private void Init()
    {
        num_cells = a_environment.grid_resolution;
        num_bins = a_environment.bin_resolution;
        num_agents_per_bin = a_environment.agents_per_bin;

        env_min_pt = a_environment.min_pt;
        env_max_pt = a_environment.max_pt;

        switch (generation_type)
        {
            case GenerationType.GenerateFromRegion:
                min_pt = generation_region.bounds.min;
                max_pt = generation_region.bounds.max;
                break;
            case GenerationType.GenerateFromMesh:
                var bounds = mesh_region.GetComponent<Collider>().bounds;
                min_pt = bounds.min;
                max_pt = bounds.max;
                break;
        }
       

        axis_offset = a_environment.axis_offset_raw;
        bin_cell_size = a_environment.bin_cell_size;
        bin_x_res = a_environment.x_bin_res;
        bin_z_res = a_environment.z_bin_res;
        bin_y_res = a_environment.y_bin_res;

        num_forces = forces.Length;
    }
    public void StartSimulation()
    {
        PLAY_AGENTS = true;
    }

    

    #endregion

    #region Population Management
    private void InitPopulation()
    {
        if (mesh_region != null) GetGenMeshData();

        float3 pos = float3.zero;

        for(int i=0; i<num_agents; i++)
        {
            // create random position
            switch (generation_type)
            {
                case GenerationType.GenerateFromRegion:
                    pos = CreateRandomPositionInRegion();
                    break;
                case GenerationType.GenerateFromMesh:
                    pos = CreateRandomPositionFromMesh();
                    break;
                default:
                    CreateRandomPositionInRegion();
                    break;
            }
            
            // init agent to random position
            var agent = job_agents[i];
            agent.id = i;
            agent.position = pos;
            agent.prev_position = pos;
            agent.velocity = float3.zero;
            agent.bin_id = 0;
            job_agents[i] = agent;
            //job_agent_overall_forces[i] = float3.zero;
        }

        
        
    }
    
    public void DisplayPopulation(bool display)
    {
        display_agents = display; 
    }

    public void ResetPopulation()
    {
        InitPopulation();
        ClearAgentForces();
    }

    private float3 CreateRandomPositionInRegion()
    {
        var x = UnityEngine.Random.Range(min_pt.x, max_pt.x);
        var y = UnityEngine.Random.Range(min_pt.y, max_pt.y);
        var z = UnityEngine.Random.Range(min_pt.z, max_pt.z);

        return new float3(x, y, z);
    }

    private void GetGenMeshData()
    {
        mesh = mesh_region.GetComponent<MeshFilter>().sharedMesh;
        mesh_verts = mesh.vertices;
        num_verts = mesh_verts.Length;
    }

    private float3 CreateRandomPositionFromMesh()
    {
        var random_id = (int)UnityEngine.Random.Range(0, num_verts);
        return mesh_region.transform.TransformPoint( mesh_verts[random_id]) ;
    }

    private void InitializePopulationForces()
    {
        for (int i = 0; i < num_forces; i++)
        {
            if (forces[i].enabled)
            {
                forces[i].agent_force.strength = forces[i].agent_force.OVERALL_STRENGTH;// force_strength;
                forces[i].agent_force.num_agents = num_agents;
                forces[i].agent_force.population = this;               
                forces[i].agent_force.InitForce();
            }
        }
    }

    private void UpdatePopulationForces()
    {
        for (int i = 0; i < num_forces; i++)
        {
            if (forces[i].enabled)
            {
                forces[i].agent_force.strength = forces[i].agent_force.OVERALL_STRENGTH;
                forces[i].agent_force.job_agents = job_agents;
                forces[i].agent_force.population = this;

                forces[i].agent_force.CalculateForce();

                ScheduleForceAdditionJob(forces[i].agent_force.job_agent_force, forces[i].agent_force.OVERALL_STRENGTH);
            }
        }

        
    }

    private void MoveAgents()
    {
        ScheduleAgentMove();
        SchedulePositionExtractionJob();
    }

    private void ClearAgentForces()
    {
        ScheduleClearAgentForce();
    }

    private void AssignToBins()
    {
        ScheduleBinCleaning();
        ScheduleBinAssignmentJob();
    }

    #endregion



    #region Jobs

    private void AllocateJobMemory()
    {
        job_agents = new NativeArray<Agent>(num_agents, Allocator.Persistent);
        job_bins = new NativeArray<int>(num_bins * num_agents_per_bin, Allocator.Persistent);
        job_bin_counters = new NativeArray<int>(num_bins, Allocator.Persistent);
        job_agent_overall_forces = new NativeArray<float3>(num_agents, Allocator.Persistent);
        job_agent_positions = new NativeArray<float3>(num_agents, Allocator.Persistent);
    }

    private void ClearJobMemory()
    {
        if (job_agents.IsCreated) job_agents.Dispose();
        if (job_bins.IsCreated) job_bins.Dispose();
        if (job_bin_counters.IsCreated) job_bin_counters.Dispose();
        if (job_agent_overall_forces.IsCreated) job_agent_overall_forces.Dispose();
        if (job_agent_positions.IsCreated) job_agent_positions.Dispose();
    }

    private void ScheduleBinCleaning()
    {
        var BinCleanJob = new ClearBinsJob
        {
            bin_counters = job_bin_counters
        };

        BinCleanJob.Schedule(num_bins, 128).Complete();
    }

    private void ScheduleBinAssignmentJob()
    {
        var BinAssignJob = new AssignAgentsToBinsJob
        {
            agents = job_agents,
            bins = job_bins,
            bin_counters = job_bin_counters,
            bin_size = bin_cell_size,
            bin_x_res = bin_x_res,
            bin_z_res = bin_z_res,
            bin_capacity = num_agents_per_bin,
            offset = axis_offset
        };
        BinAssignJob.Schedule(num_agents, 128).Complete();
    }

    private void ScheduleForceAdditionJob(NativeArray<float3> forces, float strength)
    {
        var ForceAdditionJob = new CalculateAgentForcesJob
        {
            agent_forces = forces,
            overall_agent_forces = job_agent_overall_forces,

            speed = strength,
            delta_time = Time.deltaTime

        };

        ForceAdditionJob.Schedule(num_agents, 128).Complete();
    }

    private void ScheduleAgentMove()
    {
        random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100));
        var AgentMoveJob = new MoveAgentsJob
        {
            agents = job_agents,
            overall_force = job_agent_overall_forces,

            gen_min = min_pt,
            gen_max = max_pt,
            min = env_min_pt,
            max = env_max_pt,
            random = random
        };
        AgentMoveJob.Schedule(num_agents, 128).Complete();
    }

    private void ScheduleClearAgentForce()
    {
        var clearForce = new ClearForcesJob
        {
            forces = job_agent_overall_forces
        };
        clearForce.Schedule(num_agents, 128).Complete();
    }

    private void SchedulePositionExtractionJob()
    {
        var extractpositionsJob = new ExtractAgentPositionsJob
        {
            agents = job_agents,
            positions = job_agent_positions
        };

        extractpositionsJob.Schedule(num_agents, 128).Complete();
    }


    #endregion

    #region Rendering

    private void InitInstancing()
    {
        instanceCount = num_agents;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        // A float3 has size of 3 x 4bytes = 12 bytes
        positionBuffer = new ComputeBuffer(instanceCount, 12);
        

        // Set the Buffer data and then pass the buffer to the shader
        positionBuffer.SetData(job_agent_positions);
        instanceMaterial.SetBuffer("_positionBuffer", positionBuffer);


        // Update the Indirect arguments
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(0);
            args[3] = (uint)instanceMesh.GetBaseVertex(0);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
    }

    #endregion


    #region Testing

    private void CreateAgentObjects()
    {
        for(int i=0; i<num_agents; i++)
        {
            //GameObject agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject agent = Instantiate(agent_object);

            agent.name = "agent_" + i.ToString();
            agent.transform.position = job_agents[i].position;
            //agent.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            agent.transform.SetParent(transform);
        }
    }

    private void UpdateAgentObjects()
    {
        for (int i = 0; i < num_agents; i++)
        {
            var agent = transform.GetChild(i);
            agent.transform.position = job_agents[i].position;
        }
    }

    #endregion
}



[System.Serializable]
public enum GenerationType
{
    GenerateFromRegion,
    GenerateFromMesh
}
