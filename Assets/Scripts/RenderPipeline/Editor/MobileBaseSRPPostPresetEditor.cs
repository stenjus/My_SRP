using UnityEditor;
using UnityEngine;
using System;

public enum FishEyeType
{
    UseVertexFishEye = 0,
    UseFragmentFishEye = 1
}

[CustomEditor(typeof(MobileBaseSRPPostProcessPreset))]
public class MobileBaseSRPPostPresetEditor : Editor
{
    //Preset Variables
    SerializedProperty useVignetting;
    SerializedProperty vignettingColor;
    SerializedProperty vignettingSize;
    SerializedProperty vignettingContrast;

    SerializedProperty useLutGrading;
    SerializedProperty lutTex;
    SerializedProperty lutPower;

    SerializedProperty useFishEyeFragment;
    SerializedProperty useFishEyeVertex;
    SerializedProperty fishEyePower;

    SerializedProperty useBloom;
    SerializedProperty bloomIntensity;

    SerializedProperty useChromaticAberration;
    SerializedProperty chromaticAberrationOffset;
    SerializedProperty chromaticAberrationRadius;



    //Additional Bools
    bool useFishEye = true;

    //Enums
    FishEyeType fishEyeType;

    //Foldout bools
    private bool vignettingFoldout = true;
    private bool lutFoldout = true;
    private bool fishEyeFoldout = true;
    private bool bloomFoldout = true;
    private bool chromaticAberrationFoldout = true;

    //GUI Content vars
    GUIContent headerContent;
    readonly GUIContent headerHeaderName = new GUIContent("Post Processing Preset");
    readonly GUIContent useVignettingContent = new GUIContent("Use Vignetting:");
    readonly GUIContent vignettingColorContent = new GUIContent("Vignetting Color:");
    readonly GUIContent vignettingSizeContent = new GUIContent("Vignetting Size:");
    readonly GUIContent vignettingContrastContent = new GUIContent("Vignetting Contrast:");
    readonly GUIContent useLutContent = new GUIContent("Use LUT Grading:");
    readonly GUIContent lutTexContent = new GUIContent("LUT Texture:");
    readonly GUIContent lutPowerContent = new GUIContent("LUT Contribution:");
    readonly GUIContent useFishEyeContent = new GUIContent("Use FishEye:");
    readonly GUIContent fishEyePowerContent = new GUIContent("FishEye Power:");
    readonly GUIContent useBloomContent = new GUIContent("Use Bloom:");
    readonly GUIContent bloomIntensityContent = new GUIContent("Bloom Intensity:");
    readonly GUIContent useChromaticAberrationContent = new GUIContent("Use Chromatic Aberration:");
    readonly GUIContent chromaticAberrationOffsetContent = new GUIContent("Chromatic Aberration Offset:");
    readonly GUIContent chromaticAberrationRadiusContent = new GUIContent("Chromatic Aberration Radius:");

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

        EditorGUILayout.LabelField(headerHeaderName, EditorStyles.boldLabel);

