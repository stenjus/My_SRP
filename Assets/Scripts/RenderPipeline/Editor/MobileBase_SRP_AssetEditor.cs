using UnityEditor;
using UnityEngine;
using System;

public enum LightColorSpace
{
    _UseLinear = 0,
    _UseLegaacy = 1
}

public enum BatchingType
{
    _NoBatching = 0,
    _UseGPU = 1,
    _UseDynamic = 2
}


[CustomEditor(typeof(MobileBase_SRP_Asset))]
public class MobileBase_SRP_AssetEditor : Editor
{
    //Preset Variables
    SerializedProperty _ShaderErrorMaterial;
    SerializedProperty _UseLinearLightIntencity;
    SerializedProperty _GPUInstancing;
    SerializedProperty _InDynamicBatching;
    SerializedProperty _RenderMesh;
    SerializedProperty _RenderMaterial;
    SerializedProperty _DownScaleValue;
    SerializedProperty _useBloom;
    SerializedProperty _useFishEye;
    SerializedProperty _useVignetting;
    SerializedProperty _useLUT;
    SerializedProperty _DualFiltering;
    SerializedProperty _BlurOffsetDown;
    SerializedProperty _BlurOffsetUp;
    SerializedProperty _BluumPasses;
    SerializedProperty _UseChromaticAberration;
    SerializedProperty _DefaultPostPreset;

    //Enums
    LightColorSpace _lightColorSpaceEnums;
    BatchingType _batchingTypeEnums;

    //Foldout bools
    bool _commonPipelineProps = true;
    bool _batchingProps = false;
    bool _RenderTargetProps = false;
    bool _PostprocessingPorps = false;

    //Additional Bools
    bool _useSubResolution = false;

    //GUI Content vars
    GUIContent _HeaderContent;
    GUIContent _HeaderVersion = new GUIContent("Version: 0.1");
    GUIContent _MeshTargetContent = new GUIContent("Mesh Target:");
    GUIContent _RenderMaterialContent = new GUIContent("Render Material:");
    GUIContent _ResolutionDownscaleContent = new GUIContent("Resolution Downscale:");
    GUIContent _UseSubResContent = new GUIContent("Use Sub Resolution:");
    GUIContent _UseFishEyeContent = new GUIContent("Use FishEye:");
    GUIContent _UseVignettingContent = new GUIContent("Use Vignetting:");
    GUIContent _UseBloomContent = new GUIContent("Use Bloom:");
    GUIContent _UseLUTContent = new GUIContent("Use LUT Grading:");
    GUIContent _PostGlobalTogglesLableContent = new GUIContent("Global post toggles:");
    GUIContent _SubResLableContent = new GUIContent("Subresolution:");
    GUIContent _BlurFiltering = new GUIContent("Blur Filtering:");
    GUIContent _DualMaterialContent = new GUIContent("Dual Filtering Matrerial:");
    GUIContent _BlurOffsetDownContent = new GUIContent("Filtering Offset Down:");
    GUIContent _BlurOffsetUpContent = new GUIContent("Filtering Offset Up:");
    GUIContent _BluumPassesContent = new GUIContent("Bloom passes count:");
    GUIContent _UseChromaticAberrationContent = new GUIContent("Use Chromatic Aberration:");
    GUIContent _DefaultPostPresetContent = new GUIContent("Default PostProcessing Preset:");

