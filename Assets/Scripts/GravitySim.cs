using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySim : MonoBehaviour
{
    public float GravitationalConstant = .0000000000667f;
    private List<GravitySimObject> members = new List<GravitySimObject>();
    private GravitySimObject[] memberArray;
    private bool tracking = false;
    public GravitySimObject trackObject;

    // Register method allows GravitySimObjects to register in this simulation
    // Must be called from the Awake() method, because we convert the list to an array
    // in Start() and make it immutable
    public void RegisterObject(GravitySimObject obj)
    {
        if(obj == trackObject)
        {
            tracking = true;
        }
        else
        {
            members.Add(obj);
            obj.worldSim = this;
        }
        memberArray = members.ToArray();
    }

    void Start()
    {
    }

    // FixedUpdate is where we do all our physics calculations
    void FixedUpdate()
    {
        Vector3 forceVector;
        for(int i = 0; i < memberArray.Length; i++)
        {
            // tracking object's forces get ignored
            if (tracking)
            {
                forceVector = CalculateForce(trackObject, memberArray[i]);
                memberArray[i].ApplyForce(forceVector * -1f);
            }
            // otherwise all objects act on all other objects
            for(int j = i + 1; j < memberArray.Length; j++)
            {
                forceVector = CalculateForce(memberArray[i], memberArray[j]);
                memberArray[i].ApplyForce(forceVector);
                memberArray[j].ApplyForce(forceVector * -1f);
            }
        }
    }

    void Update()
    {
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