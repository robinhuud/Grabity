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
    private JugglingBall grabbedProjectile;
    private GravitySimObject startingGravitySimObject;
    private int currentClip = 0;
    private GameLogicManager glm;

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
        glm = GameLogicManager.Instance;
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
            if (!spawningProjectile && !holdingProjectile && glm.CanSpawn())
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
        }
        else
        {
            wantsToGrab = false;
            if(holdingProjectile)
            {
                LaunchProjectile();
                holdingProjectile = false;
                grabbedProjectile = null;
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

    public void OnTriggerStay(Collider other)
    {
        if (wantsToGrab)
        {
            grabbedProjectile = other.gameObject.GetComponent<JugglingBall>();
            if (grabbedProjectile != null && grabbedProjectile.gravityObject.worldSim != null)
            {
                wantsToGrab = false;
                GrabProjectile(ref grabbedProjectile);
            }
        }
    }

    void GrabProjectile(ref JugglingBall projectile)
    {
        if (projectile != nextProjectile)
        {
            glm.AddScore(projectile.score);
        }
        CreateProjectile(projectile.gravityObject.mass, projectile.GetComponent<SoundSimElement>().GetClip());
        projectile.Pop(false);
        glm.AddBalls(1);
        holdingProjectile = true;

        //Debug.Log(nextProjectile.gravityObject.worldSim);
    }

    void CreateProjectile(float mass = 0, int clipId = -1)
    {
        GameObject newGameObject = objectPooler.SpawnFromPool("ball", spawnPoint.transform.position, spawnPoint.transform.rotation);
        glm.AddBalls(-1);
        //Debug.Log("gameobject" + newGameObject);
        nextProjectile = newGameObject.GetComponent<JugglingBall>();
        if(mass == 0)
        {
            nextProjectile.gravityObject.mass = startingGravitySimObject.mass;
        }
        else
        {
            nextProjectile.gravityObject.mass = mass;
        }
        if(clipId == -1)
        {
            nextProjectile.soundObject.SetClip(currentClip);
        }
        else
        {
            nextProjectile.soundObject.SetClip(clipId);
        }
        
        nextProjectile.Reset();
    }

    void LaunchProjectile()
    {
        Vector3 launchVelocity = OVRInput.GetLocalControllerVelocity(currentController);
        Vector3 launchSpin = OVRInput.GetLocalControllerAngularVelocity(currentController);
        nextProjectile.gravityObject.velocity = launchVelocity;
        nextProjectile.gravityObject.spin = launchSpin;
        if(newMoonMaterial != null)
        {
            nextProjectile.GetComponentInChildren<MeshRenderer>().material = newMoonMaterial;
        }
        nextProjectile.gravityObject.worldSim = simulationSpace;
        nextProjectile.gravityObject.Init();

        //Debug.Log("Adding object to sim, pitch: " + nextProjectileRight.GetComponent<AudioSource>().pitch + " volume: " + projectile.GetComponent<AudioSource>().volume);
    }
}
