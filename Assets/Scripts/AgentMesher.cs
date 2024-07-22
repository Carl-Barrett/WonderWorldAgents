using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;

public class AgentMesher : MonoBehaviour
{
    [Header("Referenced Elements")]
    public StigmergyManager stigmergy;
    public ObjExporter exporter;

    [Header("Meshing Settings")]
    public int _triangleBudget = 65536;
    public float _targetValue = 0;

    [Header("Compute")]
    public ComputeShader _builderCompute;

    private ComputeBuffer _voxelBuffer;
    private MeshBuilder _builder;

    private float3[] m_vertices;
    private int[] m_indices;

    private bool initialized;
    public Mesh CPU_mesh;

    // native arrays
    private NativeArray<float3> meshData;
    private NativeArray<int> meshIndices;


    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
        m_vertices = new float3[_triangleBudget * 3 * 2];
        m_indices = new int[_triangleBudget * 3];
        meshData = new NativeArray<float3>(_triangleBudget * 3 * 2, Allocator.Persistent);
        meshIndices = new NativeArray<int>(_triangleBudget * 3 , Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < 0.5f) return;
        if (!initialized)
        {
            _builder = new MeshBuilder(stigmergy.res_x, stigmergy.res_y, stigmergy.res_z, _triangleBudget, _builderCompute);
            _voxelBuffer = new ComputeBuffer(stigmergy.num_cells, sizeof(float));
            //m_vertices = new NativeArray<float3>(_triangleBudget * 3, Allocator.Persistent);
            //m_normals = new NativeArray<float3>(_triangleBudget * 3, Allocator.Persistent);

            initialized = true;

        }

        // Isosurface reconstruction

        _voxelBuffer.SetData(stigmergy.job_flipped_scalar_field);
        _builder.BuildIsosurface(_voxelBuffer, _targetValue, stigmergy.cell_size);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;


        // export mesh
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateCPUMesh();
            exporter.mesh = CPU_mesh;
            exporter.name = "new_mesh";
            exporter.root = transform;
            exporter.DoExportWOSubmeshes();

           
        }

        //Debug.Log(_builder.Mesh.vertices.Length);
    }

    private void OnDestroy()
    {
        _builder.Dispose();
        _voxelBuffer.Dispose();
        if (meshData.IsCreated) meshData.Dispose();
        if (meshIndices.IsCreated) meshIndices.Dispose();

        //CreateCPUMesh();
    }

    public void ExportCPUMesh()
    {
        CreateCPUMesh();
        exporter.mesh = CPU_mesh;
        exporter.name = "new_mesh";
        exporter.root = transform;
        exporter.DoExportWOSubmeshes();
    }

    public void CreateCPUMesh()
    {
        _builder._vertexBuffer.GetData(m_vertices);
        _builder._indexBuffer.GetData(m_indices);


        // Allocate mesh data based on the valid indices count
        var md = Mesh.AllocateWritableMeshData(1);
        var d = md[0];


        // Define vertex buffer
        d.SetVertexBufferParams(m_vertices.Length/2,
                                new VertexAttributeDescriptor(VertexAttribute.Position),
                                new VertexAttributeDescriptor(VertexAttribute.Normal));
        //Define index buffer
        d.SetIndexBufferParams(m_indices.Length, IndexFormat.UInt32);

        // Get Mesh Data arrays
        var vBuffer = d.GetVertexData<MeshVertex>();
        var iBuffer = d.GetIndexData<int>();

        meshData.CopyFrom(m_vertices);
        iBuffer.CopyFrom(m_indices);
        //meshIndices.CopyFrom(m_indices);

        // Write to nesh data arrays in parallel
        new DecomposeMeshBufferJob
         {
             vertexBuffer = vBuffer,
            // indices = iBuffer,
             meshDataBuffer = meshData,
            // validVerticesIndexMap = meshIndices,
             //res = new int3((int)solver.domainResolution),
             //size = solver.bounds.size
         }.Schedule(meshIndices.Length, 128).Complete();

        // Define sub-meshes
        d.subMeshCount = 1;
        d.SetSubMesh(0, new SubMeshDescriptor(0, iBuffer.Length, MeshTopology.Triangles));

        // Clean mesh
        if (CPU_mesh == null)
        {
            CPU_mesh = new Mesh();
        }

        CPU_mesh.Clear();

        // Apply arrays to mesh
        Mesh.ApplyAndDisposeWritableMeshData(md, CPU_mesh);

       CPU_mesh.bounds = new Bounds(new Vector3(0, -20, 0), new Vector3(40, 40, 40));

    }

    public struct MeshVertex
    {
        public float3 pos;
        public float3 norm;
    }

    public struct Vert
    {
        public Vector4 position;
        public Vector3 normal;
    }

    [BurstCompile]
    private struct DecomposeMeshBufferJob : IJobParallelFor
    {
      //  [ReadOnly] public NativeArray<int> validVerticesIndexMap;
        [ReadOnly] public NativeArray<float3> meshDataBuffer;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<MeshVertex> vertexBuffer;

        //[NativeDisableContainerSafetyRestriction]
      //  public NativeArray<uint> indices;

        public void Execute(int index)
        {
          //  var i = validVerticesIndexMap[index];
            var mv = meshDataBuffer[index * 2];
            var mn = meshDataBuffer[index * 2 + 1];

            var v = vertexBuffer[index];

            v.pos = mv;
            v.norm = mn;

            vertexBuffer[index] = v;
          //  indices[index] = (uint)i;

            //Debug.Log($"index {index}, vert {v.pos} , norm {v.norm}");
        }

        //public int3 res;
        //public float3 size;

        /*public void Execute(int index)
        {
            var i = validVerticesIndexMap[index];
            var md = meshDataBuffer[i];

            var v = vertexBuffer[index];
            v.pos = (new float3(md.position.x, md.position.y, md.position.z) - new float3(0, res.y * 0.5f, 0)) / res * size;
            v.norm = md.normal;

            vertexBuffer[index] = v;
            indices[index] = (uint)index;
        }*/
    }
}


