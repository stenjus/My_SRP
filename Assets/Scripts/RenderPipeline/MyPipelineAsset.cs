using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
    [SerializeField] Material _ShaderErrorMaterial;
    [SerializeField] bool _InDynamicBatching;
    [SerializeField] bool _GPUInstancing;
    [SerializeField] bool _UseLinearLightIntencity = true;

    //Render to custom mesh vars
    [SerializeField] Mesh _RenderMesh;
    [SerializeField] Material _RenderMaterial;

    [SerializeField, Range(1, 10)] int _DownScaleValue = 1;

    public bool _Bloom;

    protected override IRenderPipeline InternalCreatePipeline()
    {
        _InDynamicBatching = !_GPUInstancing;
        return new MyPipeline(this, _ShaderErrorMaterial, _InDynamicBatching, _GPUInstancing, _UseLinearLightIntencity, _RenderMesh, _RenderMaterial, _DownScaleValue);
    }
}