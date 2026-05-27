using UnityEngine;

public class GlitchWaveTest : MonoBehaviour
{
    [Header("Glitch Settings")]
    public Material glitchMaterial;
    
    [Range(0f, 1f)]
    public float glitchIntensity = 0.5f;
    
    [Range(0f, 10f)]
    public float glitchSpeed = 1.0f;

    void Update()
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
            glitchMaterial.SetFloat("_Speed", glitchSpeed);
        }
    }
}
