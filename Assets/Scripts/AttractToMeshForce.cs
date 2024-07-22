using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class AttractToMeshForce : AgentForce
{
    [Header("Referenced Elements")]
    public GameObject mesh_object;
    public AgentEnvironment a_environment;

    [Header("Attraction Settings")]
    public float max_strenth;
    public float min_strength;

    // native arrays
    public NativeArray<float3> job_mesh_field;
    public NativeArray<float3> job_grid_pts;
    public NativeArray<Vector3> job_mesh_vertices;

    // private variables
    private int field_res;
    private int res_x;
    private int res_y;
    private int res_z;
    private float cell_size;

    private float3 offset;

    private Vector3[] verts;
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = mesh_object.GetComponent<MeshFilter>().sharedMesh;
        verts = mesh.vertices;

        Init();

        AllocateJobMemory();

        CalculateField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        ClearJobMemory();
    }

    public override void CalculateForce()
    {
        if (!force_initialized) return;

        SampleMeshField();
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

        TransformMeshVerts();
    }

    private void TransformMeshVerts()
    {
        for(int i=0; i<verts.Length; i++)
        {
            var vert = mesh_object.transform.TransformPoint(verts[i]);
            verts[i] = vert;
        }
    }

    #endregion

    #region Jobs

    private void AllocateJobMemory()
    {
        job_mesh_field = new NativeArray<float3>(field_res, Allocator.Persistent);
        job_grid_pts = new NativeArray<float3>(a_environment.grid_pts, Allocator.Persistent);
        job_mesh_vertices = new NativeArray<Vector3>(verts, Allocator.Persistent);
    }

    private void ClearJobMemory()
    {
        if (job_mesh_vertices.IsCreated) job_mesh_vertices.Dispose();
        if (job_grid_pts.IsCreated) job_grid_pts.Dispose();
        if (job_mesh_field.IsCreated) job_mesh_field.Dispose();
    }

    private void CalculateField()
    {
        var getField = new BakeMeshForceJob
        {
            mesh_vertices = job_mesh_vertices,
            field_points = job_mesh_field,
            grid_points = job_grid_pts,

            cell_size = a_environment.cell_size,
            vertex_count = verts.Length,
            max_force = max_strenth,
            min_force = min_strength
        };
        getField.Schedule(a_environment.grid_resolution, 128).Complete();
    }

    private void SampleMeshField()
    {
        var sampleJob = new SampleMeshForceJob
        {
            agents = job_agents,
            forces = job_agent_force,
            field = job_mesh_field,

            offset = offset,
            res_x = res_x,
            res_z = res_z,
            cell_size = cell_size,

            
        };

        sampleJob.Schedule(num_agents, 128).Complete();
    }

    #endregion
}
