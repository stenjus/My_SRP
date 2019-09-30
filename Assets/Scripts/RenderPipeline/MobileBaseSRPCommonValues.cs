using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class MobileBaseSRPCommonValues
{
    public class RenderTexture
    {
        public static int FrameBuffer = Shader.PropertyToID("_FrameBuffer");
        public static RenderTargetIdentifier FrameBufferId;
        public static RenderTextureDescriptor FrameBufferDescriptor;
    }
}
