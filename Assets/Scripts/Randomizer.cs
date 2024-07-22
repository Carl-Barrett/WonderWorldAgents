using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public class Randomizer : MonoBehaviour
{
    Unity.Mathematics.Random random;

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("=========NEW RANDOMS==========");
        random = new Unity.Mathematics.Random((uint)Random.Range(1, 100));
        GenerateRandoms();
    }

    private void GenerateRandoms()
    {
        
        var randomizer = new RandomTester
        {
            random = random
        };

        randomizer.Schedule(20, 128).Complete();
    }
}
