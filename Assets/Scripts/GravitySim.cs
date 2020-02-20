using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;

/// <summary>
/// This class is strictly for the actual gravity simulation, other physics and audio should be in a separate class
/// </summary>
public class GravitySim : MonoBehaviour
{
    public float GravitationalConstant = 6.67e-11f;
    public GravitySimObject trackObject;

    [SerializeField]
    public List<GravitySimObject> members = new List<GravitySimObject>();
    private GravitySimObject[] memberArray;
    private bool dirty = true;

    // Register method allows GravitySimObjects to register in this simulation
    // re-creates the array each time the list is updated.
    public void RegisterObject(ref GravitySimObject obj)
    {
        members.Add(obj);
        obj.worldSim = this;
        dirty = true;
    }

    public void UnregisterObject(ref GravitySimObject obj)
    {
        members.Remove(obj);
        dirty = true;
    }

    void Start()
    {
        memberArray = members.ToArray();
    }

    void Update()
    {
        if (dirty)
        {
            dirty = false;
            memberArray = members.ToArray();
            //Debug.Log("Members:" + memberArray.Length);
        }
    }

    // FixedUpdate is where we do all our physics calculations
    void FixedUpdate()
    {
        Vector3 forceVector;
        // First apply the gravitational forces to all objects in the simulation, and check for overlaps
        //Debug.Log("FixedUpdate GravitySim memberArray.Length is " + memberArray.Length);
        if(memberArray.Length > 1)
        {
            for (int i = 0; i < memberArray.Length; i++)
            {
                for (int j = i + 1; j < memberArray.Length; j++)
                {
                    forceVector = CalculateForce(memberArray[i], memberArray[j]);
                    if (0.5f * (memberArray[i].radius + memberArray[j].radius) > (memberArray[i].transform.position - memberArray[j].transform.position).magnitude)
                    {
                        //Debug.Log("Collide: Distance is " + (memberArray[i].transform.position - memberArray[j].transform.position).magnitude + 
                        //    ", radii are: " + (memberArray[i].radius) + ", " + (memberArray[j].radius));
                        Combine(memberArray[i], memberArray[j]);
                    } else
                    {
                        memberArray[i].ApplyForce(forceVector);
                        memberArray[j].ApplyForce(forceVector * -1f);
                    }
                }
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

    private void Combine(GravitySimObject obj1, GravitySimObject obj2)
    {
        float massRatio = obj1.mass / (obj2.mass + obj1.mass);
        Vector3 newVelocity = obj1.velocity * massRatio + obj2.velocity * (1f - massRatio);
        Vector3 centerOfMass = ((obj1.transform.position * obj1.mass) + (obj2.transform.position * obj2.mass)) / (obj1.mass + obj2.mass);
        float newDensity = obj1.density * massRatio + obj2.density * (1f - massRatio);
        if(massRatio >= .5f) // obj1 is bigger
        {
            obj1.mass += obj2.mass;
            obj1.velocity = newVelocity;
            obj1.density = newDensity;
            obj1.Resize();
            obj1.transform.position = centerOfMass;
            obj2.Pop();
        }
        else
        {
            obj2.mass += obj1.mass;
            obj2.velocity = newVelocity;
            obj2.density = newDensity;
            obj2.Resize();
            obj2.transform.position = centerOfMass;
            obj1.Pop();
        }
    }

    // Calculate the gravitational force on obj1 applied by obj2
    private Vector3 CalculateForce(GravitySimObject obj1, GravitySimObject obj2)
    {
        Vector3 delta = obj2.transform.position - obj1.transform.position;
        float sqrMag = delta.sqrMagnitude;
        // use the Square of the distance to calculate the force (inverse-square)
        float f = sqrMag == 0 ? 0 : GravitationalConstant * ((obj1.mass * obj2.mass) / sqrMag);
        // now make a vector with that force applied in the correct direction
        return delta.normalized * f;
    }
}
