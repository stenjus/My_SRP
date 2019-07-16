using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class MobileBase_SRP_CommonValues
{
    public class RenderTexture
    {
        public static int FrameBuffer = Shader.PropertyToID("_FrameBuffer");
        public static RenderTargetIdentifier FrameBufferID;
        public static RenderTextureDescriptor FrameBufferDescriptor;

        //Bloom Chain Variables
        public static int _BloomPassFrameBuffer = Shader.PropertyToID("_BloomPassFrameBuffer1");
        public static RenderTargetIdentifier _BloomPassFrameBufferID;
    }
}
