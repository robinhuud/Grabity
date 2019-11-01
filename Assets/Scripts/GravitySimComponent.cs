using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class GravitySimComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct GS_mass : IComponentData
{
    public float mass;
}

public struct GS_density : IComponentData
{
    public float density;
}

public struct GS_radius : IComponentData
{
    public float radius;
}

public struct GS_position : IComponentData
{
    public float3 position;
}

public struct GS_velocity : IComponentData
{
    public float3 velocity;
}