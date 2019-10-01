using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public partial class MobileBaseSRP : RenderPipeline
{

    //SRP Main Variables
    private ScriptableCullingParameters cullingParams;
    private CameraClearFlags clearFlags;
    private CullingResults cull;
    private DrawingSettings drawSettings;
    private FilteringSettings filterSettings;
    private SortingSettings   sortingSettings;

    //Command Buffers
    readonly CommandBuffer cameraBuffer             = new CommandBuffer { name = "Render Camera Buffer" };
    readonly CommandBuffer clearBuffer              = new CommandBuffer { name = "Clear Buffer" };
    readonly CommandBuffer blitBuffer               = new CommandBuffer { name = "Blit Buffer" };
    readonly CommandBuffer bloomBuffer              = new CommandBuffer { name = "Bloom Buffer" };
    readonly CommandBuffer lightingBuffer           = new CommandBuffer { name = "Lighting Buffer" };
    readonly CommandBuffer postProcessingBuffer     = new CommandBuffer { name = "PostProcessing Buffer" };

    //Preset Imported
    private MobileBaseSRPAsset pipeLineAsset;
    private Material errorMaterial;
    private Material renderMaterial;
    private Mesh renderMesh;

    //Lighting variable
    static readonly int MaxVisibleLights = 16;
    static readonly int VisibleLightColorId = Shader.PropertyToID("_VisibleLightColor");
    static readonly int VisibleLightDirectionOrPositionId = Shader.PropertyToID("_VisibleLightDirectionOrPosition");
    static readonly int MaxVisibleLightsId = Shader.PropertyToID("_MaxVisibleLights");
    static readonly int VisibleLightAttenuationId = Shader.PropertyToID("_VisibleLightAttenuation");
    static readonly int VisibleLightSpotDirectionsId = Shader.PropertyToID("_visibleLightSpotDirections");

    private Vector4[] visibleLightColors;
    private Vector4[] visibleLightDirectionOrPosition;
    private Vector4[] visibleLightAttenuations = new Vector4[MaxVisibleLights];
    private Vector4[] visibleLightSpotDirections = new Vector4[MaxVisibleLights];

    //Post Processing values
    private MobileBaseSRPPostProcessPreset postSettingsSource;

    //SubRes Settings
    private int lowResW;
    private int lowResH;

    //Constructor
    public MobileBaseSRP(MobileBaseSRPAsset inPipeLineAsset)
    {
        pipeLineAsset = inPipeLineAsset;
        errorMaterial = pipeLineAsset.ShaderErrorMaterial;
        
        //Lighting
        visibleLightColors = new Vector4[MaxVisibleLights];
        visibleLightDirectionOrPosition = new Vector4[MaxVisibleLights];

        //Set Linear light intensity for our pipeline
        GraphicsSettings.lightsUseLinearIntensity = pipeLineAsset.UseLinearLightIntensity;
    }

    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        RenderPipeline.BeginFrameRendering(renderContext, cameras);
        foreach (var camera in cameras)
        {
            Render(renderContext, camera);
        }
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        //Setup culling for current camera
        if (!camera.TryGetCullingParameters(out cullingParams))
        {
            return;
        }
        camera.TryGetCullingParameters(out cullingParams);
        cull = context.Cull(ref cullingParams);

        //Render Unity UI in SceneView
        #if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
        #endif

        context.SetupCameraProperties(camera); //SetUp Camera Matrix (transformation)

        //Select between scene local or global post processing presets
        if (pipeLineAsset.MobileBaseSrpPostProcessController == null || pipeLineAsset.MobileBaseSrpPostProcessController.PostProcessPreset == null)
        {
            postSettingsSource = pipeLineAsset.DefaultPostPreset;
        }
        else
        {
            postSettingsSource = pipeLineAsset.MobileBaseSrpPostProcessController.PostProcessPreset;
        }

        //Invoke SetPostprocessingValues function
        SetPostprocessingValues(context);

        //Set down scaled resolutuion
        SetDownScaledResolution(camera);

        //Define render target as Temporary render texture
        MobileBaseSRPCommonValues.RenderTexture.FrameBufferId = new RenderTargetIdentifier(MobileBaseSRPCommonValues.RenderTexture.FrameBuffer);
        MobileBaseSRPCommonValues.RenderTexture.FrameBufferDescriptor =
            new RenderTextureDescriptor(lowResW, lowResH, RenderTextureFormat.ARGBHalf, 32)
            {
                memoryless = RenderTextureMemoryless.Depth
            };
        
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) MobileBaseSRPCommonValues.RenderTexture.FrameBufferDescriptor.memoryless |= RenderTextureMemoryless.MSAA;

        blitBuffer.GetTemporaryRT(MobileBaseSRPCommonValues.RenderTexture.FrameBuffer, MobileBaseSRPCommonValues.RenderTexture.FrameBufferDescriptor, FilterMode.Bilinear);
        blitBuffer.SetRenderTarget(MobileBaseSRPCommonValues.RenderTexture.FrameBufferId,    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                                                                                                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        context.ExecuteCommandBuffer(blitBuffer);
        blitBuffer.Clear();

        clearFlags = camera.clearFlags; //Setup clear flags from from active camera
        clearBuffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth) != 0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);

        context.ExecuteCommandBuffer(clearBuffer);
        clearBuffer.Clear();

        //Invoke Lighting function
        ConfigureLights(context);

        //FrameDebugger Sampling
        cameraBuffer.BeginSample("Render Camera Buffer");

        //Define SortingSettings and DrawSettings
        sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque }; //Set sorting flags for opaque render
        drawSettings = new DrawingSettings(new ShaderTagId("Forward"), sortingSettings)
        {enableDynamicBatching = pipeLineAsset.InDynamicBatching, enableInstancing = pipeLineAsset.GpuInstancing, perObjectData = PerObjectData.LightIndices};
        
        //Opaque Filter and Render
        filterSettings = new FilteringSettings(RenderQueueRange.opaque, camera.cullingMask);
        context.DrawRenderers(cull, ref drawSettings, ref filterSettings);
        
        //Draw none MBSRP renders
        #if UNITY_EDITOR
        DrawDefaultPipeline(context);
        #endif

        //SkyBox Rendering before Transparent
        context.DrawSkybox(camera);

        //Transparent Filter and Render
        sortingSettings.criteria        = SortingCriteria.CommonTransparent;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;

        //Finally Draw Culled and filtered renders
        context.DrawRenderers(cull, ref drawSettings, ref filterSettings);

        cameraBuffer.EndSample("Render Camera Buffer");

        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        //Get final image on screen
        blitBuffer.SetGlobalTexture("_FrameBuffer", MobileBaseSRPCommonValues.RenderTexture.FrameBufferId);
        blitBuffer.Blit(MobileBaseSRPCommonValues.RenderTexture.FrameBufferId, BuiltinRenderTextureType.CameraTarget);
        blitBuffer.DrawMesh(pipeLineAsset.RenderMesh, Matrix4x4.identity, pipeLineAsset.RenderMaterial);
        
        //Execute Bloom postprocess
        if (pipeLineAsset.UseGlobalBloom && postSettingsSource.UseBloom)
        {
            BloomPost(context, lowResW / 2, lowResH / 2);
        }
        
        context.ExecuteCommandBuffer(blitBuffer);
        blitBuffer.Clear();
        context.Submit();
    }

    //Default Unity Pipeline used for rendering Unity included shader side Editor or in Development builds
    [Conditional ("UNITY_EDITOR"), Conditional ("DEVELOPMENT_BUILD")]
    private void DrawDefaultPipeline(ScriptableRenderContext contex)
    {
        if (errorMaterial == null)
        {
            Debug.Log("Shader Error Material not assigned");
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        var drawingSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), sortingSettings);
        drawingSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawingSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawingSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawingSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawingSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        drawingSettings.overrideMaterial = errorMaterial;
        drawingSettings.overrideMaterialPassIndex = 0;

        contex.DrawRenderers(cull, ref drawingSettings, ref filterSettings);
    }

    private void SetDownScaledResolution(Camera camera)
    {
        if (pipeLineAsset.DownScaleValue > 1)
        {
            lowResW = camera.pixelWidth / pipeLineAsset.DownScaleValue;
            lowResH = camera.pixelHeight / pipeLineAsset.DownScaleValue;
        }
        else
        {
            lowResW = camera.pixelWidth;
            lowResH = camera.pixelHeight;
        }
    }

    private void SetPostprocessingValues(ScriptableRenderContext context)
    {
        //Set postProcessing values to the Camera Blit Shader
        //
        //Set global values and keywords for vignetting
        if (pipeLineAsset.UseGlobalVignetting)
        {
            if (postSettingsSource == null)
                postSettingsSource = (MobileBaseSRPPostProcessPreset)ScriptableObject.CreateInstance(typeof(MobileBaseSRPPostProcessPreset));

            if (postSettingsSource.UseVignetting)
            {
                postProcessingBuffer.SetGlobalColor("_VignettingColor", postSettingsSource.VignettingColor);
                postProcessingBuffer.SetGlobalFloat("_Vignetting_Size", postSettingsSource.VignettingSize);
                postProcessingBuffer.SetGlobalFloat("_Vignetting_Contrast", postSettingsSource.VignettingContrast);
                postProcessingBuffer.EnableShaderKeyword("VIGNETTING_ON");
            }
            else
            {
                postProcessingBuffer.DisableShaderKeyword("VIGNETTING_ON");
            }
        }
        else
        {
            postProcessingBuffer.DisableShaderKeyword("VIGNETTING_ON");
        }
        //
        //Set global values and keywords for vignetting
        if (pipeLineAsset.UseGlobalLut)
        {
            if (postSettingsSource.UseLUTGrading)
            {
                postProcessingBuffer.SetGlobalFloat("_LUT_Power", postSettingsSource.LUTPower);
                postProcessingBuffer.SetGlobalTexture("_LUT_Tex", postSettingsSource.LUTTex);
                postProcessingBuffer.EnableShaderKeyword("LUT_ON");
            }
            else
            {
                postProcessingBuffer.DisableShaderKeyword("LUT_ON");
            }
        }
        else
        {
            postProcessingBuffer.DisableShaderKeyword("LUT_ON");
        }
        //
        //Set global values and keywords for vignetting
        if (pipeLineAsset.UseGlobalFishEye)
        {
            if (postSettingsSource.UseFishEyeFragment)
            {
                postProcessingBuffer.SetGlobalFloat("_FishEye", postSettingsSource.FishEyePower);
                postProcessingBuffer.EnableShaderKeyword("FISHEYE_ON_FRAGMENT");
            }
            else
            {
                postProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_FRAGMENT");
            }
            if (postSettingsSource.UseFishEyeVertex)
            {
                postProcessingBuffer.SetGlobalFloat("_FishEye", postSettingsSource.FishEyePower);
                postProcessingBuffer.EnableShaderKeyword("FISHEYE_ON_VERTEX");
            }
            else
            {
                postProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_VERTEX");
            }
        }
        else
        {
            postProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_FRAGMENT");
            postProcessingBuffer.DisableShaderKeyword("FISHEYE_ON_VERTEX");
        }
        //
        //Set global values and keywords for Bloom
        if (pipeLineAsset.UseGlobalBloom && postSettingsSource.UseBloom)
        {
            postProcessingBuffer.SetGlobalFloat("_BloomIntencity", postSettingsSource.BloomIntensity);
            postProcessingBuffer.EnableShaderKeyword("BLOOM_ON");
        }
        else
        {
            postProcessingBuffer.DisableShaderKeyword("BLOOM_ON");
        }
        //
        //Set global values and keywords for Chromatic Aberration
        if (pipeLineAsset.UseChromaticAberration && postSettingsSource.UseChromaticAberration)
        {
            postProcessingBuffer.SetGlobalFloat("_Chromatic_Aberration_Offset", postSettingsSource.ChromaticAberrationOffset);
            postProcessingBuffer.SetGlobalFloat("_Chromatic_Aberration_Radius", postSettingsSource.ChromaticAberrationRadius);
            postProcessingBuffer.EnableShaderKeyword("CHROMATIC_ABERRATION");
        }
        else
        {
            postProcessingBuffer.DisableShaderKeyword("CHROMATIC_ABERRATION");
        }


        context.ExecuteCommandBuffer(postProcessingBuffer);
        postProcessingBuffer.Clear();
    }

    private void ConfigureLights(ScriptableRenderContext context)
    {
        for (int i = 0; i < cull.visibleLights.Length; i++)
        {
            //Abort the loop when sending more than maximum supported lights
            if (i == MaxVisibleLights)
            {
                break;
            }

            //Define visible lights at index
            VisibleLight light = cull.visibleLights[i];

            //Set lights colors array
            visibleLightColors[i] = light.finalColor;

            Vector4 attenuation = Vector4.zero;
            attenuation.w = 1.0f;

            //Negate directional light direction
            if(light.lightType == LightType.Directional)
            { 
                Vector4 v = light.localToWorldMatrix.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;

                //Set directional lights directions array
                visibleLightDirectionOrPosition[i] = v;
            }
            else
            {
                visibleLightDirectionOrPosition[i] = light.localToWorldMatrix.GetColumn(3);
                attenuation.x = 1.0f / Mathf.Max(light.range * light.range, 0.00001f);


                
                if (light.lightType == LightType.Spot)
                {
                    //Negate spot light direction
                    Vector4 v = light.localToWorldMatrix.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;

                    //Set spot lights directions array
                    visibleLightSpotDirections[i] = v;

                    //Angle Falloff calculations (A pice of mathf magic to make shit works)
                    float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                    float outerCos = Mathf.Cos(outerRad);
                    float outerTan = Mathf.Tan(outerRad);
                    float innerCos = Mathf.Cos(Mathf.Atan((46.0f / 64.0f) * outerTan));
                    float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                    attenuation.z = 1f / angleRange;
                    attenuation.w = -outerCos * attenuation.z;
                }
            }

            visibleLightAttenuations[i] = attenuation;
        }

        //Send Data to GPU CBUFFER's
        lightingBuffer.SetGlobalVectorArray(VisibleLightColorId, visibleLightColors);
        lightingBuffer.SetGlobalVectorArray(VisibleLightDirectionOrPositionId, visibleLightDirectionOrPosition);
        lightingBuffer.SetGlobalInt(MaxVisibleLightsId, MaxVisibleLights);
        lightingBuffer.SetGlobalVectorArray(VisibleLightAttenuationId, visibleLightAttenuations);
        lightingBuffer.SetGlobalVectorArray(VisibleLightSpotDirectionsId, visibleLightSpotDirections);

        context.ExecuteCommandBuffer(lightingBuffer);
        lightingBuffer.Clear();
    }

    private void BloomPost(ScriptableRenderContext context, int screenWidth, int screenHeight)
    {
        Material dualFilterMat = pipeLineAsset.DualFiltering;
        int blurOffsetDown = Shader.PropertyToID("_BlurOffsetDown");
        int blurOffsetUp = Shader.PropertyToID("_BlurOffsetUp");
        int brightId = Shader.PropertyToID("_BrightID");
        int bloomResult = Shader.PropertyToID("_BloomResult");
        int passes = pipeLineAsset.BloomPasses;
        int[] downId = new int[passes];
        int[] upId = new int[passes];

        //Set Offset to the shader
        bloomBuffer.SetGlobalFloat(blurOffsetDown, pipeLineAsset.BlurOffsetDown);
        bloomBuffer.SetGlobalFloat(blurOffsetUp, pipeLineAsset.BlurOffsetUp);

        //IDs Loop
        for (int i = 0; i < passes; i++)
        {
            downId[i] = Shader.PropertyToID("_DownID" + i);
            upId[i] = Shader.PropertyToID("_UpID" + i);
        }

        //Bright Pass
        bloomBuffer.GetTemporaryRT(brightId, screenWidth, screenHeight, 0, FilterMode.Bilinear);
        bloomBuffer.Blit(MobileBaseSRPCommonValues.RenderTexture.FrameBufferId, brightId, dualFilterMat, 2);

        //DownScale Pass
        for (int i = 0; i < passes; i++)
        {
            bloomBuffer.GetTemporaryRT(downId[i], screenWidth >> i, screenHeight >> i, 0, FilterMode.Bilinear);
            if (i == 0)
            {
                bloomBuffer.Blit(brightId, downId[i], dualFilterMat, 0);
            }
            else bloomBuffer.Blit(downId[i -1], downId[i], dualFilterMat, 0);
        }
        
        //UpScale Pass
        for (int i = passes - 1; i > 0; i--)
        {
            bloomBuffer.GetTemporaryRT(upId[i], screenWidth >> i, screenHeight >> i, 0, FilterMode.Bilinear);
            if (i == passes - 1)
            {
                bloomBuffer.Blit(downId[i], upId[i], dualFilterMat, 1);
            }
            else
            {
                bloomBuffer.Blit(upId[i + 1], upId[i], dualFilterMat, 1);
            }
            bloomBuffer.SetGlobalTexture("_DownPass", downId[i]);
        }
        
        bloomBuffer.GetTemporaryRT(bloomResult, screenWidth, screenHeight, 0, FilterMode.Bilinear);
        bloomBuffer.Blit(upId[1], bloomResult, dualFilterMat, 1);
        bloomBuffer.SetGlobalTexture("_BloomResult", bloomResult);

        //CleanUp Chain
        for (int i = 0; i < passes; i++)
        {
            bloomBuffer.ReleaseTemporaryRT(downId[i]);
        }
        for (int i = passes - 1; i > 0; i--)
        {
            bloomBuffer.ReleaseTemporaryRT(upId[i]);
        }

        context.ExecuteCommandBuffer(bloomBuffer);
        bloomBuffer.Clear();
    }
}
