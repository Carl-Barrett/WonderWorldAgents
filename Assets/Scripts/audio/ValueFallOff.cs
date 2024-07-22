using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ValueFallOff : MonoBehaviour
{
    [Tooltip("The changing value")]
    private float value;
    
    [Tooltip("The smooth value with fall off")]
    public float smooth_value;

    [Tooltip("The weight defines the percentage of the raw value that will be used in each iteration until the smooth value reaches it")]
    [Range(0.0f,1.0f)]
    public float weight;  // the smaller the weight the slower the smooth value will move toaward the raw, meaning a smoother transition



    // you need to keep track of the value's previous state as it changes and moving towards the raw value
    private float prev_smooth_value;



    // Start is called before the first frame update
    void Start()
    {
        prev_smooth_value = value;
    }

    // Update is called once per frame
    void Update()
    {
        value = transform.localScale.x;
        Equalizer();
        
    }

    private void Equalizer()
    {
        smooth_value = (value * weight + prev_smooth_value * (1 - weight));
        prev_smooth_value = smooth_value;
    }
}
