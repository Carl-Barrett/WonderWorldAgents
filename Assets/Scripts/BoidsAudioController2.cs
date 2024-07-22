using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsAudioController2 : MonoBehaviour
{

    public AudioSpectrum spectrum;
    public BoidsForce boids2;
    public int band;

   
    

    private int numBands;
    public float min_value;
    public float max_value;

    private float band_1;
    
    // Start is called before the first frame update
    void Start()
    {
        numBands = spectrum.MeanLevels.Length;
        min_value = Mathf.Infinity;
        max_value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        // observe the value range
        band_1 = spectrum.MeanLevels[band];
        if (band_1> max_value)
        {
            max_value = band_1;

        }

        if (band_1 < min_value)
        {
            min_value = band_1;
        }
        
        AdjustBoidsScale();
    }

    public void AdjustBoidsScale()
    {
        boids2.OVERALL_STRENGTH = Remap(band_1, 0,  .05f, -1f, 5f);
    }
    
    public  float Remap ( float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

