using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandInputScript : MonoBehaviour
{
    public GameObject spawnPoint;
    public OVRInput.Controller currentController;
    public GravitySim simulationSpace;
    public GravitySimObject projectileTemplate;
    public float flowRate = 500f;
    public AudioSource beatMaster;
    public float buttonThreshold = .1f;
    public Material newMoonMaterial;
    [SerializeField]
    public AudioClip[] spawnClips;

    private bool grabPressed = false;
    private bool triggerPressed = false;
    private GravitySimObject nextProjectile;
    private int newClip = 0;
    int indexTrigger = Animator.StringToHash("IndexTrigger");
    int handTrigger = Animator.StringToHash("HandTrigger");
    Animator anim;

    void Awake()
    {
        // just in case you for got to un-set the world sim in the template object.
        projectileTemplate.worldSim = null;
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
            if (!triggerPressed)
            {
                triggerPressed = true;
                CreateMoon(ref nextProjectile);
            }
        }
        else
        {
            if (triggerPressed)
            {
                triggerPressed = false;
                ReleaseMoon(ref nextProjectile);
            }
        }
        if (triggerPressed)
        {
            nextProjectile.transform.position = spawnPoint.transform.position;
            nextProjectile.transform.rotation = spawnPoint.transform.rotation;
            nextProjectile.mass += flowRate * trigger * Time.deltaTime;
            nextProjectile.Reset();
        }
        if(grab > buttonThreshold)
        {
            if(!grabPressed)
            {
                if(triggerPressed)
                {
                    NextClip(ref nextProjectile);
                }
                grabPressed = true;
            }
        } else
        {
            if(grabPressed)
            {
                grabPressed = false;
            }
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

    void CreateMoon(ref GravitySimObject projectile)
    {
        projectile = Instantiate(projectileTemplate);
        projectile.GetComponent<AudioSource>().clip = spawnClips[newClip];
        projectile.GetComponent<AudioSource>().enabled = true;
        projectile.GetComponent<AudioSource>().timeSamples = beatMaster.timeSamples;
    }

    void ReleaseMoon(ref GravitySimObject projectile)
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(currentController);
        projectile.velocity = launchVelocity;
        if(newMoonMaterial != null)
        {
            projectile.GetComponentInChildren<MeshRenderer>().material = newMoonMaterial;
        }
        simulationSpace.RegisterObject(projectile);
        //Debug.Log("Adding object to sim, pitch: " + nextProjectileRight.GetComponent<AudioSource>().pitch + " volume: " + projectile.GetComponent<AudioSource>().volume);
    }
}