    public override void OnInspectorGUI()
    {
        //Header
        Texture2D _headerImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Scripts/RenderPipeline/Sources/Textures/MobileBaseSRP_Header.psd", typeof(Texture2D));
        _HeaderContent = new GUIContent(_headerImage);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_HeaderContent, GUILayout.MaxHeight(30f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(_HeaderVersion, EditorStyles.miniLabel);
        GUILayout.EndHorizontal();


        //Common Setup Block
        CustomUI.GuiLineSeparator(1);
        _commonPipelineProps = CustomUI.FoldOut("Common Pipeline properties", _commonPipelineProps);
        EditorGUILayout.Separator();

        if (_commonPipelineProps)
        {
            _ShaderErrorMaterial = serializedObject.FindProperty("_ShaderErrorMaterial");
            _UseLinearLightIntencity = serializedObject.FindProperty("_UseLinearLightIntencity");

            //Shader Error material field
            EditorGUILayout.ObjectField(_ShaderErrorMaterial);
            serializedObject.ApplyModifiedProperties();
            if (_ShaderErrorMaterial.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Warning: Shader Error material is not assigned. Please assign one!", MessageType.Warning);
            }

            //Light Attenuation type selector
            _lightColorSpaceEnums = (LightColorSpace)EditorGUILayout.EnumPopup("Light Attenuation Formule:", _lightColorSpaceEnums);
            ChangeLightAttenuationType(_lightColorSpaceEnums);
        }

        //Batching Setup Block
        CustomUI.GuiLineSeparator(2);
        _batchingProps = CustomUI.FoldOut("Batching Settings", _batchingProps);
        EditorGUILayout.Separator();
        if (_batchingProps)
        {
            _GPUInstancing = serializedObject.FindProperty("_GPUInstancing");
            _InDynamicBatching = serializedObject.FindProperty("_InDynamicBatching");
            _batchingTypeEnums = (BatchingType)EditorGUILayout.EnumPopup("Batching Type:", _batchingTypeEnums);
            ChangeBatchingType(_batchingTypeEnums);
        }

        //Render Target Setup Block
        CustomUI.GuiLineSeparator(2);
        _RenderTargetProps = CustomUI.FoldOut("Render Target Properties", _RenderTargetProps);
        EditorGUILayout.Separator();

        if (_RenderTargetProps)
        {
            _RenderMesh = serializedObject.FindProperty("_RenderMesh");
            _RenderMaterial = serializedObject.FindProperty("_RenderMaterial");
            EditorGUILayout.ObjectField(_RenderMesh, _MeshTargetContent);
            if (_RenderMesh.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Warning: Target mesh filter is not assigned. Please assign one!", MessageType.Warning);
            }
            EditorGUILayout.ObjectField(_RenderMaterial, _RenderMaterialContent);
            if (_RenderMaterial.objectReferenceValue == null)
            {
                _RenderMaterial.objectReferenceValue = new Material(Shader.Find("Hidden/CameraBlit"));
                EditorGUILayout.HelpBox("Warning: Render material is not assigned. Please assign one!", MessageType.Warning);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Postprocessing Settings Block
        CustomUI.GuiLineSeparator(2);
        _PostprocessingPorps = CustomUI.FoldOut("Postprocessing", _PostprocessingPorps);
        EditorGUILayout.Separator();
        if (_PostprocessingPorps)
        {
            //Usegae tip
            EditorGUILayout.HelpBox("Create PostProcessing preset and assign it to scenes PostProcess controller", MessageType.Info);

            //Global toggles
            CustomUI.GuiLineSeparator(1);

            _useBloom = serializedObject.FindProperty("_useBloom");
            _useFishEye = serializedObject.FindProperty("_useFishEye");
            _useVignetting = serializedObject.FindProperty("_useVignetting");
            _useLUT = serializedObject.FindProperty("_useLUT");
            _UseChromaticAberration = serializedObject.FindProperty("_UseChromaticAberration");

            EditorGUILayout.LabelField(_PostGlobalTogglesLableContent, EditorStyles.boldLabel);
            _useBloom.boolValue = EditorGUILayout.Toggle(_UseBloomContent, _useBloom.boolValue);
            _useFishEye.boolValue = EditorGUILayout.Toggle(_UseFishEyeContent, _useFishEye.boolValue);
            _useVignetting.boolValue = EditorGUILayout.Toggle(_UseVignettingContent, _useVignetting.boolValue);
            _UseChromaticAberration.boolValue = EditorGUILayout.Toggle(_UseChromaticAberrationContent, _UseChromaticAberration.boolValue);
            _useLUT.boolValue = EditorGUILayout.Toggle(_UseLUTContent, _useLUT.boolValue);

            CustomUI.GuiLineSeparator(1);

            _DefaultPostPreset = serializedObject.FindProperty("_DefaultPostPreset");
            EditorGUILayout.ObjectField(_DefaultPostPreset, _DefaultPostPresetContent);

            CustomUI.GuiLineSeparator(1);

            EditorGUILayout.LabelField(_SubResLableContent, EditorStyles.boldLabel); 
            _DownScaleValue = serializedObject.FindProperty("_DownScaleValue");
            _useSubResolution = EditorGUILayout.Toggle(_UseSubResContent, _useSubResolution);
            if (_useSubResolution)
            {
                EditorGUILayout.IntSlider(_DownScaleValue, 1, 10, _ResolutionDownscaleContent);
            }
            else _DownScaleValue.intValue = 1;

            CustomUI.GuiLineSeparator(1);

            EditorGUILayout.LabelField(_BlurFiltering, EditorStyles.boldLabel);
            _DualFiltering = serializedObject.FindProperty("_DualFiltering");
            _BlurOffsetDown = serializedObject.FindProperty("_BlurOffsetDown");
            _BlurOffsetUp = serializedObject.FindProperty("_BlurOffsetUp");
            _BluumPasses = serializedObject.FindProperty("_BluumPasses");
            EditorGUILayout.ObjectField(_DualFiltering, _DualMaterialContent);
            EditorGUILayout.Slider(_BlurOffsetDown, 0f, 10f, _BlurOffsetDownContent);
            EditorGUILayout.Slider(_BlurOffsetUp, 0f, 10f, _BlurOffsetUpContent);
            EditorGUILayout.IntSlider(_BluumPasses, 3, 10, _BluumPassesContent);
            
            serializedObject.ApplyModifiedProperties();
        }
        //CustomUI.GuiLineSeparator(10);
        //base.OnInspectorGUI();
    }

    void ChangeLightAttenuationType(LightColorSpace _lightColorSpaceEnums)
    {
        switch (_lightColorSpaceEnums)
        {
            case LightColorSpace._UseLinear:
                _UseLinearLightIntencity.boolValue = true;
                serializedObject.ApplyModifiedProperties();
                break;
            case LightColorSpace._UseLegaacy:
                _UseLinearLightIntencity.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
    }

    void ChangeBatchingType (BatchingType _batchingTypeEnums)
    {
        switch (_batchingTypeEnums)
        {
            case BatchingType._NoBatching:
                _InDynamicBatching.boolValue = false;
                _GPUInstancing.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            case BatchingType._UseGPU:
                _InDynamicBatching.boolValue = false;
                _GPUInstancing.boolValue = true;
                serializedObject.ApplyModifiedProperties();
                break;
            case BatchingType._UseDynamic:
                _InDynamicBatching.boolValue = true;
                _GPUInstancing.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
    }
}
