using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class GravitySimSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref GS_velocity v1, ref GS_position p1, ref GS_mass m1) =>
        {
            //p1.position += v1.velocity * Time.deltaTime;
        });
    }
}
