using UnityEngine;
using UnityEngine.UI;

using System.Runtime.InteropServices;

public class SegmentToTexture : MonoBehaviour
{
    [SerializeField] RawImage rawImage;
    [SerializeField] ComputeShader segment2Texture;

    [SerializeField]
    Color[] defaultColors = new Color[]
    {
        Color.clear,
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow,
        Color.cyan,
        Color.grey
    };

    RenderTexture renderTexture;

    ComputeBuffer userColorsBuffer;
    ComputeBuffer sourceDataBuffer;

    byte[] segmentDataArray = null;

    void Start()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            NuitrackManager.onUserTrackerUpdate += NuitrackManager_onUserTrackerUpdate;

            userColorsBuffer = new ComputeBuffer(defaultColors.Length, sizeof(float) * 4);
            userColorsBuffer.SetData(defaultColors);
        }
        else
            Debug.LogError("Comppute Shader is not support.");
    }

    void NuitrackManager_onUserTrackerUpdate(nuitrack.UserFrame frame)
    {
        ToRenderTexture(frame);
        rawImage.texture = renderTexture;
    }

    void OnDestroy()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            NuitrackManager.onUserTrackerUpdate -= NuitrackManager_onUserTrackerUpdate;

            userColorsBuffer.Release();
            sourceDataBuffer.Release();
        }
    }

    void ToRenderTexture(nuitrack.UserFrame frame)
    {
        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            renderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            segment2Texture.SetInt("textureWidth", renderTexture.width);

            sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
            segmentDataArray = new byte[frame.DataSize];
        }

        Marshal.Copy(frame.Data, segmentDataArray, 0, frame.DataSize);
        sourceDataBuffer.SetData(segmentDataArray);

        uint x, y, z;
        int kernelIndex = segment2Texture.FindKernel("Segment2Texture");

        segment2Texture.SetTexture(kernelIndex, "Result", renderTexture);

        segment2Texture.SetBuffer(kernelIndex, "UserIndexes", sourceDataBuffer);
        segment2Texture.SetBuffer(kernelIndex, "UserColors", userColorsBuffer);

        segment2Texture.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        segment2Texture.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }
}