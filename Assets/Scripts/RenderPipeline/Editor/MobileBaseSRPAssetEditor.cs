using UnityEditor;
using UnityEngine;

public enum LightColorSpace
{
    UseLinear = 0,
    UseLegacy = 1
}

public enum BatchingType
{
    NoBatching = 0,
    UseGpu = 1,
    UseDynamic = 2
}


[CustomEditor(typeof(MobileBaseSRPAsset))]
public class MobileBaseSRPAssetEditor : Editor
{
    //Preset Variables
    SerializedProperty shaderErrorMaterial;
    SerializedProperty defaultMaterialOverride;
    SerializedProperty useLinearLightIntensity;
    SerializedProperty gpuInstancing;
    SerializedProperty inDynamicBatching;
    SerializedProperty renderMesh;
    SerializedProperty renderMaterial;
    SerializedProperty downScaleValue;
    SerializedProperty useBloom;
    SerializedProperty useFishEye;
    SerializedProperty useVignetting;
    SerializedProperty useLut;
    SerializedProperty dualFiltering;
    SerializedProperty blurOffsetDown;
    SerializedProperty blurOffsetUp;
    SerializedProperty bluumPasses;
    SerializedProperty useChromaticAberration;
    SerializedProperty defaultPostPreset;

    //Enums
    LightColorSpace lightColorSpaceEnums;
    BatchingType batchingTypeEnums;

    //Foldout bools
    bool commonPipelineProps = true;
    bool batchingProps = false;
    bool renderTargetProps = false;
    bool postprocessingPorps = false;

    //Additional Bools
    bool useSubResolution = false;

    //GUI Content vars
    GUIContent headerContent;
    readonly GUIContent headerVersion = new GUIContent("Version: 0.1");
    readonly GUIContent meshTargetContent = new GUIContent("Mesh Target:");
    readonly GUIContent renderMaterialContent = new GUIContent("Render Material:");
    readonly GUIContent resolutionDownscaleContent = new GUIContent("Resolution Downscale:");
    readonly GUIContent useSubResContent = new GUIContent("Use Sub Resolution:");
    readonly GUIContent useFishEyeContent = new GUIContent("Use FishEye:");
    readonly GUIContent useVignettingContent = new GUIContent("Use Vignetting:");
    readonly GUIContent useBloomContent = new GUIContent("Use Bloom:");
    readonly GUIContent useLutContent = new GUIContent("Use LUT Grading:");
    readonly GUIContent postGlobalTogglesLableContent = new GUIContent("Global post toggles:");
    readonly GUIContent subResLableContent = new GUIContent("Subresolution:");
    readonly GUIContent blurFiltering = new GUIContent("Blur Filtering:");
    readonly GUIContent dualMaterialContent = new GUIContent("Dual Filtering Material:");
    readonly GUIContent blurOffsetDownContent = new GUIContent("Filtering Offset Down:");
    readonly GUIContent blurOffsetUpContent = new GUIContent("Filtering Offset Up:");
    readonly GUIContent bluumPassesContent = new GUIContent("Bloom passes count:");
    readonly GUIContent useChromaticAberrationContent = new GUIContent("Use Chromatic Aberration:");
    readonly GUIContent defaultPostPresetContent = new GUIContent("Default PostProcessing Preset:");

