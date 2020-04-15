using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySimObject : MonoBehaviour
{
    public float mass = 100f; // measured in Kg
    public float density = 1f; // measured in Kg/m^3
    public Vector3 velocity; // measured in m/s
    public Vector3 spin = Vector3.zero; // rotation per tick
    [SerializeField]
    public GravitySim worldSim;

    public float radius;

    private bool isDead = true;
    private GravitySimObject me;

    public void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        me = this;
        if (worldSim != null)
        {
            isDead = false;
            worldSim.RegisterObject(ref me);
        }
        isDead = false;
        Resize();
    }

    // Start is called before the first frame update
    void Start()
    {
        Resize();
    }

    // FixedUpdate is for physics calculations, in this case, movement based on velocity.
    void FixedUpdate()
    {
        if(!isDead)
        {
            if (worldSim != null)
            {
                this.transform.position += this.velocity * Time.fixedDeltaTime;
                this.transform.Rotate(this.spin);
            }
        }
    }

    // Can be called multiple times per physics frame (FixedUpdate)
    public void ApplyForce(Vector3 force)
    {
        this.velocity += force / mass;
    }

    public void Resize()
    {
        // Calculate radius of sphere based on volume (mass / density)
        radius = Mathf.Pow((((mass / density) / Mathf.PI) * (.75f)), (1.0f / 3.0f));
        this.transform.localScale = new Vector3(radius, radius, radius);

        JugglingBall jb = GetComponent<JugglingBall>();
        if(jb != null)
        {
            jb.Reset(false);
        }
    }

    public void Die()
    {
        if (!isDead) // Don't do anything if you're already dead.
        {
            isDead = true;
            // Remove myself from the simulation.
            worldSim.UnregisterObject(ref me);
            worldSim = null;

            // Special case for JugglingBalls Colliding
            JugglingBall jb = GetComponent<JugglingBall>();
            if(jb != null)
            {
                jb.Pop();
            }
            else
            {
                Debug.Log("No JugglingBall");
            }
        }
    }
}