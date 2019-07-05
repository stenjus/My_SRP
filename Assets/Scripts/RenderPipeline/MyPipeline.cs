using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
    CullResults _Cull;
    DrawRendererFlags _DrawFlags;
    CommandBuffer _CameraBuffer = new CommandBuffer {name = "Render Camera"};

    Material _ErrorMaterial;
    MyPipelineAsset _PipeLineAsset;

    //Constructor
    public MyPipeline(MyPipelineAsset _InPipeLineAsset, Material _InShaderErrorMaterial, bool _InDynamicBatching, bool _InGPU_Instancing)
    {
        _PipeLineAsset = _InPipeLineAsset;
        if (_InDynamicBatching) _DrawFlags = DrawRendererFlags.EnableDynamicBatching;
        if (_InGPU_Instancing) _DrawFlags = DrawRendererFlags.EnableInstancing;
        if (_InShaderErrorMaterial) _ErrorMaterial = _InShaderErrorMaterial;
    }

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


#if UNITY_EDITOR
        if (_Camera.cameraType == CameraType.SceneView)
        { 
        ScriptableRenderContext.EmitWorldGeometryForSceneView(_Camera);
        }
        #endif

        CullResults.Cull(ref _CullingParams, _Context, ref _Cull);

        //Setup the camera, culling and buffer
        _Context.SetupCameraProperties(_Camera);//SetUp Camera Matrix (transformation)
        CameraClearFlags _ClearFlags = _Camera.clearFlags; //Setup clearflags from from active camera

        
        _CameraBuffer.ClearRenderTarget
        (
            (_ClearFlags & CameraClearFlags.Depth) != 0,
            (_ClearFlags & CameraClearFlags.Color) != 0,
            _Camera.backgroundColor
        );
        //FrameDebugger Sampling
        _CameraBuffer.BeginSample("Render Camera");
        _Context.ExecuteCommandBuffer(_CameraBuffer);
        _CameraBuffer.Clear();

        var _DrawSettings = new DrawRendererSettings(_Camera, new ShaderPassName("SRPDefaultUnlit"));
        _DrawSettings.flags = _DrawFlags;

        //Opquare Filter and Render
        _DrawSettings.sorting.flags = SortFlags.CommonOpaque; //Set sorting flags for opquare render
        var _FilterSettings = new FilterRenderersSettings(true)
        {
            renderQueueRange = RenderQueueRange.opaque
        };
        _Context.DrawRenderers
        (
            _Cull.visibleRenderers,
            ref _DrawSettings,
            _FilterSettings
        );

        //SkyBox Rendering before Transparent
        _Context.DrawSkybox(_Camera);

        //Transparent Filter and Render
        _DrawSettings.sorting.flags = SortFlags.CommonTransparent; //Set sorting flags for transparent render
        _FilterSettings.renderQueueRange = RenderQueueRange.transparent;

        _Context.DrawRenderers
        (
            _Cull.visibleRenderers,
            ref _DrawSettings,
            _FilterSettings
        );

        //Set Default Pipline camera and context to render
        DrawDefaultPipeline(_Context, _Camera);

        _CameraBuffer.EndSample("Render Camera");
        _Context.ExecuteCommandBuffer(_CameraBuffer);
        _CameraBuffer.Clear();

        _Context.Submit();
    }

    //Default Unity Pipeline used for rendering Unity included shader iside Editor or in Development builds
    [Conditional ("UNITY_EDITOR"), Conditional ("DEVELOPMENT_BUILD")]
    void DrawDefaultPipeline(ScriptableRenderContext _Contex, Camera _Camera)
    {
        if (_ErrorMaterial == null)
        {
            Debug.Log("Shader Error Material not assigned");
            Shader _ErrorShader = Shader.Find("Hidden/InternalErrorShader");
            _ErrorMaterial = new Material(_ErrorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var _DrawSettings = new DrawRendererSettings(_Camera, new ShaderPassName("ForwardBase"));
        _DrawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        _DrawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        _DrawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        _DrawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        _DrawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
        _DrawSettings.SetOverrideMaterial(_ErrorMaterial, 0);
        var _FilterSettings = new FilterRenderersSettings(true);

        _Contex.DrawRenderers(_Cull.visibleRenderers, ref _DrawSettings, _FilterSettings);
    }
}