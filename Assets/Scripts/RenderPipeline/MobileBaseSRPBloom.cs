using UnityEngine;
using UnityEngine.Rendering;

public static class MobileBaseSRPBloom
{
    static readonly CommandBuffer bloomBuffer  = new CommandBuffer { name = "Bloom Buffer" };
    
    public static void BloomPost(ScriptableRenderContext context, int screenWidth, int screenHeight, MobileBaseSRPAsset pipeLineAsset)
    {
        Material dualFilterMat = pipeLineAsset.DualFiltering;
        int blurOffsetDown = Shader.PropertyToID("_BlurOffsetDown");
        int blurOffsetUp = Shader.PropertyToID("_BlurOffsetUp");
        int brightId = Shader.PropertyToID("_BrightID");
        int bloomResult = Shader.PropertyToID("_BloomResult");
        int passes = pipeLineAsset.BloomPasses;
        int[] downId = new int[passes];
        int[] upId = new int[passes];

        //Set Offset to the shader
        bloomBuffer.SetGlobalFloat(blurOffsetDown, pipeLineAsset.BlurOffsetDown);
        bloomBuffer.SetGlobalFloat(blurOffsetUp, pipeLineAsset.BlurOffsetUp);

        //IDs Loop
        for (int i = 0; i < passes; i++)
        {
            downId[i] = Shader.PropertyToID("_DownID" + i);
            upId[i] = Shader.PropertyToID("_UpID" + i);
        }

        //Bright Pass
        bloomBuffer.GetTemporaryRT(brightId, screenWidth, screenHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
        bloomBuffer.SetGlobalTexture("_BrightPassTex", MobileBaseSRPCommonValues.RenderTexture.FrameBufferId);
        bloomBuffer.Blit(null, brightId, dualFilterMat, 2);

        //DownScale Pass
        for (int i = 0; i < passes; i++)
        {
            bloomBuffer.GetTemporaryRT(downId[i], screenWidth >> i, screenHeight >> i, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            if (i == 0)
            {
                bloomBuffer.SetGlobalTexture("_DownScalePassTex", brightId);
                bloomBuffer.Blit(null, downId[i], dualFilterMat, 0);
            }
            else
            {
                bloomBuffer.SetGlobalTexture("_DownScalePassTex", downId[i - 1]);
                bloomBuffer.Blit(null, downId[i], dualFilterMat, 0);
            }
        }
        
        //UpScale Pass
        for (int i = passes - 2; i > 0; i--)
        {
            if (i == passes -  2)
            {
                //WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING
                //WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING
                //WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING GOVNOCOD WARNING
            }
            else
            {
                bloomBuffer.GetTemporaryRT(upId[i], screenWidth >> i + 1, screenHeight >> i + 1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
                if (i == passes - 3)
                {
                    bloomBuffer.SetGlobalTexture("_UpscalePassTex", upId[i]);
                    bloomBuffer.SetGlobalTexture("_DownPass", downId[i + 1]);
                    bloomBuffer.Blit(null, upId[i], dualFilterMat, 1);
                }
                else
                {
                    bloomBuffer.SetGlobalTexture("_UpscalePassTex", upId[i + 1]);
                    bloomBuffer.SetGlobalTexture("_DownPass", downId[i]);
                    bloomBuffer.Blit(null, upId[i], dualFilterMat, 1);
                }
            }

        }
        
        bloomBuffer.GetTemporaryRT(bloomResult, screenWidth * 2, screenHeight * 2, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
        bloomBuffer.Blit(upId[1], bloomResult, dualFilterMat, 1);
        bloomBuffer.SetGlobalTexture("_BloomResult", bloomResult);

        //CleanUp Chain
        for (int i = 0; i < passes; i++)
        {
            bloomBuffer.ReleaseTemporaryRT(downId[i]);
        }
        for (int i = passes - 2; i > 0; i--)
        {
            bloomBuffer.ReleaseTemporaryRT(upId[i]);
        }

        context.ExecuteCommandBuffer(bloomBuffer);
        bloomBuffer.Clear();
    }
}
