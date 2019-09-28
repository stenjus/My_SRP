using UnityEngine;

[CreateAssetMenu(fileName = "MBSRP PostProcessing", menuName = "Presets/PostProcessing Preset")]
public class MobileBaseSRPPostProcessPreset : ScriptableObject
{
    //Vignetting values
    public bool UseVignetting;
    public Color VignettingColor = Color.black;
    public float VignettingSize = 0.5F;
    public float VignettingContrast = 0.2F;

    [Space]

    //LUT Grading
    public bool UseLUTGrading;
    public Texture2D LUTTex;
    public float LUTPower;

    [Space]

    //FishEye Distortion
    public bool UseFishEyeFragment;
    public bool UseFishEyeVertex;
    public float FishEyePower;

    [Space]

    //Bloom 
    public bool UseBloom;
    public float BloomIntensity;

    [Space]
    //Chromatic Aberration
    public bool UseChromaticAberration;
    public float ChromaticAberrationOffset;
    public float ChromaticAberrationRadius;

}
