using UnityEngine;

public class FrameUtils : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    ComputeShader instanceShader;

    public enum Operation
    {
        Cut = 0,
        ReverseCut,
        Mul,
        MixMask
    }

    ComputeShader ComputeShader
    {
        get
        {
            if (instanceShader == null)
                instanceShader = Instantiate(computeShader);

            return instanceShader;
        }
    }

    public void Join(Texture texture, Texture mask, ref RenderTexture renderTexture, Operation operation)
    {
        JoinTextures(texture, mask, operation.ToString(), ref renderTexture);
    }

    public void Join(FrameToTexture texture, FrameToTexture mask, ref RenderTexture renderTexture, Operation operation)
    {
        JoinTextures(texture.GetRenderTexture(), mask.GetRenderTexture(), operation.ToString(), ref renderTexture);
    }

    void JoinTextures(Texture texture, Texture mask, string kernelName, ref RenderTexture renderTexture)
    {
        if (texture == null && mask == null)
            return;

        int kernelIndex = ComputeShader.FindKernel(kernelName);
        uint x, y, z;
        instanceShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);

        instanceShader.SetTexture(kernelIndex, "Texture", texture);
        instanceShader.SetTexture(kernelIndex, "Mask", mask);

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        instanceShader.SetTexture(kernelIndex, "Output", renderTexture);

        instanceShader.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }
}
