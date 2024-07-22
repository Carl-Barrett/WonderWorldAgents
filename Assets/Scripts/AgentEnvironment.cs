using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class AgentEnvironment : MonoBehaviour
{
    [Header("Environment Settings")]
    public BoxCollider agent_region;
    public float cell_size;

    [Header("Spatial Binning Settings")]
    public int bin_cell_size;
    public int agents_per_bin;
    [HideInInspector] public int num_bins;

    [Header("Debug")]
    public bool see_grid_cells;
    public bool see_bin_cells;

    // private variables
    [HideInInspector] public int grid_resolution;
    [HideInInspector] public int bin_resolution;

    [HideInInspector] public Vector3 min_pt;
    [HideInInspector] public Vector3 max_pt;

    [HideInInspector] public int x_res;
    [HideInInspector] public int y_res;
    [HideInInspector] public int z_res;

    [HideInInspector] public int x_bin_res;
    [HideInInspector] public int y_bin_res;
    [HideInInspector] public int z_bin_res;

    private float x_length;
    private float y_length;
    private float z_length;

    [HideInInspector] public float3 axis_offset_raw;
    private float3 axis_offset;
    private float3 bin_axis_offset;
    private Vector3 center;

    [HideInInspector] public float3[] grid_pts;
    private float3[] bin_pts;

    private void Awake()
    {
        InitEnvironment();
        InitBins();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Create Environment

    private void InitEnvironment()
    {
        min_pt = agent_region.bounds.min;
        max_pt = agent_region.bounds.max;
        center = agent_region.bounds.center;

        axis_offset_raw = Vector3.zero - new Vector3(min_pt.x, min_pt.y, min_pt.z);
        axis_offset = axis_offset_raw - (float3)new Vector3(cell_size * 0.5f, cell_size * 0.5f, cell_size * 0.5f);

        x_length = max_pt.x - min_pt.x;
        y_length = max_pt.y - min_pt.y;
        z_length = max_pt.z - min_pt.z;

        x_res = (int)Mathf.Ceil(x_length / cell_size);
        y_res = (int)Mathf.Ceil(y_length / cell_size);
        z_res = (int)Mathf.Ceil(z_length / cell_size);

        grid_resolution = x_res * y_res * z_res;
        grid_pts = new float3[grid_resolution];

        CreateGridPts();
    }

    private void InitBins()
    {
        axis_offset_raw = Vector3.zero - new Vector3(min_pt.x, min_pt.y, min_pt.z);
        bin_axis_offset = axis_offset_raw - (float3)new Vector3(bin_cell_size * 0.5f, bin_cell_size * 0.5f, bin_cell_size * 0.5f);

        x_bin_res = (int)Mathf.Ceil(x_length / bin_cell_size);
        y_bin_res = (int)Mathf.Ceil(y_length / bin_cell_size);
        z_bin_res = (int)Mathf.Ceil(z_length / bin_cell_size);

        bin_resolution = x_bin_res * y_bin_res * z_bin_res;
        bin_pts = new float3[bin_resolution];

        CreateBinPts();
    }

    private void CreateGridPts()
    {
        for(int y = 0; y < y_res; y++)
        {
            for(int z = 0; z < z_res; z++)
            {
                for(int x = 0; x < x_res; x++)
                {
                    float3 pos = new float3(x * cell_size, y * cell_size, z * cell_size) - axis_offset;
                    int index = y * x_res * z_res + z * x_res + x;
                    grid_pts[index] = pos;
                }
            }
        }
    }

    private void CreateBinPts()
    {
        for (int y = 0; y < y_bin_res; y++)
        {
            for (int z = 0; z < z_bin_res; z++)
            {
                for (int x = 0; x < x_bin_res; x++)
                {
                    float3 pos = new float3(x * bin_cell_size, y * bin_cell_size, z * bin_cell_size) - bin_axis_offset;
                    int index = y * x_bin_res * z_bin_res + z * x_bin_res + x;
                    bin_pts[index] = pos;
                }
            }
        }
    }


    #endregion

    #region Visualization

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // see grid
        if (see_grid_cells)
        {
            for (int i = 0; i < grid_resolution; i++)
            {
                var pt = grid_pts[i];
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(pt, new Vector3(cell_size, cell_size, cell_size));
            }
        }

        // see bins
        if (see_bin_cells)
        {
            for (int i = 0; i < bin_resolution; i++)
            {
                var pt = bin_pts[i];
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(pt, new Vector3(bin_cell_size, bin_cell_size, bin_cell_size));
            }
        }
    }

    #endregion
}
