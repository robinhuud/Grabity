using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a container for holding a GravitySimObject and a SoundSimElement
/// as well as any other special effects we want to attach to the projectile, manages
/// the lifetimes of the objects it references.
/// </summary>
public class JugglingBall : MonoBehaviour, IPooledObject
{
    [SerializeField]
    public GravitySimObject gravityObject;
    [SerializeField]
    public SoundSimElement soundObject;

    public int score = 0;

    private bool isActive = false;

    private ObjectPooler objectPooler;
    private GameObject me;

    public void OnObjectSpawn()
    {
        gravityObject = GetComponent<GravitySimObject>();
        soundObject = GetComponent<SoundSimElement>();
        isActive = true;
    }

    public void OnObjectDespawn()
    {
        gravityObject.Die();
        soundObject.Pop(false);
        isActive = false;
    }

    void Start()
    {
        objectPooler = ObjectPooler.Instance;
        me = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reset(bool resizeGravity = true)
    {
        if(gravityObject != null && resizeGravity)
        {
            gravityObject.Resize();
        }
        if (soundObject != null)
        {
            soundObject.SetToneByMass(gravityObject.mass);
        }
        score = Mathf.FloorToInt(gravityObject.mass/1000);
        SphereCollider c = GetComponent<SphereCollider>();
        if ( c != null)
        {
            c.radius = gravityObject.radius;
        }
    }

    public void Pop(bool playSound = true)
    {
        if (isActive) // Don't do anything if you're already dead.
        {
            isActive = false;
            // Remove myself from the simulation.
            gravityObject.Die();
            // Stop any particle Systems
            if (null != GetComponent<ParticleSystem>())
            {
                GetComponent<ParticleSystem>().Stop();
            }
            // if we are supposed to play a sound on destruction, play it.
            if (soundObject != null)
            {
                soundObject.Pop(playSound);
            }
            objectPooler.ReleaseFromPool("ball", ref me);
        }
    }

    public void Bounce(Collider other)
    {
        RaycastHit hit;
        Vector3 posLastFrame = this.transform.position - (gravityObject.velocity * Time.fixedDeltaTime);
        other.Raycast(new Ray(posLastFrame, gravityObject.velocity.normalized), out hit, gravityObject.velocity.magnitude * 2f);
        // reflect the velocity around the hit normal
        if (hit.collider != null)
        {
            Vector3 reflect = gravityObject.velocity + 2f * (hit.normal * (Vector3.Dot(-gravityObject.velocity, hit.normal)));
            //Debug.Log("posLastFrame:" + posLastFrame + "\nhit.normal:" + hit.normal + "\nreflect:" + reflect);
            gravityObject.velocity = reflect;
        }
    }

    public void Push(Collider other)
    {

    }

    public void Score(Collider other)
    {
        ScoreTarget st = other.gameObject.GetComponent<ScoreTarget>();
        if (st != null)
        {
            st.activate(score);
        }
    }

    // This is the object that handles collisions.
    public void OnTriggerEnter(Collider other)
    {
        // This is for crashing into walls/floors. 
        // If other object is in the Gravity Simulation, then it's collision is already handled.
        if (!other.gameObject.GetComponent<GravitySimObject>())
        {
            if (other.CompareTag("Pop"))
            {
                Pop();
            }
            if (other.CompareTag("Bounce"))
            {
                Bounce(other);
            }
            if (other.CompareTag("Push"))
            {
                Push(other);
            }
            if (other.CompareTag("Score"))
            {
                Score(other);
            }
        }
    }
}