        //Vignetting Foldout
        CustomUi.GuiLineSeparator(1);
        vignettingFoldout = CustomUi.FoldOut("Vignetting", vignettingFoldout);
        EditorGUILayout.Separator();
        if (vignettingFoldout)
        {
            useVignetting = serializedObject.FindProperty("UseVignetting");
            vignettingColor = serializedObject.FindProperty("VignettingColor");
            vignettingSize = serializedObject.FindProperty("VignettingSize");
            vignettingContrast = serializedObject.FindProperty("VignettingContrast");

            useVignetting.boolValue = EditorGUILayout.Toggle(useVignettingContent, useVignetting.boolValue);
            if (useVignetting.boolValue)
            {
                vignettingColor.colorValue = EditorGUILayout.ColorField(vignettingColorContent, vignettingColor.colorValue);
                vignettingSize.floatValue = EditorGUILayout.FloatField(vignettingSizeContent, vignettingSize.floatValue);
                vignettingContrast.floatValue = EditorGUILayout.FloatField(vignettingContrastContent, vignettingContrast.floatValue);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //LUT Foldout
        CustomUi.GuiLineSeparator(2);
        lutFoldout = CustomUi.FoldOut("LUT Grading", lutFoldout);
        EditorGUILayout.Separator();
        if (lutFoldout)
        {
            useLutGrading = serializedObject.FindProperty("UseLUTGrading");
            lutTex = serializedObject.FindProperty("LUTTex");
            lutPower = serializedObject.FindProperty("LUTPower");

            useLutGrading.boolValue = EditorGUILayout.Toggle(useLutContent, useLutGrading.boolValue);
            if (useLutGrading.boolValue)
            {
                EditorGUILayout.ObjectField(lutTex, lutTexContent);
                EditorGUILayout.Slider(lutPower, 0.0f, 1.0f, lutPowerContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //FishEye Foldout
        CustomUi.GuiLineSeparator(2);
        fishEyeFoldout = CustomUi.FoldOut("FishEye Distortion", fishEyeFoldout);
        EditorGUILayout.Separator();
        if (fishEyeFoldout)
        {
            fishEyePower = serializedObject.FindProperty("FishEyePower");

            useFishEye = EditorGUILayout.Toggle(useFishEyeContent, useFishEye);
            if (useFishEye)
            {
                fishEyeType = (FishEyeType)EditorGUILayout.EnumPopup("FishEye Type:", fishEyeType);
                EditorGUILayout.Slider(fishEyePower, -1.0f, 2.0f, fishEyePowerContent);
            }
            ChangeFishEyeType(fishEyeType);
            serializedObject.ApplyModifiedProperties();
        }

        //Bloom Foldout
        CustomUi.GuiLineSeparator(2);
        bloomFoldout = CustomUi.FoldOut("Bloom", bloomFoldout);
        EditorGUILayout.Separator();
        if (bloomFoldout)
        {
            useBloom = serializedObject.FindProperty("UseBloom");
            bloomIntensity = serializedObject.FindProperty("BloomIntensity");

            useBloom.boolValue = EditorGUILayout.Toggle(useBloomContent, useBloom.boolValue);
            if(useBloom.boolValue)
            {
                EditorGUILayout.Slider(bloomIntensity, 0.0f, 5.0f, bloomIntensityContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Chromatic Aberration Foldout
        CustomUi.GuiLineSeparator(2);
        chromaticAberrationFoldout = CustomUi.FoldOut("Chromatic Aberration", chromaticAberrationFoldout);
        EditorGUILayout.Separator();
        if (chromaticAberrationFoldout)
        {
            useChromaticAberration = serializedObject.FindProperty("UseChromaticAberration");
            chromaticAberrationOffset = serializedObject.FindProperty("ChromaticAberrationOffset");
            chromaticAberrationRadius = serializedObject.FindProperty("ChromaticAberrationRadius");

            useChromaticAberration.boolValue = EditorGUILayout.Toggle(useChromaticAberrationContent, useChromaticAberration.boolValue);
            if (useChromaticAberration.boolValue)
            {
                EditorGUILayout.Slider(chromaticAberrationOffset, 0.001f, 0.1f, chromaticAberrationOffsetContent);
                EditorGUILayout.Slider(chromaticAberrationRadius, 0.0f, 1.0f, chromaticAberrationRadiusContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Usage Tip
        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox("To use this Preset in your scene, please add 'MSRP PostProcess Controller' to any object on the scene and assign this preset into it", MessageType.None);


        //CustomUI.GuiLineSeparator(10);
        //base.OnInspectorGUI();
    }

    void ChangeFishEyeType(FishEyeType fishEyeType)
    {
        useFishEyeFragment = serializedObject.FindProperty("UseFishEyeFragment");
        useFishEyeVertex = serializedObject.FindProperty("UseFishEyeVertex");
        switch (fishEyeType)
        {
            case FishEyeType.UseVertexFishEye:
                useFishEyeVertex.boolValue = true;
                useFishEyeFragment.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            case FishEyeType.UseFragmentFishEye:
                useFishEyeFragment.boolValue = true;
                useFishEyeVertex.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
        if (!useFishEye)
        {
            useFishEyeVertex.boolValue = false;
            useFishEyeFragment.boolValue = false;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
