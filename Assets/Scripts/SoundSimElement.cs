using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSimElement : MonoBehaviour
{
    public AudioSource beatMaster;
    public AudioClip popClip;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] spawnClips;
    private int myClipId = 0;


    // Start is called before the first frame update
    void Start()
    {
        if(audioSource is null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        //Debug.Log("AudioSource:" + audioSource);
        Debug.Assert(spawnClips.Length >= 2, "Not enough spawn clips");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetToneByMass(float mass)
    {
        if (audioSource != null)
        {
            float logVal = Mathf.Log(mass * .1f);
            //audioSource.pitch = 8f - logVal;
            audioSource.volume = logVal / 12f;
        }
    }

    public void NextClip()
    {
        myClipId++;
        if (myClipId >= spawnClips.Length)
        {
            myClipId = 0;
        }
        SetClip(myClipId);
    }

    public void SetClip(int clipId)
    {
        //Debug.Log("SetClip AudioSource:" + audioSource);
        clipId = clipId % spawnClips.Length;
        if(audioSource != null)
        {
            audioSource.enabled = false;
            audioSource.clip = spawnClips[clipId];
            audioSource.timeSamples = beatMaster.timeSamples;
            audioSource.enabled = true;
        }
    }

    public int GetClip()
    {
        return myClipId;
    }

    public void Pop(bool playSound = true)
    {
        if (playSound && audioSource != null && popClip != null && audioSource.isActiveAndEnabled)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(popClip);
            IEnumerator coroutine = WaitThenDie(popClip.length);
            StartCoroutine(coroutine);
        }
    }

    public IEnumerator WaitThenDie(float delay)
    {
        // If it has a mesh renderer or skinned mesh renderer, turn it off
        yield return new WaitForSeconds(delay);
        // RETURN TO POOL
        this.transform.gameObject.SetActive(false);
        yield return false;
    }

}
