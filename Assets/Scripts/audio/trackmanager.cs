using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trackmanager : MonoBehaviour
{
    public List<AudioClip> tracks = new List<AudioClip>();

    public AudioSource source;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    public void songselector(int trackID)
    {
        source.clip = tracks[trackID];
        source.Play();
    }
}
