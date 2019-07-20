using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Presets/Mobile Base SRP Preset")]
public class MobileBase_SRP_Asset : RenderPipelineAsset
{
    public Material _ShaderErrorMaterial;
    public bool _InDynamicBatching;
    public bool _GPUInstancing;
    public bool _UseLinearLightIntencity = true;

    //Render to custom mesh vars
    public Mesh _RenderMesh;
    public Material _RenderMaterial;

    //Post Processing bools
    public bool _useBloom = false;
    public bool _useFishEye = true;
    public bool _useVignetting = true;
    public bool _useLUT = true;
    public Material _DualFiltering;
    public float _BlurOffsetDown = 1;
    public float _BlurOffsetUp = 1;
    public int _BluumPasses = 4;
    public bool _UseChromaticAberration = false;

    [Range(0, 10)] public int _DownScaleValue = 1;

    public MobileBase_SRP_PostProcess_Controller _MobileBase_SRP_PostProcess_Controller;

    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new MobileBase_SRP(this, _ShaderErrorMaterial, _InDynamicBatching, _GPUInstancing, _UseLinearLightIntencity);
    }
}