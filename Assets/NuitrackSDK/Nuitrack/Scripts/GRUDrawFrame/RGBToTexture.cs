using UnityEngine;
using UnityEngine.UI;

public class RGBToTexture : MonoBehaviour
{
    [SerializeField] RawImage rawImage;
    [SerializeField] ComputeShader RGB2BGRShader;

    Texture2D dstRgbTexture2D;
    RenderTexture renderTexture;

    void Start()
    {
        if (SystemInfo.supportsComputeShaders)
            NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
        else
            Debug.LogError("Comppute Shader is not support.");
    }

    void OnDestroy()
    {
        if (SystemInfo.supportsComputeShaders)
            NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
    }

    void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);

            renderTexture = new RenderTexture(dstRgbTexture2D.width, dstRgbTexture2D.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        dstRgbTexture2D.Apply();

        uint x, y, z;
        int kernelIndex = RGB2BGRShader.FindKernel("RGB2BGR");

        RGB2BGRShader.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);
        RGB2BGRShader.SetTexture(kernelIndex, "Result", renderTexture);

        RGB2BGRShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        RGB2BGRShader.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);
    }


}
