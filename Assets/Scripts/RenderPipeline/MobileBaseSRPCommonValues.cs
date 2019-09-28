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

        //Bloom Chain Variables
        public static int BloomPassFrameBuffer = Shader.PropertyToID("_BloomPassFrameBuffer1");
        public static RenderTargetIdentifier BloomPassFrameBufferId;
    }
}
