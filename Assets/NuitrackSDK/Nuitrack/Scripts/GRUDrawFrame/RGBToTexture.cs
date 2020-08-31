using UnityEngine;
using UnityEngine.UI;

public class RGBToTexture : MonoBehaviour
{
    [SerializeField] RawImage rawImage;
    [SerializeField] ComputeShader BGR2RGBShader;

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
        ToRenderTexture(frame);
        rawImage.texture = renderTexture;
    }

    void ToRenderTexture(nuitrack.ColorFrame frame)
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
        int kernelIndex = BGR2RGBShader.FindKernel("RGB2BGR");

        BGR2RGBShader.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);
        BGR2RGBShader.SetTexture(kernelIndex, "Result", renderTexture);

        BGR2RGBShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        BGR2RGBShader.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);
    }

}
