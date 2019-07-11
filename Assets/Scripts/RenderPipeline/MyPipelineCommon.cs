﻿using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class MyPipelineCommon
{
    public class RenderTexture
    {
        public static int FrameBuffer = Shader.PropertyToID("_FrameBuffer");
        public static RenderTargetIdentifier FrameBufferID;
        public static RenderTextureDescriptor FrameBufferDescriptor;
    }
}
