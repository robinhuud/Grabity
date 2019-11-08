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
    [SerializeField]
    public AudioClip[] spawnClips;

    private static List<InputDevice> inputDevices = new List<InputDevice>();
    private static List<InputFeatureUsage> featureUsages = new List<InputFeatureUsage>();
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool grabPressed = false;
    private bool triggerPressed = false;
    private bool launched = false;
    private GravitySimObject nextProjectile;
    private int newClip = 0;

    // Start is called before the first frame update
    void Start()
    {
        CheckClips();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTracking();
        CheckHands();
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
        // update the position / rotation of the anchors.
        //Vector3 pos;
        //Quaternion rot;
        //if (leftDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos))
        //{
        //    leftAnchor.transform.localPosition = pos;
        //}
        //if (leftDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        //{
        //    leftAnchor.transform.rotation = rot;
        //}
        //if (rightDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos))
        //{
        //    rightAnchor.transform.localPosition = pos;
        //}
        //if (rightDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        //{
        //    rightAnchor.transform.rotation = rot;
        //}
        leftAnchor.transform.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        leftAnchor.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        rightAnchor.transform.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        rightAnchor.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
    }
    
    void CheckHands()
    {
        float rightTrigger = 0;
        bool rightGrab = false;
        rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        rightGrab = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        if (rightTrigger > buttonThreshold)
        {
            if (!triggerPressed)
            {
                triggerPressed = true;
                CreateMoon();
            }
        }
        else
        {
            if (triggerPressed)
            {
                triggerPressed = false;
                ReleaseMoon();
            }
        }
        if (triggerPressed)
        {
            nextProjectile.transform.position = rightAnchor.transform.position;
            nextProjectile.transform.rotation = rightAnchor.transform.rotation;
            nextProjectile.mass += flowRate * rightTrigger * Time.deltaTime;
            nextProjectile.Reset();
        }
        if(rightGrab)
        {
            if(!grabPressed)
            {
                if(triggerPressed)
                {
                    nextProjectile.GetComponent<AudioSource>().enabled = false;
                    newClip++;
                    if (newClip >= spawnClips.Length)
                    {
                        newClip = 0;
                    }
                    nextProjectile.GetComponent<AudioSource>().clip = spawnClips[newClip];
                    nextProjectile.GetComponent<AudioSource>().enabled = true;
                    nextProjectile.GetComponent<AudioSource>().timeSamples = beatMaster.timeSamples;
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

    void CreateMoon()
    {
        nextProjectile = Instantiate(projectileTemplate);
        nextProjectile.GetComponent<AudioSource>().clip = spawnClips[newClip];
        nextProjectile.GetComponent<AudioSource>().enabled = true;
        nextProjectile.GetComponent<AudioSource>().timeSamples = beatMaster.timeSamples;
    }

    void ReleaseMoon()
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
        nextProjectile.velocity = launchVelocity;
        simulationSpace.RegisterObject(nextProjectile);
        Debug.Log("Adding object to sim, pitch: " + nextProjectile.GetComponent<AudioSource>().pitch + " volume: " + nextProjectile.GetComponent<AudioSource>().volume);
    }
}
