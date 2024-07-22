using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public struct RandomTester : IJobParallelFor
{
    //public NativeArray<float> random_numbers;
    public Unity.Mathematics.Random random;


    public void Execute(int index)
    {
        var r = random.NextFloat();
        var r1 = random.NextFloat();
        var r2 = random.NextFloat();

        var pos = new float3(r, r1, r2);
        Debug.Log(pos);
    }
}
