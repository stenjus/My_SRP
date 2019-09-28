using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MobileBaseSRPPostProcessController : MonoBehaviour
{
    public MobileBaseSRPPostProcessPreset PostProcessPreset;
    
    void Awake()
    {
        var srpAsset = GraphicsSettings.renderPipelineAsset as MobileBaseSRPAsset;
        srpAsset.MobileBaseSrpPostProcessController = this;
    }

}
