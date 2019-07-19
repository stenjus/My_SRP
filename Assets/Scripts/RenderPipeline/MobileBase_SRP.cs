﻿using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MobileBase_SRP : RenderPipeline
{

    //SRP Main Variables
    CullResults _Cull;
    DrawRendererFlags _DrawFlags;
    ScriptableCullingParameters _CullingParams;
    CameraClearFlags _ClearFlags;
    DrawRendererSettings _DrawSettings;
    FilterRenderersSettings _FilterSettings;

    //Command Buffers
    CommandBuffer _CameraBuffer             = new CommandBuffer {name = "Render Camera"};
    CommandBuffer _BloomBuffer              = new CommandBuffer { name = "Bloom Buffer" };
    CommandBuffer _LightingBuffer           = new CommandBuffer { name = "Lighting Buffer" };
    CommandBuffer _PostProcessingBuffer     = new CommandBuffer { name = "PostPorcessing Buffer" };

    //Preset Imported
    MobileBase_SRP_Asset _PipeLineAsset;
    Material _ErrorMaterial;
    Material _RenderMaterial;
    Mesh _RenderMesh;

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

    //Post Processing values
    //SubRes Settings
    int _lowResW;
    int _lowResH;
    //Bloom Settings
    static int _BloomPasses = 6;

    //Constructor
    public MobileBase_SRP(MobileBase_SRP_Asset _InPipeLineAsset, 
                        Material _InShaderErrorMaterial, 
                        bool _InDynamicBatching, 
                        bool _InGPU_Instancing, 
                        bool _InUseLinearLightIntencity)
    {
        _PipeLineAsset = _InPipeLineAsset;
        if (_InDynamicBatching) _DrawFlags = DrawRendererFlags.EnableDynamicBatching;
        if (_InGPU_Instancing) _DrawFlags = DrawRendererFlags.EnableInstancing;
        if (_InShaderErrorMaterial) _ErrorMaterial = _InShaderErrorMaterial;

        //Lighting
        _VisibleLightColors = new Vector4[_maxVisibleLights];
        _VisibleLightDirectionOrPosition = new Vector4[_maxVisibleLights];

        //Set Linear light intencity for our pipeline
        GraphicsSettings.lightsUseLinearIntensity = _InUseLinearLightIntencity;

        //Find PostProcess Controller in current open scene
        
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
        CullResults.GetCullingParameters(_Camera, out _CullingParams);
        if (!CullResults.GetCullingParameters(_Camera, out _CullingParams)) { return; }
        CullResults.Cull(ref _CullingParams, _Context, ref _Cull);

        //Render Unity UI in SceneView
        #if UNITY_EDITOR
        if (_Camera.cameraType == CameraType.SceneView){ ScriptableRenderContext.EmitWorldGeometryForSceneView(_Camera); }
        #endif

        _Context.SetupCameraProperties(_Camera); //SetUp Camera Matrix (transformation)
        _ClearFlags = _Camera.clearFlags; //Setup clearflags from from active camera

        //Set postProcessing values to the Camera Blit Shader
        //
        //Set global values and keywords for vignetting
        if (_PipeLineAsset._useVignetting)
        {
            if (_PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._USE_Vignetting)
            {
                _PostProcessingBuffer.SetGlobalColor("_VignettingColor", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._VignettingColor);
                _PostProcessingBuffer.SetGlobalFloat("_Vignetting_Size", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._Vignetting_Size);
                _PostProcessingBuffer.SetGlobalFloat("_Vignetting_Contrast", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._Vignetting_Contrast);
                _PostProcessingBuffer.EnableShaderKeyword("VIGNETTING_ON");
            }
            else
            {
                _PostProcessingBuffer.DisableShaderKeyword("VIGNETTING_ON");
            }
        }
        else
        {
            _PostProcessingBuffer.DisableShaderKeyword("VIGNETTING_ON");
        }
        //
        //Set global values and keywords for vignetting
        if (_PipeLineAsset._useLUT)
        {
            if (_PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._USE_LUTGrading)
            {
                _PostProcessingBuffer.SetGlobalFloat("_LUT_Power", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._LUT_Power);
                _PostProcessingBuffer.SetGlobalTexture("_LUT_Tex", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._LUT_Tex);
                _PostProcessingBuffer.EnableShaderKeyword("LUT_ON");
            }
            else
            {
                _PostProcessingBuffer.DisableShaderKeyword("LUT_ON");
            }
        }
        else
        {
            _PostProcessingBuffer.DisableShaderKeyword("LUT_ON");
        }
        //
        //Set global values and keywords for vignetting
        if (_PipeLineAsset._useFishEye)
        {
            if (_PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._USE_FishEye_Fragment)
            {
                _PostProcessingBuffer.SetGlobalFloat("_FishEye", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._FishEye_Power);
                _PostProcessingBuffer.EnableShaderKeyword("FISHEYE_ON_FRAGMENT");
            }
            else
            {
                _PostProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_FRAGMENT");
            }
            if (_PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._USE_FishEye_Vertex)
            {
                _PostProcessingBuffer.SetGlobalFloat("_FishEye", _PipeLineAsset._MobileBase_SRP_PostProcess_Controller._PostProcessPreset._FishEye_Power);
                _PostProcessingBuffer.EnableShaderKeyword("FISHEYE_ON_VERTEX");
            }
            else
            {
                _PostProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_VERTEX");
            }
        }
        else
        {
            _PostProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_FRAGMENT");
            _PostProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_VERTEX");
        }


        _Context.ExecuteCommandBuffer(_PostProcessingBuffer);
        _PostProcessingBuffer.Clear();

        //Set down scaled resolutuion
        if (_PipeLineAsset._DownScaleValue > 1)
        {
            _lowResW = _Camera.pixelWidth / _PipeLineAsset._DownScaleValue;
            _lowResH = _Camera.pixelHeight / _PipeLineAsset._DownScaleValue;
        }
        else
        {
            _lowResW = _Camera.pixelWidth;
            _lowResH = _Camera.pixelHeight;
        }

        //Define render target as Temporary render textur
        MobileBase_SRP_CommonValues.RenderTexture.FrameBufferID = new RenderTargetIdentifier(MobileBase_SRP_CommonValues.RenderTexture.FrameBuffer);
        MobileBase_SRP_CommonValues.RenderTexture.FrameBufferDescriptor = new RenderTextureDescriptor(_lowResW, _lowResH, RenderTextureFormat.ARGBHalf, 24);
        _CameraBuffer.GetTemporaryRT(MobileBase_SRP_CommonValues.RenderTexture.FrameBuffer, MobileBase_SRP_CommonValues.RenderTexture.FrameBufferDescriptor, FilterMode.Bilinear);
        _CameraBuffer.SetRenderTarget(MobileBase_SRP_CommonValues.RenderTexture.FrameBufferID);
        _CameraBuffer.SetGlobalTexture("_FrameBuffer", MobileBase_SRP_CommonValues.RenderTexture.FrameBufferID);


        _CameraBuffer.ClearRenderTarget
        (
            (_ClearFlags & CameraClearFlags.Depth) != 0,
            (_ClearFlags & CameraClearFlags.Color) != 0,
            _Camera.backgroundColor
        );

        _Context.ExecuteCommandBuffer(_CameraBuffer);
        _CameraBuffer.Clear();


        //Lighting Commands ---
        ConfigureLights();
        //Send Data to GPU CBUFFER's
        _LightingBuffer.SetGlobalVectorArray(_VisibleLightColorID, _VisibleLightColors);
        _LightingBuffer.SetGlobalVectorArray(_VisibleLightDirectionOrPositionID, _VisibleLightDirectionOrPosition);
        _LightingBuffer.SetGlobalInt(_maxVisibleLightsID, _maxVisibleLights);
        _LightingBuffer.SetGlobalVectorArray(_visibleLightAttenuationID, _VisibleLightAttenuations);
        _LightingBuffer.SetGlobalVectorArray(_visibleLightSpotDirectionsID, _visibleLightSpotDirections);

        _Context.ExecuteCommandBuffer(_LightingBuffer);
        _LightingBuffer.Clear();
        // ---

        //FrameDebugger Sampling
        _CameraBuffer.BeginSample("Render Camera");
        _DrawSettings = new DrawRendererSettings(_Camera, new ShaderPassName("SRPDefaultUnlit"))
        {
            flags = _DrawFlags, rendererConfiguration = RendererConfiguration.PerObjectLightIndices8
        };

        //Opquare Filter and Render
        _DrawSettings.sorting.flags = SortFlags.CommonOpaque; //Set sorting flags for opquare render
        _FilterSettings = new FilterRenderersSettings(true)
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

        /* Invoke Bloom Post void --------- Need to more know bout it.
        if (_PipeLineAsset._Bloom)
        {
            BloomPost(_Context, _lowResW, _lowResH, MobileBase_SRP_CommonValues.RenderTexture.FrameBufferID);
        }
        */

        _CameraBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        _CameraBuffer.DrawMesh(_PipeLineAsset._RenderMesh, Matrix4x4.identity, _PipeLineAsset._RenderMaterial);
        _Context.ExecuteCommandBuffer(_CameraBuffer);
        _CameraBuffer.Clear();
        _CameraBuffer.EndSample("Render Camera");

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

    private void BloomPost(ScriptableRenderContext _Context, int _ScreenWidth, int _ScreenHeight, RenderTargetIdentifier _FrameBuffer)
    {
        MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBufferID = new RenderTargetIdentifier(MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBuffer);
        _BloomBuffer.BeginSample("Bloom Buffer");
        for (int i = 0; i < _BloomPasses; i++)
        {
            _BloomBuffer.GetTemporaryRT(MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBuffer, _ScreenWidth >> i, _ScreenHeight >> i, 16, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            _BloomBuffer.SetRenderTarget(MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBufferID);
            _BloomBuffer.SetGlobalTexture(MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBuffer, MobileBase_SRP_CommonValues.RenderTexture._BloomPassFrameBufferID);
        }
        _BloomBuffer.EndSample("Bloom Buffer");
        _Context.ExecuteCommandBuffer(_BloomBuffer);
        _BloomBuffer.Clear();
    }
}