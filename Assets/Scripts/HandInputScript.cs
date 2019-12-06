using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandInputScript : MonoBehaviour
{
    public GameObject leftAnchor;
    public GameObject rightAnchor;
    public GravitySim simulationSpace;
    public GravitySimObject projectileTemplate;
    public float flowRate = 500f;
    public AudioSource beatMaster;
    public float buttonThreshold = .1f;
    public Material newMoonMaterial;
    [SerializeField]
    public AudioClip[] spawnClips;

    private bool rightGrabPressed = false;
    private bool rightTriggerPressed = false;
    private bool leftGrabPressed = false;
    private bool leftTriggerPressed = false;
    private GravitySimObject nextProjectileLeft;
    private GravitySimObject nextProjectileRight;
    private int newClip = 0;

    void Awake()
    {
        // just in case you for got to un-set the world sim in the template object.
        projectileTemplate.worldSim = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        CheckClips();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTracking();
        CheckTriggers();
    }

    void CheckClips()
    {
        if(spawnClips.Length < 2)
        {
            Debug.Log("Less than 2 Audio Loops, please provide more");
        }
    }

    void UpdateTracking()
    {
        leftAnchor.transform.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        leftAnchor.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        rightAnchor.transform.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        rightAnchor.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
    }
    
    void CheckTriggers()
    {
        float rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        float rightGrab = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        float leftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        float leftGrab = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        if (rightTrigger > buttonThreshold)
        {
            if (!rightTriggerPressed)
            {
                rightTriggerPressed = true;
                CreateMoon(ref nextProjectileRight, OVRInput.Controller.RTouch);
            }
        }
        else
        {
            if (rightTriggerPressed)
            {
                rightTriggerPressed = false;
                ReleaseMoon(ref nextProjectileRight, OVRInput.Controller.RTouch);
            }
        }
        if (rightTriggerPressed)
        {
            nextProjectileRight.transform.position = rightAnchor.transform.position;
            nextProjectileRight.transform.rotation = rightAnchor.transform.rotation;
            nextProjectileRight.mass += flowRate * rightTrigger * Time.deltaTime;
            nextProjectileRight.Reset();
        }
        if(rightGrab > buttonThreshold)
        {
            if(!rightGrabPressed)
            {
                if(rightTriggerPressed)
                {
                    NextClip(ref nextProjectileRight);
                }
                rightGrabPressed = true;
            }
        } else
        {
            if(rightGrabPressed)
            {
                rightGrabPressed = false;
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
        Debug.Log("Clip is " + newClip);
        aSource.timeSamples = beatMaster.timeSamples;
        aSource.enabled = true;
        
    }

    void CreateMoon(ref GravitySimObject projectile, OVRInput.Controller controller)
    {
        projectile = Instantiate(projectileTemplate);
        projectile.GetComponent<AudioSource>().clip = spawnClips[newClip];
        projectile.GetComponent<AudioSource>().enabled = true;
        projectile.GetComponent<AudioSource>().timeSamples = beatMaster.timeSamples;
    }

    void ReleaseMoon(ref GravitySimObject projectile, OVRInput.Controller controller)
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(controller);
        projectile.velocity = launchVelocity;
        projectile.GetComponent<MeshRenderer>().material = newMoonMaterial;
        simulationSpace.RegisterObject(projectile);
        Debug.Log("Adding object to sim, pitch: " + nextProjectileRight.GetComponent<AudioSource>().pitch + " volume: " + projectile.GetComponent<AudioSource>().volume);
    }
}
