using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySimObject : MonoBehaviour
{
    public float mass = 100f; // measured in Kg
    public float density = 1f; // measured in Kg/m^3
    public Vector3 velocity; // measured in m/s
    [SerializeField]
    public GravitySim worldSim;

    private float radius;
    private float volume;

    // Called before scene starts, before any Update is called on any object
    public void Awake()
    {
        // Calculate the volume based on density and mass
        volume = mass / density;
        // Radius based on volume
        radius = Mathf.Pow(((volume / Mathf.PI) * (.75f)), (1.0f / 3.0f));
        if(worldSim != null)
        {
            worldSim.RegisterObject(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("volume = " + volume + " radius = " + radius);
        this.transform.localScale = new Vector3(radius,radius,radius);
    }

    // Physics ticks have consistent deltaTime
    void Update()
    {
        //Debug.Log("Velocity is " + Velocity);
        this.transform.position += this.velocity * Time.deltaTime;
    }

    // This is the place where we apply the force. Can be done multiple times per physics frame (FixedUpdate)
    // if you need to but make sure you scale the forces appropriately.
    public void ApplyForce(Vector3 force)
    {
        //Debug.Log("Applied Force " + force.magnitude);
        this.velocity += force / mass;
    }
}