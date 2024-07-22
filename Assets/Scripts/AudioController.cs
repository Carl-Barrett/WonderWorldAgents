using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{

    public AudioSpectrum spectrum;
    public PerlinForce perlin;

   
    

    private int numBands;
    public float min_value;
    public float max_value;
    public int band;

    private float band_6;
    
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
        band_6 = spectrum.MeanLevels[band];
        if (band_6> max_value)
        {
            max_value = band_6;

        }

        if (band_6 < min_value)
        {
            min_value = band_6;
        }
        
        AdjustPerlinScale();
    }

    public void AdjustPerlinScale()
    {
        perlin.noise_scale = Remap(band_6, 0,  2f, 0.1f, 4);
    }
    
    public  float Remap ( float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
