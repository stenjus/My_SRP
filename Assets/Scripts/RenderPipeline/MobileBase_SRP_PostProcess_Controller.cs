using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MobileBase_SRP_PostProcess_Controller : MonoBehaviour
{
    public MobileBase_SRP_PostProcess_Preset _PostProcessPreset;
    
    void Start()
    {
        var SRP_Asset = GraphicsSettings.renderPipelineAsset as MobileBase_SRP_Asset;
        SRP_Asset._MobileBase_SRP_PostProcess_Controller = this;
    }

}
