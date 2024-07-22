using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public struct Agent  
{
    public int id;
    public int bin_id;
    public float3 position;
    public float3 prev_position;
    public float3 velocity;
}
