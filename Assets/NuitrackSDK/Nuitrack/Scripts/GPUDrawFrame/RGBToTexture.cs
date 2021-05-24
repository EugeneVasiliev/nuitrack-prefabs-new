/*
 * This script converts source data of nuitrack.ColorFrame to RenderTexture using the GPU,
 * which is several times faster than CPU conversions.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using UnityEngine.UI;

public class RGBToTexture : MonoBehaviour
{
    [SerializeField] RawImage rawImage;
    [SerializeField] ComputeShader BGR2RGBShader;

    ComputeShader instance;

    Texture2D dstRgbTexture2D;
    RenderTexture renderTexture;

    uint x, y, z;
    int kernelIndex;

    void OnEnable()
    {
        NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;

        if (SystemInfo.supportsComputeShaders)
        {
            instance = Instantiate(BGR2RGBShader);
            kernelIndex = instance.FindKernel("RGB2BGR");
            instance.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        }
        else
            Debug.LogWarning("Compute Shader is not support. Performance may be affected. Check requirements https://docs.unity3d.com/Manual/class-ComputeShader.html");
    }

    void OnDisable()
    {
        NuitrackManager.onColorUpdate -= NuitrackManager_onColorUpdate;
        Destroy(renderTexture);
        Destroy(instance);
    }

    void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            rawImage.texture = frame.ToTexture2D();
            return;
        }

        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            instance.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);

            renderTexture = new RenderTexture(dstRgbTexture2D.width, dstRgbTexture2D.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            instance.SetTexture(kernelIndex, "Result", renderTexture);

            rawImage.texture = renderTexture;
        }

        dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        dstRgbTexture2D.Apply();
        instance.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);
    }
}
