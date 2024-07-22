using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin2AudioController1 : MonoBehaviour
{

    public AudioSpectrum spectrum;
    public PerlinForce perlin2;
    public int band;
   
    

    private int numBands;
    public float min_value;
    public float max_value;

    private float band_8;
    
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
        band_8 = spectrum.MeanLevels[band];
        if (band_8> max_value)
        {
            max_value = band_8;

        }

        if (band_8 < min_value)
        {
            min_value = band_8;
        }
        
        AdjustBoidsScale();
    }

    public void AdjustBoidsScale()
    {
        perlin2.noise_speed = Remap(band_8, 0,  .25f, 0f, 5);
    }
    
    public  float Remap ( float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

