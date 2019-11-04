using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;

public class GravitySim : MonoBehaviour
{
    public float GravitationalConstant = .0000000000667f;
    private List<GravitySimObject> members = new List<GravitySimObject>();
    private GravitySimObject[] memberArray;
    private bool tracking = false;
    private bool dirty = true;
    public GravitySimObject trackObject;
    [SerializeField]
    private Mesh mesh;
    [SerializeField]
    private Material material;

    public void CreateEntities()
    {
        EntityManager entityManager = World.Active.EntityManager;
        EntityArchetype GS_object = entityManager.CreateArchetype(
            typeof(GS_mass),
            typeof(GS_radius),
            typeof(GS_density),
            typeof(Translation),
            typeof(GS_velocity),
            typeof(RenderMesh)
        );
        NativeArray<Entity> entityArray = new NativeArray<Entity>(memberArray.Length, Allocator.Temp);
        entityManager.CreateEntity(GS_object, entityArray);
        for(int i = 0; i < entityArray.Length; i++)
        {
            entityManager.SetComponentData(entityArray[i], new GS_mass { mass = memberArray[i].mass });
            entityManager.SetComponentData(entityArray[i], new Translation { Value = memberArray[i].transform.position });
            entityManager.SetComponentData(entityArray[i], new GS_velocity { velocity = memberArray[i].velocity });
            entityManager.SetSharedComponentData(entityArray[i], new RenderMesh
            {
                mesh = mesh,
                material = material
            });
        }
        entityArray.Dispose();
    }

    // Register method allows GravitySimObjects to register in this simulation
    // re-creates the array each time the list is updated.
    public void RegisterObject(GravitySimObject obj)
    {
        members.Add(obj);
        obj.worldSim = this;
        dirty = true;
    }

    public void UnregisterObject(GravitySimObject obj)
    {
        members.Remove(obj);
        dirty = true;
    }

    void Start()
    {
        memberArray = members.ToArray();
        CreateEntities();
    }

    void Update()
    {
        if (dirty)
        {
            dirty = false;
            memberArray = members.ToArray();
        }
    }

    // FixedUpdate is where we do all our physics calculations
    void FixedUpdate()
    {
        Vector3 forceVector;
        // First apply the gravitational forces to all objects in the simulation
        for (int i = 0; i < memberArray.Length; i++)
        {
            for (int j = i + 1; j < memberArray.Length; j++)
            {
                forceVector = CalculateForce(memberArray[i], memberArray[j]);
                if (0.5f * (memberArray[i].radius + memberArray[j].radius) > (memberArray[i].transform.position - memberArray[j].transform.position).magnitude)
                {
                    Collide(memberArray[i], memberArray[j]);
                }
                memberArray[i].ApplyForce(forceVector);
                memberArray[j].ApplyForce(forceVector * -1f);
            }
        }
        if (trackObject != null)
        {
            // If we are tracking an object, we subtract it's velocity from everything in the simulation so that it looks like its
            // standing still. Better than moving the camera to track because we keep things near the origin to avoid floating point
            // precision issues.
            Vector3 trackOffset = trackObject.velocity;
            for (int i = 0; i < memberArray.Length; i++)
            {
                memberArray[i].velocity -= trackOffset;
            }
        }
    }

    private void Collide(GravitySimObject obj1, GravitySimObject obj2)
    {
        float massRatio = obj1.mass / (obj2.mass + obj1.mass);
        Vector3 newVelocity = obj1.velocity * massRatio + obj2.velocity * (1f - massRatio);
        Vector3 centerOfMass = ((obj1.transform.position * obj1.mass) + (obj2.transform.position * obj2.mass)) / (obj1.mass + obj2.mass);
        float newDensity = obj1.density * massRatio + obj2.density * (1f - massRatio);
        if(massRatio > .5f) // obj1 is bigger
        {
            obj1.mass += obj2.mass;
            obj1.velocity = newVelocity;
            obj1.density = newDensity;
            obj1.Reset();
            obj1.transform.position = centerOfMass;
            obj2.Poof();
        }
        else
        {
            obj2.mass += obj1.mass;
            obj2.velocity = newVelocity;
            obj2.density = newDensity;
            obj2.Reset();
            obj2.transform.position = centerOfMass;
            obj1.Poof();
        }
    }

    // Calculate the gravitational force on obj1 applied by obj2
    private Vector3 CalculateForce(GravitySimObject obj1, GravitySimObject obj2)
    {
        float dSquared = (obj1.transform.position - obj2.transform.position).sqrMagnitude;
        // use the Square of the distance to calculate the force
        float f = GravitationalConstant * ((obj1.mass * obj2.mass) / dSquared);
        // now make a vector with that force applied in the correct direction
        return (obj2.transform.position - obj1.transform.position).normalized * f;
    }
}
