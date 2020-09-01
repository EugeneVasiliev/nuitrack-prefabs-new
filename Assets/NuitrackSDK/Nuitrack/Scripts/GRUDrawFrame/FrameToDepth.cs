using UnityEngine;
using UnityEngine.UI;

using System.Runtime.InteropServices;

public class FrameToDepth : MonoBehaviour
{
    [SerializeField] RawImage background;
    [SerializeField] ComputeShader depthToTexture;

    [Range (-1.0f, 1.0f)]
    [SerializeField] float contrast = 0f;

    ComputeBuffer sourceDataBuffer;

    byte[] depthDataArray = null;
    RenderTexture renderTexture;

    uint x, y, z;
    int kernelIndex;

    void Start()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            kernelIndex = depthToTexture.FindKernel("Depth2Texture");
            depthToTexture.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);

            NuitrackManager.onDepthUpdate += DrawDepth;
        }
        else
            Debug.LogError("Comppute Shader is not support.");
    }

    void OnDestroy()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            NuitrackManager.onDepthUpdate -= DrawDepth;
            sourceDataBuffer.Release();
        }
    }

    void DrawDepth(nuitrack.DepthFrame frame)
    {
        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            renderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            background.texture = renderTexture;

            depthToTexture.SetInt("textureWidth", renderTexture.width);
            depthToTexture.SetTexture(kernelIndex, "Result", renderTexture);

            /*
            We put the source data in the buffer, but the buffer does not support types 
            that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

            For optimization, we specify a length half the original length,
            since the data is correctly projected into memory
            (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
            */

            int dataSize = frame.DataSize;
            sourceDataBuffer = new ComputeBuffer(dataSize / 2, sizeof(uint));
            depthToTexture.SetBuffer(kernelIndex, "DepthFrame", sourceDataBuffer);

            depthDataArray = new byte[dataSize];
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        sourceDataBuffer.SetData(depthDataArray);

        depthToTexture.SetFloat("contrast", contrast);
        depthToTexture.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }
}