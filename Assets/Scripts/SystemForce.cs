using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SystemForce
{
    public string name;
    public bool enabled;
    //[Range(-2.0f,2.0f)]
    //public float force_strength;
    public AgentForce agent_force;
}
