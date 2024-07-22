using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFromMic : MonoBehaviour
{
    public AudioSource source;
    public Vector3 minScale;
    public Vector3 maxScale;
    public AudioLoudness detector;
    public float weight;
    public float loudness;
    public float smooth_value;
    private float prev_smooth_value;
    
    

    public float loudnessSensibility = 100;
    public float threshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        prev_smooth_value = loudness;

    }

    // Update is called once per frame
    void Update()
    {
        
        loudness = detector.GetLoudnessFromMicrophone() * loudnessSensibility;

        //.GetLoudnessFromMicrophone(source.timeSamples, source.clip);

        if (loudness < threshold)
        {
            loudness = 0;
        }
        Equalizer();
        //lerp value from minScale to maxScale
        transform.localScale = Vector3.Lerp(minScale, maxScale, loudness);

        


    }
    
    private void Equalizer()
    {
        smooth_value = (loudness * weight + prev_smooth_value * (1 - weight));
        prev_smooth_value = smooth_value;
    }

}
