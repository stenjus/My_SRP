using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
    public Material _ShaderErrorMaterial;
    public bool _InDynamicBatching;
    public bool _GPUInstancing;
    public bool _UseLinearLightIntencity = true;

    //Render to custom mesh vars
    public Mesh _RenderMesh;
    public Material _RenderMaterial;

    [Range(0, 10)] public int _DownScaleValue = 1;

    public bool _Bloom;

    protected override IRenderPipeline InternalCreatePipeline()
    {
        _InDynamicBatching = !_GPUInstancing;
        return new MyPipeline(this, _ShaderErrorMaterial, _InDynamicBatching, _GPUInstancing, _UseLinearLightIntencity);
    }
}