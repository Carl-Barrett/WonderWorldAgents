using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct PerlinForceJob : IJobParallelFor
{
    public NativeArray<Agent> agents;
    public NativeArray<float3> agent_forces;

    public float n_scale;
    public float n_speed;
    public float time;


    public void Execute(int index)
    {       
        var pos = agents[index].position;
        float3 gr = float3.zero;
        float3 sampler = new float3(pos.x * n_scale + time * n_speed , pos.y * n_scale + time * n_speed , pos.z * n_scale + time * n_speed);

		// gradient
		//noise.snoise(sampler, out gr);
		//agent_forces[index] = gr;

		var curl = curlNoise(sampler);

		agent_forces[index] = curl;
       
    }

	public float3 snoiseVec3(float3 x)
	{

		float s = noise.snoise(x);
		float s1 = noise.snoise(new float3(x.y - 19.1f, x.z + 33.4f, x.x + 47.2f));
		float s2 = noise.snoise(new float3(x.z + 74.2f, x.x - 124.5f, x.y + 99.4f));
		float3 c = new float3(s, s1, s2);
		return c;
	}


	public float3 curlNoise(float3 p)
	{

		const float e = 0.5f;
		float3 dx = new float3(e, 0.0f, 0.0f);
		float3 dy = new float3(0.0f, e, 0.0f);
		float3 dz = new float3(0.0f, 0.0f, e);

		float3 p_x0 = snoiseVec3(p - dx);
		float3 p_x1 = snoiseVec3(p + dx);
		float3 p_y0 = snoiseVec3(p - dy);
		float3 p_y1 = snoiseVec3(p + dy);
		float3 p_z0 = snoiseVec3(p - dz);
		float3 p_z1 = snoiseVec3(p + dz);

		float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
		float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
		float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

		const float divisor = 1.0f / (2.0f * e);
		//return normalize(float3(x, y, z) * divisor);
		var curl = new float3(x, y, z) * divisor;

		return math.normalize(curl);

	}

	public float3 computeCurl(float3 pos)
	{
		var eps = 0.5f;

		var curl = float3.zero;

		//Find rate of change in YZ plane
		var n1a = noise.snoise(new float3(pos.x, pos.y + eps, pos.z));
		var n2a = noise.snoise(new float3(pos.x, pos.y - eps, pos.z));
		//Average to find approximate derivative
		var a = (n1a - n2a) / (2 * eps);
		var n1b = noise.snoise(new float3(pos.x, pos.y, pos.z + eps));
		var n2b = noise.snoise(new float3(pos.x, pos.y, pos.z - eps));
		//Average to find approximate derivative
		var b = (n1b - n2b) / (2 * eps);
		var curl_x = a - b;

		//Find rate of change in XZ plane
		n1a = noise.snoise(new float3(pos.x, pos.y, pos.z + eps));
		n2a = noise.snoise(new float3(pos.x, pos.y, pos.z - eps));
		a = (n1a - n2a) / (2 * eps);
		n1b = noise.snoise(new float3(pos.x + eps, pos.y, pos.z));
		n2b = noise.snoise(new float3(pos.x - eps, pos.y, pos.z));
		b = (n1b - n2b) / (2 * eps);
		var curl_y = a - b;

		//Find rate of change in XY plane
		n1a = noise.snoise(new float3(pos.x + eps, pos.y, pos.z));
		n2a = noise.snoise(new float3(pos.x - eps, pos.y, pos.z));
		a = (n1a - n2a) / (2 * eps);
		n1b = noise.snoise(new float3(pos.x, pos.y + eps, pos.z));
		n2b = noise.snoise(new float3(pos.x, pos.y - eps, pos.z)) ;
		b = (n1b - n2b) / (2 * eps);
		var curl_z = a - b;

		curl = new float3(curl_x, curl_y, curl_z);

		return curl;
	}
}
