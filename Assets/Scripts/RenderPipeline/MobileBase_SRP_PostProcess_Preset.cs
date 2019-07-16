using UnityEngine;

[CreateAssetMenu(fileName = "MBSRP PostProccessing", menuName = "Presets/PostProcessing Preset")]
public class MobileBase_SRP_PostProcess_Preset : ScriptableObject
{
    //Vignetting values
    public Color _VignettingColor = Color.black;
    public float _Vignetting_Size = 0.5F;
    public float _Vignetting_Contrast = 0.2F;
}
