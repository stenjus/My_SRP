﻿using UnityEditor;
using UnityEngine;
using System;

public enum FishEyeType
{
    _useVertexFishEye = 0,
    _useFragmentFishEye = 1
}

[CustomEditor(typeof(MobileBase_SRP_PostProcess_Preset))]
public class MobileBase_SRP_PostPresetEditor : Editor
{
    //Preset Variables
    SerializedProperty _USE_Vignetting;
    SerializedProperty _VignettingColor;
    SerializedProperty _Vignetting_Size;
    SerializedProperty _Vignetting_Contrast;

    SerializedProperty _USE_LUTGrading;
    SerializedProperty _LUT_Tex;
    SerializedProperty _LUT_Power;

    SerializedProperty _USE_FishEye_Fragment;
    SerializedProperty _USE_FishEye_Vertex;
    SerializedProperty _FishEye_Power;

    SerializedProperty _USE_Bloom;
    SerializedProperty _BloomIntencity;

    SerializedProperty _USE_ChromaticAberration;
    SerializedProperty _Chromatic_Aberration_Offset;
    SerializedProperty _Chromatic_Aberration_Radius;

    //Additional Bools
    bool _USE_FishEye = true;

    //Enums
    FishEyeType _fishEyeType;

    //Foldout bools
    bool _VignettingFoldout = true;
    bool _LUTFoldout = true;
    bool _FishEyeFoldout = true;
    bool _BloomFoldout = true;
    bool _ChromaticAberrationFoldout = true;

    //GUI Content vars
    GUIContent _HeaderContent;
    GUIContent _HeaderHeaderName = new GUIContent("Post Processing Preset");
    GUIContent _USE_VignettingContent = new GUIContent("Use Vignetting:");
    GUIContent _VignettingColorContent = new GUIContent("Vignetting Color:");
    GUIContent _VignettingSizeContent = new GUIContent("Vignetting Size:");
    GUIContent _VignettingContrastContent = new GUIContent("Vignetting Contrast:");
    GUIContent _USE_LUTContent = new GUIContent("Use LUT Grading:");
    GUIContent _LUTTexContent = new GUIContent("LUT Texture:");
    GUIContent _LUTPowerContent = new GUIContent("LUT Contribution:");
    GUIContent _USE_FishEyeContent = new GUIContent("Use FishEye:");
    GUIContent _FishEyePowerContent = new GUIContent("FishEye Power:");
    GUIContent _USE_BloomContent = new GUIContent("Use Bloom:");
    GUIContent _BloomIntencityContent = new GUIContent("Bloom Intencity:");
    GUIContent _USE_ChromaticAberrationContent = new GUIContent("Use Chromatic Aberration:");
    GUIContent _Chromatic_Aberration_OffsetContent = new GUIContent("Chromatic Aberration Offset:");
    GUIContent _Chromatic_Aberration_RadiusContent = new GUIContent("Chromatic Aberration Radius:");

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

        EditorGUILayout.LabelField(_HeaderHeaderName, EditorStyles.boldLabel);

