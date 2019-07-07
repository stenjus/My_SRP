using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
    [SerializeField] Material _ShaderErrorMaterial;
    [SerializeField] bool _InDynamicBatching;
    [SerializeField] bool _GPUInstancing;
    [SerializeField] bool _UseLinearLightIntencity = true;
    [SerializeField] int _MaximumVisibleDirectionalLights = 4;

    protected override IRenderPipeline InternalCreatePipeline()
    {
        _InDynamicBatching = !_GPUInstancing;
        return new MyPipeline(this, _ShaderErrorMaterial, _InDynamicBatching, _GPUInstancing, _UseLinearLightIntencity, _MaximumVisibleDirectionalLights);
    }
}