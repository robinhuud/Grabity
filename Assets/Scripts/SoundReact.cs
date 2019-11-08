using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundReact : MonoBehaviour
{
    [Tooltip("border values are in 86Hz increments (1/256th of 22050Hz)")]
    public short[] frequencyRangeBorders = new short[] { 2, 15, 63 };
    [Tooltip("range array must have one more element than the border values above.")]
    public float[] rangeMultipliers = new float[] { 200f, 800f, 400f, 3200f };
    [Tooltip("SkinnedMesh must have at least as many shape keys as the multipliers above")]
    public SkinnedMeshRenderer meshRenderer;
    [Tooltip("range 0-1, 0 is realtime, 1 is fully damped")]
    public float damping = .5f;
    [Tooltip("0-1 scroll speed multiplier for the texture")]
    public float textureScroll = .5f;
    public Color glowColor1 = Color.white;
    public Color glowColor2 = Color.green;
    public Vector2 textureOffset = new Vector2(0f, 0f);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              

    private float[] samples;
    private float[] bands;


    void Awake()
    {
        samples = new float[256];
        bands = new float[frequencyRangeBorders.Length + 1];
        if(meshRenderer is null)
        {
            meshRenderer = GetComponent<SkinnedMeshRenderer>();
            Debug.Assert(meshRenderer != null, "Cannot find a SkinnedMeshRenderer");
        }
        Debug.Assert(meshRenderer.sharedMesh.blendShapeCount > frequencyRangeBorders.Length, "Not enough blend shapes for frequency ranges");
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        MakeBands();
        for (int i = 0; i < bands.Length; i++)
        {
            meshRenderer.SetBlendShapeWeight(i, Mathf.Lerp(bands[i] * rangeMultipliers[i], meshRenderer.GetBlendShapeWeight(i), damping));
        }
        textureOffset[1] += Time.deltaTime * textureScroll;
        textureOffset[1] %= 1f;
        meshRenderer.materials[0].SetTextureOffset("_MainTex",textureOffset);
        meshRenderer.materials[1].SetColor("_EmissionColor", 5f * Color.Lerp(glowColor1, glowColor2, bands[1] * 2));
        //DynamicGI.SetEmissive(meshRenderer, Color.Lerp(glowColor1, glowColor2, bands[1]));
    }

    void MakeBands()
    {
        // must be called from within Update() or the AudioListener call returns all zeros
        // 22050 samples in 256 buckets = 86Hz/sample
        AudioListener.GetSpectrumData(samples, 0, FFTWindow.Triangle);
        int bandIndex = 0;
        int bandWidth = 0;
        bands[0] = 0;
        for(int i = 0; i < samples.Length; i++)
        {
            if(bandIndex < frequencyRangeBorders.Length && i > frequencyRangeBorders[bandIndex])
            {
                bands[bandIndex] /= bandWidth;
                bandIndex++;
                bandWidth = 1;
                bands[bandIndex] = samples[i];
            }
            else
            {
                bands[bandIndex] += samples[i];
                bandWidth++;
            }
        }
        bands[bandIndex] /= bandWidth;
    }
}
