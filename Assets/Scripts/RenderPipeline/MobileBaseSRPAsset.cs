﻿using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Presets/Mobile Base SRP Preset")]
public class MobileBaseSRPAsset : RenderPipelineAsset
{
    //Default materials
    public Material ShaderErrorMaterial;
    public Material defaultMaterialOverride;
    
    public bool InDynamicBatching;
    public bool GpuInstancing;
    public bool UseLinearLightIntensity = true;

    //Render to custom mesh vars
    public Mesh RenderMesh;
    public Material RenderMaterial;

    //Post Processing bools
    public bool UseGlobalBloom = false;
    public bool UseGlobalFishEye = true;
    public bool UseGlobalVignetting = true;
    public bool UseGlobalLut = true;
    public Material DualFiltering;
    public float BlurOffsetDown = 1;
    public float BlurOffsetUp = 1;
    public int BloomPasses = 4;
    public bool UseChromaticAberration = false;

    //Default Post Preset
    public MobileBaseSRPPostProcessPreset DefaultPostPreset;
    
    //Set default materials
    public override Material defaultMaterial => defaultMaterialOverride;
    public override Shader defaultShader => Shader.Find("My SRP/Unlit");

    [Range(0, 10)] public int DownScaleValue = 1;

    public MobileBaseSRPPostProcessController MobileBaseSrpPostProcessController;

	protected override RenderPipeline CreatePipeline()
	{
        return new MobileBaseSRP(this);
    }
}
