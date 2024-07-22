using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class microphoneController : MonoBehaviour
{

    public GameObject cubeScale;
    public ValueFallOff value;
    public AttractionForce push;
    public int band;
    public int twoband;

   
    

    private int numBands;
    public float min_value;
    public float max_value;

    private Vector3 size;
    private float band_2;
    
    // Start is called before the first frame update
    void Start()
    {
        //size = 1;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        size = cubeScale.transform.localScale;
        //Debug.Log(size.ToString());

        // observe the value range
        /*band_1 = spectrum.MeanLevels[band];
         band_2 = spectrum.MeanLevels[twoband];
         if (band_1> max_value)
         {
             max_value = band_1;
 
         }
 
         if (band_1 < min_value)
         {
             min_value = band_1;
         }
         
         AdjustBoidsScale();
         if (band_2> max_value)
         {
             max_value = band_2;
 
         }
 
         if (band_2 < min_value)
         {
             min_value = band_2;
         }
         */
         AdjustBoidsScale();
         
    }

    public void AdjustBoidsScale()
    {
        push.OVERALL_STRENGTH = Remap(size.x, 1f,  6f, 0f, -20);
        //uniform.force.x  = Remap(band_1, 0,  .03f, -5f, 2);
        //uniform.force.y  = Remap(band_2, 0,  .03f, -5f, 2);
        //uniform.force.z  = Remap(band_1, 0,  .03f, -5f, 2);
        
    }
    
    public  float Remap ( float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

