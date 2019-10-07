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

    private static List<InputDevice> inputDevices = new List<InputDevice>();
    private static List<InputFeatureUsage> featureUsages = new List<InputFeatureUsage>();
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool pressed = false;
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
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, inputDevices);
        if (inputDevices.Count == 1)
            leftDevice = inputDevices[0];
        else
            Debug.Log("No device found for LeftHand");
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        if (inputDevices.Count == 1)
            rightDevice = inputDevices[0];
        else
            Debug.Log("No device found for rightHand");
    }

    void UpdateTracking()
    {
        // update the position / rotation of the anchors.
        Vector3 pos;
        Quaternion rot;
        if (leftDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos))
        {
            leftAnchor.transform.localPosition = pos;
        }
        if (leftDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        {
            leftAnchor.transform.rotation = rot;
        }
        if (rightDevice.TryGetFeatureValue(CommonUsages.devicePosition, out pos))
        {
            rightAnchor.transform.localPosition = pos;
        }
        if (rightDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rot))
        {
            rightAnchor.transform.rotation = rot;
        }
    }
    
    void CheckHands()
    {
        bool button = false;
        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out button);
        if (button)
        {
            if (!pressed)
            {
                pressed = true;
                GrabMoon();
            }
        }
        else
        {
            if (pressed)
            {
                pressed = false;
                ReleaseMoon();
            }
                
        }
        if (pressed)
        {
            nextProjectile.transform.position = rightAnchor.transform.position;
            nextProjectile.transform.rotation = rightAnchor.transform.rotation;
        }
    }

    void GrabMoon()
    {
        nextProjectile = Instantiate(projectileTemplate);
    }

    void ReleaseMoon()
    {
        Vector3 launchVelocity;
        if(rightDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out launchVelocity))
        {
            nextProjectile.velocity = launchVelocity;
        }  
        simulationSpace.RegisterObject(nextProjectile);
    }
}
