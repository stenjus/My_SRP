using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class MyPipeline : RenderPipeline
{
    public override void Render(ScriptableRenderContext _renderContext, Camera[] _Cameras)
    {
        base.Render(_renderContext, _Cameras);
        foreach (var _Camera in _Cameras)
        {
            Render(_renderContext, _Camera);
        }
    }
    void Render(ScriptableRenderContext _Context, Camera _Camera)
    {
        
        //Setup culling for current camera
        ScriptableCullingParameters _CullingParams;
        CullResults.GetCullingParameters(_Camera, out _CullingParams);
        if (!CullResults.GetCullingParameters(_Camera, out _CullingParams))
        {
            return;
        }
        CullResults _Cull = CullResults.Cull(ref _CullingParams, _Context);

        //Setup the camera, culling and buffer
        _Context.SetupCameraProperties(_Camera);//SetUp Camera Matrix (transformation)
        var _Buffer = new CommandBuffer {name = _Camera.name}; //Create and name our buffer to contains future commands
        CameraClearFlags _ClearFlags = _Camera.clearFlags; //Setup clearflags from from active camera
        _Buffer.ClearRenderTarget
        (
            (_ClearFlags & CameraClearFlags.Depth) != 0,
            (_ClearFlags & CameraClearFlags.Color) != 0,
            _Camera.backgroundColor
        );
        _Context.ExecuteCommandBuffer(_Buffer);
        _Buffer.Release();

        var _DrawSettings = new DrawRendererSettings(_Camera, new ShaderPassName("SRPDefaultUnlit"));
        var _FilterSettings = new FilterRenderersSettings(true);
        _Context.DrawRenderers
        (
            _Cull.visibleRenderers,
            ref _DrawSettings,
            _FilterSettings
        );

        _Context.DrawSkybox(_Camera);

        _Context.Submit();
    }
}