/*
 * This script converts source data of nuitrack.DepthFrame to RenderTexture using the GPU,
 * which is several times faster than CPU conversions.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using UnityEngine.UI;

using System.Runtime.InteropServices;

public class DepthToTexture : MonoBehaviour
{
    [SerializeField] RawImage background;
    [SerializeField] ComputeShader depthToTexture;

    [Range (-1.0f, 1.0f)]
    [SerializeField] float contrast = 0f;

    ComputeShader instance;

    ComputeBuffer sourceDataBuffer;

    byte[] depthDataArray = null;
    RenderTexture renderTexture;

    uint x, y, z;
    int kernelIndex;

    void OnEnable()
    {
        NuitrackManager.onDepthUpdate += DrawDepth;

        if (SystemInfo.supportsComputeShaders)
        {
            instance = Instantiate(depthToTexture);

            kernelIndex = instance.FindKernel("Depth2Texture");
            instance.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        }
        else
            Debug.LogError("Compute Shader is not support. Performance may be affected. Check requirements https://docs.unity3d.com/Manual/class-ComputeShader.html");
    }

    void OnDisable()
    {
        NuitrackManager.onDepthUpdate -= DrawDepth;

        if (SystemInfo.supportsComputeShaders)
        {
            NuitrackManager.onDepthUpdate -= DrawDepth;
            sourceDataBuffer.Release();
        }

        Destroy(renderTexture);
        Destroy(instance);
    }

    void DrawDepth(nuitrack.DepthFrame frame)
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            background.texture = frame.ToTexture2D();
            return;
        }

        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            renderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            background.texture = renderTexture;

            instance.SetInt("textureWidth", renderTexture.width);
            instance.SetTexture(kernelIndex, "Result", renderTexture);

            /*
            We put the source data in the buffer, but the buffer does not support types 
            that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

            For optimization, we specify a length half the original length,
            since the data is correctly projected into memory
            (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
            */

            int dataSize = frame.DataSize;
            sourceDataBuffer = new ComputeBuffer(dataSize / 2, sizeof(uint));
            instance.SetBuffer(kernelIndex, "DepthFrame", sourceDataBuffer);

            depthDataArray = new byte[dataSize];
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        sourceDataBuffer.SetData(depthDataArray);

        instance.SetFloat("contrast", contrast);
        instance.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }
}