        //Vignetting Foldout
        CustomUI.GuiLineSeparator(1);
        _VignettingFoldout = CustomUI.FoldOut("Vignetting", _VignettingFoldout);
        EditorGUILayout.Separator();
        if (_VignettingFoldout)
        {
            _USE_Vignetting = serializedObject.FindProperty("_USE_Vignetting");
            _VignettingColor = serializedObject.FindProperty("_VignettingColor");
            _Vignetting_Size = serializedObject.FindProperty("_Vignetting_Size");
            _Vignetting_Contrast = serializedObject.FindProperty("_Vignetting_Contrast");

            _USE_Vignetting.boolValue = EditorGUILayout.Toggle(_USE_VignettingContent, _USE_Vignetting.boolValue);
            if (_USE_Vignetting.boolValue)
            {
                _VignettingColor.colorValue = EditorGUILayout.ColorField(_VignettingColorContent, _VignettingColor.colorValue);
                _Vignetting_Size.floatValue = EditorGUILayout.FloatField(_VignettingSizeContent, _Vignetting_Size.floatValue);
                _Vignetting_Contrast.floatValue = EditorGUILayout.FloatField(_VignettingContrastContent, _Vignetting_Contrast.floatValue);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //LUT Foldout
        CustomUI.GuiLineSeparator(2);
        _LUTFoldout = CustomUI.FoldOut("LUT Grading", _LUTFoldout);
        EditorGUILayout.Separator();
        if (_LUTFoldout)
        {
            _USE_LUTGrading = serializedObject.FindProperty("_USE_LUTGrading");
            _LUT_Tex = serializedObject.FindProperty("_LUT_Tex");
            _LUT_Power = serializedObject.FindProperty("_LUT_Power");

            _USE_LUTGrading.boolValue = EditorGUILayout.Toggle(_USE_LUTContent, _USE_LUTGrading.boolValue);
            if (_USE_LUTGrading.boolValue)
            {
                EditorGUILayout.ObjectField(_LUT_Tex, _LUTTexContent);
                EditorGUILayout.Slider(_LUT_Power, 0.0f, 1.0f, _LUTPowerContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //FishEye Foldout
        CustomUI.GuiLineSeparator(2);
        _FishEyeFoldout = CustomUI.FoldOut("FishEye Distorsion", _FishEyeFoldout);
        EditorGUILayout.Separator();
        if (_FishEyeFoldout)
        {
            _FishEye_Power = serializedObject.FindProperty("_FishEye_Power");

            _USE_FishEye = EditorGUILayout.Toggle(_USE_FishEyeContent, _USE_FishEye);
            if (_USE_FishEye)
            {
                _fishEyeType = (FishEyeType)EditorGUILayout.EnumPopup("FishEye Type:", _fishEyeType);
                EditorGUILayout.Slider(_FishEye_Power, -1.0f, 2.0f, _FishEyePowerContent);
            }
            ChangeFishEyeType(_fishEyeType);
            serializedObject.ApplyModifiedProperties();
        }

        //Bloom Foldout
        CustomUI.GuiLineSeparator(2);
        _BloomFoldout = CustomUI.FoldOut("Bloom", _BloomFoldout);
        EditorGUILayout.Separator();
        if (_BloomFoldout)
        {
            _USE_Bloom = serializedObject.FindProperty("_USE_Bloom");
            _BloomIntencity = serializedObject.FindProperty("_BloomIntencity");

            _USE_Bloom.boolValue = EditorGUILayout.Toggle(_USE_BloomContent, _USE_Bloom.boolValue);
            if(_USE_Bloom.boolValue)
            {
                EditorGUILayout.Slider(_BloomIntencity, 0.0f, 5.0f, _BloomIntencityContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Chromatic Aberration Foldout
        CustomUI.GuiLineSeparator(2);
        _ChromaticAberrationFoldout = CustomUI.FoldOut("Chromatic Aberration", _ChromaticAberrationFoldout);
        EditorGUILayout.Separator();
        if (_ChromaticAberrationFoldout)
        {
            _USE_ChromaticAberration = serializedObject.FindProperty("_USE_ChromaticAberration");
            _Chromatic_Aberration_Offset = serializedObject.FindProperty("_Chromatic_Aberration_Offset");
            _Chromatic_Aberration_Radius = serializedObject.FindProperty("_Chromatic_Aberration_Radius");

            _USE_ChromaticAberration.boolValue = EditorGUILayout.Toggle(_USE_ChromaticAberrationContent, _USE_ChromaticAberration.boolValue);
            if (_USE_ChromaticAberration.boolValue)
            {
                EditorGUILayout.Slider(_Chromatic_Aberration_Offset, 0.001f, 0.1f, _Chromatic_Aberration_OffsetContent);
                EditorGUILayout.Slider(_Chromatic_Aberration_Radius, 0.0f, 1.0f, _Chromatic_Aberration_RadiusContent);
            }
            serializedObject.ApplyModifiedProperties();
        }

        //Usage Tip
        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox("To use this Preset in your scene, please add 'MSRP PostProcess Controller' to any object on the scene and assign this preset into it", MessageType.None);


        //CustomUI.GuiLineSeparator(10);
        //base.OnInspectorGUI();
    }

    void ChangeFishEyeType(FishEyeType _fishEyeType)
    {
        _USE_FishEye_Fragment = serializedObject.FindProperty("_USE_FishEye_Fragment");
        _USE_FishEye_Vertex = serializedObject.FindProperty("_USE_FishEye_Vertex");
        switch (_fishEyeType)
        {
            case FishEyeType._useVertexFishEye:
                _USE_FishEye_Vertex.boolValue = true;
                _USE_FishEye_Fragment.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            case FishEyeType._useFragmentFishEye:
                _USE_FishEye_Fragment.boolValue = true;
                _USE_FishEye_Vertex.boolValue = false;
                serializedObject.ApplyModifiedProperties();
                break;
            default:
                break;
        }
        if (!_USE_FishEye)
        {
            _USE_FishEye_Vertex.boolValue = false;
            _USE_FishEye_Fragment.boolValue = false;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
