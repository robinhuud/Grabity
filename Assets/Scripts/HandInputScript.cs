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

    private static List<InputDevice> inputDevices = new List<InputDevice>();
    private static List<InputFeatureUsage> featureUsages = new List<InputFeatureUsage>();
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool triggerPressed = false;
    private bool launched = false;
    private GravitySimObject nextProjectile;

    // Start is called before the first frame update
    void Start()
    {
        ConfigHands();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTracking();
        CheckHands();
    }

    void ConfigHands()
    {
        
        //InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, inputDevices);
        //if (inputDevices.Count >= 1)
        //    leftDevice = inputDevices[0];
        //else
        //    Debug.Log("No device found for LeftHand");
        //InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        //if (inputDevices.Count >= 1)
        //    rightDevice = inputDevices[0];
        //else
        //    Debug.Log("No device found for rightHand");
            
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
        bool rightTrigger = false;
        bool rightGrab = false;
        if(rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger))
        {
            if (rightTrigger)
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
                nextProjectile.mass += flowRate * Time.deltaTime;
                nextProjectile.Reset();
            }
        }
        if(rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out rightGrab))
        {

        }
 
        
    }

    void CreateMoon()
    {
        nextProjectile = Instantiate(projectileTemplate);
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
