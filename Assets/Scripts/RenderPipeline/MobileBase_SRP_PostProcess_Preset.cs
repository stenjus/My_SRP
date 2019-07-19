using UnityEngine;

[CreateAssetMenu(fileName = "MBSRP PostProccessing", menuName = "Presets/PostProcessing Preset")]
public class MobileBase_SRP_PostProcess_Preset : ScriptableObject
{
    //Vignetting values
    public bool _USE_Vignetting;
    public Color _VignettingColor = Color.black;
    public float _Vignetting_Size = 0.5F;
    public float _Vignetting_Contrast = 0.2F;

    [Space]

    //LUT Grading
    public bool _USE_LUTGrading;
    public Texture2D _LUT_Tex;
    public float _LUT_Power;

    [Space]

    //FishEye Distorsion
    public bool _USE_FishEye_Fragment;
    public bool _USE_FishEye_Vertex;
    public float _FishEye_Power;
}
