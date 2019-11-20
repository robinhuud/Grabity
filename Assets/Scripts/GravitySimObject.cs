﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySimObject : MonoBehaviour
{
    public float mass = 100f; // measured in Kg
    public float density = 1f; // measured in Kg/m^3
    public Vector3 velocity; // measured in m/s
    [SerializeField]
    public GravitySim worldSim;
    public AudioClip popClip;

    public float radius;
    private float volume;
    private bool dirty = true;
    private AudioSource audioSource;

    // Called before scene starts, before any Update is called on any object
    public void Awake()
    {
        if(worldSim != null && this.isActiveAndEnabled)
        {
            worldSim.RegisterObject(this);
        }
        //Debug.Log("AWAKE: " + this.velocity);
    }
    // Start is called before the first frame update
    void Start()
    {
        Reset();
        //Debug.Log("START: " + this.velocity);
    }

    void Update()
    {
        if(dirty)
        {
            dirty = false;
            Reset();
        }
        //Debug.Log("velocity is " + this.velocity + " time.deltaTime is " + Time.deltaTime);

        //this.transform.position += this.velocity * Time.deltaTime;
    }

    void FixedUpdate()
    {
        this.transform.position += this.velocity * Time.fixedDeltaTime;
    }

    // This is the place where we apply the force. Can be done multiple times per physics frame (FixedUpdate)
    // if you need to but make sure you scale the forces appropriately.
    public void ApplyForce(Vector3 force)
    {
        //Debug.Log("Applied Force " + force + " / " + mass);
        this.velocity += force / mass;
    }
    public void Reset()
    {
        // Calculate the volume based on density and mass
        volume = mass / density;
        // Radius based on volume
        radius = Mathf.Pow(((volume / Mathf.PI) * (.75f)), (1.0f / 3.0f));
        this.transform.localScale = new Vector3(radius, radius, radius);
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            float logVal = Mathf.Log(mass * .1f);
            //audioSource.pitch = 8f - logVal;
            audioSource.volume = logVal / 12f;
            //Debug.Log(this.gameObject.name + " Pitch:" + audioSource.pitch + ", Vol:" + audioSource.volume);
        }
    }
    public void Poof()
    {
        worldSim.UnregisterObject(this);
        if (audioSource != null && popClip != null && audioSource.isActiveAndEnabled)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(popClip);
            IEnumerator coroutine = PlaySoundThenDie(popClip.length);
            StartCoroutine(coroutine);
        }
        else
        {
            this.transform.gameObject.SetActive(false);
        }
    }

    public IEnumerator PlaySoundThenDie(float delay)
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(delay);
        this.transform.gameObject.SetActive(false);
        yield return false;
    }
}