    public override void OnInspectorGUI()
    {
        //Header
        Texture2D headerImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Scripts/RenderPipeline/Sources/Textures/MobileBaseSRP_Header.psd", typeof(Texture2D));
        headerContent = new GUIContent(headerImage);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(headerContent, GUILayout.MaxHeight(30f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(headerVersion, EditorStyles.miniLabel);
        GUILayout.EndHorizontal();


        //Common Setup Block
        CustomUi.GuiLineSeparator(1);
        commonPipelineProps = CustomUi.FoldOut("Common Pipeline properties", commonPipelineProps);
        EditorGUILayout.Separator();

        if (commonPipelineProps)
        {
            shaderErrorMaterial = serializedObject.FindProperty("ShaderErrorMaterial");
            useLinearLightIntensity = serializedObject.FindProperty("UseLinearLightIntensity");
            defaultMaterialOverride = serializedObject.FindProperty("defaultMaterialOverride");

            //Shader Error material field
            EditorGUILayout.ObjectField(shaderErrorMaterial);
            if (shaderErrorMaterial.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Warning: Shader Error material is not assigned. Please assign one!", MessageType.Warning);
            }
            
            EditorGUILayout.ObjectField(defaultMaterialOverride);
            if (defaultMaterialOverride.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Warning: Default Unlit material is not assigned. Please assign one!", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
            
            //Light Attenuation type selector
            lightColorSpaceEnums = (LightColorSpace)EditorGUILayout.EnumPopup("Light Attenuation Formule:", lightColorSpaceEnums);
            ChangeLightAttenuationType(lightColorSpaceEnums);
        }

        //Batching Setup Block
        CustomUi.GuiLineSeparator(2);
        batchingProps = CustomUi.FoldOut("Batching Settings", batchingProps);
        EditorGUILayout.Separator();
        if (batchingProps)
        {
            gpuInstancing = serializedObject.FindProperty("GpuInstancing");
            inDynamicBatching = serializedObject.FindProperty("InDynamicBatching");
            batchingTypeEnums = (BatchingType)EditorGUILayout.EnumPopup("Batching Type:", batchingTypeEnums);
            ChangeBatchingType(batchingTypeEnums);
        }

        //Render Target Setup Block
        CustomUi.GuiLineSeparator(2);
        renderTargetProps = CustomUi.FoldOut("Render Target Properties", renderTargetProps);
        EditorGUILayout.Separator();

        if (renderTargetProps)
        {
            renderMesh = serializedObject.FindProperty("RenderMesh");
            renderMaterial = serializedObject.FindProperty("RenderMaterial");
            EditorGUILayout.ObjectField(renderMesh, meshTargetContent);
            if (renderMesh.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Warning: Target mesh filter is not assigned. Please assign one!", MessageType.Warning);
            }
            EditorGUILayout.ObjectField(renderMaterial, renderMaterialContent);
            if (renderMaterial.objectReferenceValue == null)
            {
                renderMaterial.objectReferenceValue = new Material(Shader.Find("Hidden/CameraBlit"));
                EditorGUILayout.HelpBox("Warning: Render material is not assigned. Please assign one!", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Postprocessing Settings Block
        CustomUi.GuiLineSeparator(2);
        postprocessingPorps = CustomUi.FoldOut("Postprocessing", postprocessingPorps);
        EditorGUILayout.Separator();
        if (postprocessingPorps)
        {
            //Usage tip
            EditorGUILayout.HelpBox("Create PostProcessing preset and assign it to scenes PostProcess controller", MessageType.Info);

            //Global toggles
            CustomUi.GuiLineSeparator(1);

            useBloom = serializedObject.FindProperty("UseGlobalBloom");
            useFishEye = serializedObject.FindProperty("UseGlobalFishEye");
            useVignetting = serializedObject.FindProperty("UseGlobalVignetting");
            useLut = serializedObject.FindProperty("UseGlobalLut");
            useChromaticAberration = serializedObject.FindProperty("UseChromaticAberration");

            EditorGUILayout.LabelField(postGlobalTogglesLableContent, EditorStyles.boldLabel);
            useBloom.boolValue = EditorGUILayout.Toggle(useBloomContent, useBloom.boolValue);
            useFishEye.boolValue = EditorGUILayout.Toggle(useFishEyeContent, useFishEye.boolValue);
            useVignetting.boolValue = EditorGUILayout.Toggle(useVignettingContent, useVignetting.boolValue);
            useChromaticAberration.boolValue = EditorGUILayout.Toggle(useChromaticAberrationContent, useChromaticAberration.boolValue);
            useLut.boolValue = EditorGUILayout.Toggle(useLutContent, useLut.boolValue);

            CustomUi.GuiLineSeparator(1);

            defaultPostPreset = serializedObject.FindProperty("DefaultPostPreset");
            EditorGUILayout.ObjectField(defaultPostPreset, defaultPostPresetContent);

            CustomUi.GuiLineSeparator(1);

            EditorGUILayout.LabelField(subResLableContent, EditorStyles.boldLabel); 
            downScaleValue = serializedObject.FindProperty("DownScaleValue");
            useSubResolution = EditorGUILayout.Toggle(useSubResContent, useSubResolution);
            if (useSubResolution)
            {
                EditorGUILayout.IntSlider(downScaleValue, 1, 10, resolutionDownscaleContent);
            }
            else downScaleValue.intValue = 1;

            CustomUi.GuiLineSeparator(1);

            EditorGUILayout.LabelField(blurFiltering, EditorStyles.boldLabel);
            dualFiltering = serializedObject.FindProperty("DualFiltering");
            blurOffsetDown = serializedObject.FindProperty("BlurOffsetDown");
            blurOffsetUp = serializedObject.FindProperty("BlurOffsetUp");
            bluumPasses = serializedObject.FindProperty("BloomPasses");
            EditorGUILayout.ObjectField(dualFiltering, dualMaterialContent);
            EditorGUILayout.Slider(blurOffsetDown, 0f, 10f, blurOffsetDownContent);
            EditorGUILayout.Slider(blurOffsetUp, 0f, 10f, blurOffsetUpContent);
            EditorGUILayout.IntSlider(bluumPasses, 3, 10, bluumPassesContent);
            
            serializedObject.ApplyModifiedProperties();
        }
        //CustomUI.GuiLineSeparator(10);
        //base.OnInspectorGUI();
    }

    private void ChangeLightAttenuationType(LightColorSpace lightColorSpaceEnums)
    {
        switch (lightColorSpaceEnums)
        {
            case LightColorSpace.UseLinear:
                useLinearLightIntensity.boolValue = true;
                serializedObject.ApplyModifiedProperties();
                break;
            case LightColorSpace.UseLegacy:
                useLinearLightIntensity.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
    }

    private void ChangeBatchingType (BatchingType batchingTypeEnums)
    {
        switch (batchingTypeEnums)
        {
            case BatchingType.NoBatching:
                inDynamicBatching.boolValue = false;
                gpuInstancing.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            case BatchingType.UseGpu:
                inDynamicBatching.boolValue = false;
                gpuInstancing.boolValue = true;
                serializedObject.ApplyModifiedProperties();
                break;
            case BatchingType.UseDynamic:
                inDynamicBatching.boolValue = true;
                gpuInstancing.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
    }
}
