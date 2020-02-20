using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandInputScript : MonoBehaviour
{
    public GameObject spawnPoint;
    public Collider grabCollider;
    public OVRInput.Controller currentController;
    public GravitySim simulationSpace;
    public GameObject projectileTemplate;
    public float flowRate = 500f;
    public AudioSource beatMaster;
    public float buttonThreshold = .1f;
    public Material newMoonMaterial;

    private ObjectPooler objectPooler;
    private bool holdingProjectile = false;
    private bool spawningProjectile = false;
    private bool wantsToGrab = false;
    private JugglingBall nextProjectile;
    private GravitySimObject startingGravitySimObject;
    private int currentClip = 0;

    int indexTrigger = Animator.StringToHash("IndexTrigger");
    int handTrigger = Animator.StringToHash("HandTrigger");
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        startingGravitySimObject = Instantiate(projectileTemplate).GetComponent<GravitySimObject>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(anim != null, "no animator attached");
        objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        CheckTriggers();
    }
    
    void CheckTriggers()
    {
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, currentController);
        float grab = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, currentController);
        if(anim != null)
        {
            anim.SetFloat(indexTrigger, trigger);
            anim.SetFloat(handTrigger, grab);
        }
        if (trigger > buttonThreshold)
        {
            if (!spawningProjectile && !holdingProjectile)
            {
                spawningProjectile = true;
                CreateProjectile();
            }
        }
        else
        {
            if (spawningProjectile)
            {
                spawningProjectile = false;
                LaunchProjectile();
            }
        }
        if(grab > buttonThreshold)
        {
            if(!holdingProjectile && !spawningProjectile)
            {
                wantsToGrab = true;
            }
            else
            {
                wantsToGrab = false;
            }
        }
        else
        {
            wantsToGrab = false;
            if(holdingProjectile)
            {
                LaunchProjectile();
                holdingProjectile = false;
            }
        }
        if (spawningProjectile || holdingProjectile)
        {
            nextProjectile.transform.position = spawnPoint.transform.position;
            nextProjectile.transform.rotation = spawnPoint.transform.rotation;
            if (spawningProjectile)
            {
                nextProjectile.gravityObject.mass += flowRate * trigger * Time.deltaTime;
                nextProjectile.Reset();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        JugglingBall grabbed = other.gameObject.GetComponent<JugglingBall>();
        if (grabbed != null && grabbed.gravityObject.worldSim != null && wantsToGrab)
        {
            GrabProjectile(ref grabbed);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        JugglingBall grabbed = other.gameObject.GetComponent<JugglingBall>();
        if (grabbed != null && grabbed.gravityObject.worldSim != null && wantsToGrab)
        {
            GrabProjectile(ref grabbed);
        }
    }

    void GrabProjectile(ref JugglingBall projectile)
    {
        //Debug.Log("GRABBED");
        projectile.Pop(false);
        holdingProjectile = true;
        
        CreateProjectile();
        nextProjectile.gravityObject.mass = projectile.gravityObject.mass;
        nextProjectile.Reset();
        nextProjectile.soundObject.SetClip(projectile.GetComponent<SoundSimElement>().GetClip());
    }

    void CreateProjectile()
    {
        GameObject newGameObject = objectPooler.SpawnFromPool("ball", spawnPoint.transform.position, spawnPoint.transform.rotation);
        //Debug.Log("gameobject" + newGameObject);
        nextProjectile = newGameObject.GetComponent<JugglingBall>();
        nextProjectile.gravityObject.mass = startingGravitySimObject.mass;
        nextProjectile.soundObject.SetClip(++currentClip);
    }

    void LaunchProjectile()
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(currentController);
        nextProjectile.gravityObject.velocity = launchVelocity;
        if(newMoonMaterial != null)
        {
            nextProjectile.GetComponentInChildren<MeshRenderer>().material = newMoonMaterial;
        }
        nextProjectile.gravityObject.worldSim = simulationSpace;
        nextProjectile.gravityObject.Init();
        //Debug.Log("Adding object to sim, pitch: " + nextProjectileRight.GetComponent<AudioSource>().pitch + " volume: " + projectile.GetComponent<AudioSource>().volume);
    }
}
