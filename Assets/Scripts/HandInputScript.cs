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
    public GravitySimObject projectileTemplate;
    public float flowRate = 500f;
    public AudioSource beatMaster;
    public float buttonThreshold = .1f;
    public Material newMoonMaterial;
    [SerializeField]
    public AudioClip[] spawnClips;

    private bool holdingProjectile = false;
    private bool spawningProjectile = false;
    private bool wantsToGrab = false;
    private GravitySimObject nextProjectile;
    private int newClip = 0;
    int indexTrigger = Animator.StringToHash("IndexTrigger");
    int handTrigger = Animator.StringToHash("HandTrigger");
    Animator anim;

    void Awake()
    {
        // just in case you for got to un-set the world sim in the template object.
        //projectileTemplate.worldSim = null;
        anim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(spawnClips.Length >= 2, "Not enough spawn clips");
        Debug.Assert(anim != null, "no animator attached");
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateTracking();
        CheckTriggers();

    }

    void UpdateTracking()
    {
        spawnPoint.transform.localPosition = OVRInput.GetLocalControllerPosition(currentController);
        spawnPoint.transform.rotation = OVRInput.GetLocalControllerRotation(currentController);
    }
    
    void CheckTriggers()
    {
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, currentController);
        float grab = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, currentController);
        anim.SetFloat(indexTrigger, trigger);
        anim.SetFloat(handTrigger, grab);
        if (trigger > buttonThreshold)
        {
            if (!spawningProjectile && !holdingProjectile)
            {
                spawningProjectile = true;
                CreateProjectile(ref nextProjectile);
                NextClip(ref nextProjectile);
            }
        }
        else
        {
            if (spawningProjectile)
            {
                spawningProjectile = false;
                ReleaseProjectile(ref nextProjectile);
            }
        }
        if(grab > buttonThreshold)
        {
            if(!holdingProjectile && !spawningProjectile)
            {
                wantsToGrab = true;
            }
        } else
        {
            wantsToGrab = false;
            if(holdingProjectile)
            {
                ReleaseProjectile(ref nextProjectile);
                holdingProjectile = false;
            }
        }
        if (spawningProjectile || holdingProjectile)
        {
            nextProjectile.transform.position = spawnPoint.transform.position;
            nextProjectile.transform.rotation = spawnPoint.transform.rotation;
            if (spawningProjectile)
            {
                nextProjectile.mass += flowRate * trigger * Time.deltaTime;
                nextProjectile.Reset();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        GravitySimObject grabbed = other.gameObject.GetComponent<GravitySimObject>();
        if (grabbed != null && grabbed.worldSim != null && wantsToGrab)
        {
            GrabProjectile(ref grabbed);
        }
    }

    void NextClip(ref GravitySimObject projectile)
    {
        AudioSource aSource = projectile.GetComponent<AudioSource>();
        aSource.enabled = false;
        newClip++;
        if(newClip >= spawnClips.Length)
        {
            newClip = 0;
        }
        aSource.clip = spawnClips[newClip];
        //Debug.Log("Clip is " + newClip);
        aSource.timeSamples = beatMaster.timeSamples;
        aSource.enabled = true;
        
    }

    void GrabProjectile(ref GravitySimObject projectile)
    {
        Debug.Log("GRABBED");
        projectile.Pop(false);
        holdingProjectile = true;
        //simulationSpace.UnregisterObject(ref projectile);
        //projectile.worldSim = null;
        CreateProjectile(ref nextProjectile);
        nextProjectile.mass = projectile.mass;
        nextProjectile.Reset();
        nextProjectile.GetComponent<AudioSource>().clip = projectile.GetComponent<AudioSource>().clip;
        projectile.transform.gameObject.SetActive(false);
    }

    void CreateProjectile(ref GravitySimObject projectile)
    {
        projectile = Instantiate(projectileTemplate);
        AudioSource aSource = projectile.GetComponent<AudioSource>();
        aSource.clip = spawnClips[newClip];
        aSource.enabled = true;
        aSource.timeSamples = beatMaster.timeSamples;
    }

    void ReleaseProjectile(ref GravitySimObject projectile)
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(currentController);
        projectile.velocity = launchVelocity;
        if(newMoonMaterial != null)
        {
            projectile.GetComponentInChildren<MeshRenderer>().material = newMoonMaterial;
        }
        simulationSpace.RegisterObject(ref projectile);
        //Debug.Log("Adding object to sim, pitch: " + nextProjectileRight.GetComponent<AudioSource>().pitch + " volume: " + projectile.GetComponent<AudioSource>().volume);
    }
}
