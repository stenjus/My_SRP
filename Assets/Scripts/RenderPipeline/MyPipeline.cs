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

    //Lighting variable
    static int _maxVisibleLights = 16;
    static int _VisibleLightColorID = Shader.PropertyToID("_VisibleLightColor");
    static int _VisibleLightDirectionOrPositionID = Shader.PropertyToID("_VisibleLightDirectionOrPosition");
    static int _maxVisibleLightsID = Shader.PropertyToID("_MaxVisibleLights");
    static int _visibleLightAttenuationID = Shader.PropertyToID("_VisibleLightAttenuation");
    static int _visibleLightSpotDirectionsID = Shader.PropertyToID("_visibleLightSpotDirections");

    Vector4[] _VisibleLightColors;
    Vector4[] _VisibleLightDirectionOrPosition;
    Vector4[] _VisibleLightAttenuations = new Vector4[_maxVisibleLights];
    Vector4[] _visibleLightSpotDirections = new Vector4[_maxVisibleLights];

    //Constructor
    public MyPipeline(MyPipelineAsset _InPipeLineAsset, Material _InShaderErrorMaterial, bool _InDynamicBatching, bool _InGPU_Instancing, bool _InUseLinearLightIntencity)
    {
        _PipeLineAsset = _InPipeLineAsset;
        if (_InDynamicBatching) _DrawFlags = DrawRendererFlags.EnableDynamicBatching;
        if (_InGPU_Instancing) _DrawFlags = DrawRendererFlags.EnableInstancing;
        if (_InShaderErrorMaterial) _ErrorMaterial = _InShaderErrorMaterial;
        _VisibleLightColors = new Vector4[_maxVisibleLights];
        _VisibleLightDirectionOrPosition = new Vector4[_maxVisibleLights];
        //Set Linear light intencity for our pipeline
        GraphicsSettings.lightsUseLinearIntensity = _InUseLinearLightIntencity;

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

        ConfigureLights();

        //FrameDebugger Sampling
        _CameraBuffer.BeginSample("Render Camera");

        //Send Data to GPU CBUFFER's
        _CameraBuffer.SetGlobalVectorArray(_VisibleLightColorID, _VisibleLightColors);
        _CameraBuffer.SetGlobalVectorArray(_VisibleLightDirectionOrPositionID, _VisibleLightDirectionOrPosition);
        _CameraBuffer.SetGlobalInt(_maxVisibleLightsID, _maxVisibleLights);
        _CameraBuffer.SetGlobalVectorArray(_visibleLightAttenuationID, _VisibleLightAttenuations);
        _CameraBuffer.SetGlobalVectorArray(_visibleLightSpotDirectionsID, _visibleLightSpotDirections);

        _Context.ExecuteCommandBuffer(_CameraBuffer);
        _CameraBuffer.Clear();

        var _DrawSettings = new DrawRendererSettings(_Camera, new ShaderPassName("SRPDefaultUnlit"))
        {
            flags = _DrawFlags, rendererConfiguration = RendererConfiguration.PerObjectLightIndices8
        };

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

    void ConfigureLights()
    {
        for (int i = 0; i < _Cull.visibleLights.Count; i++)
        {
            //Abort the loop when sending more than maximum supported lights
            if (i == _maxVisibleLights)
            {
                break;
            }

            //Define visible lights at index
            VisibleLight _light = _Cull.visibleLights[i];

            //Set lights colors array
            _VisibleLightColors[i] = _light.finalColor;

            Vector4 _attenuation = Vector4.zero;
            _attenuation.w = 1.0f;

            //Negate directional light direction
            if(_light.lightType == LightType.Directional)
            { 
                Vector4 v = _light.localToWorld.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;

                //Set directional lights directions array
                _VisibleLightDirectionOrPosition[i] = v;
            }
            else
            {
                _VisibleLightDirectionOrPosition[i] = _light.localToWorld.GetColumn(3);
                _attenuation.x = 1.0f / Mathf.Max(_light.range * _light.range, 0.00001f);


                
                if (_light.lightType == LightType.Spot)
                {
                    //Negate spot light direction
                    Vector4 v = _light.localToWorld.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;

                    //Set spot lights directions array
                    _visibleLightSpotDirections[i] = v;

                    //Angle Falloff calculations (A pice of mathf magic to make shit works)
                    float outerRad = Mathf.Deg2Rad * 0.5f * _light.spotAngle;
                    float outerCos = Mathf.Cos(outerRad);
                    float outerTan = Mathf.Tan(outerRad);
                    float innerCos = Mathf.Cos(Mathf.Atan((46.0f / 64.0f) * outerTan));
                    float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                    _attenuation.z = 1f / angleRange;
                    _attenuation.w = -outerCos * _attenuation.z;
                }
            }

            _VisibleLightAttenuations[i] = _attenuation;
        }
    }
}