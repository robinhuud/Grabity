using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class GravitySimSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //Entities.ForEach((ref GS_velocity velocity, ref Translation position) =>
        //{
        //    position.Value += velocity.velocity;
        //});
    }
}
