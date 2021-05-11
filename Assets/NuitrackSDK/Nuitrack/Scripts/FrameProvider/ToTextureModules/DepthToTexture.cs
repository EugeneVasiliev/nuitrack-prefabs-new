/*
 * This script converts source data of nuitrack.DepthFrame to RenderTexture using the GPU,
 * which is several times faster than CPU conversions.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

public class DepthToTexture : FrameToTexture
{
    [Range (0f, 32.0f)]
    [SerializeField] float maxDepthSensor = 10f;

    ComputeBuffer sourceDataBuffer;

    byte[] depthDataArray = null;

    public nuitrack.DepthFrame SourceFrame
    {
        get
        {
            return NuitrackManager.DepthFrame;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (sourceDataBuffer != null)
        {
            sourceDataBuffer.Release();
            sourceDataBuffer = null;
        }

        depthDataArray = null;
    }

    Texture2D GetCPUTexture(nuitrack.DepthFrame frame)
    {
        if (frame.Timestamp == lastTimeStamp && outRgbTexture != null)
            return outRgbTexture;
        else
        {
            outRgbTexture = frame.ToTexture2D(maxDepthSensor);
            lastTimeStamp = frame.Timestamp;

            return outRgbTexture;
        }
    }

    RenderTexture GetGPUTexture(nuitrack.DepthFrame frame)
    {
        if (frame.Timestamp == lastTimeStamp && outRgbTexture != null)
            return renderTexture;
        else
        {
            lastTimeStamp = frame.Timestamp;

            if (outRgbTexture == null)
            {
                outRgbTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                rect = new Rect(0, 0, frame.Cols, frame.Rows);
            }

            if (instanceShader == null)
            {
                InitShader("Depth2Texture");
                instanceShader.SetInt("textureWidth", frame.Cols);

                /*
                     We put the source data in the buffer, but the buffer does not support types 
                     that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

                     For optimization, we specify a length half the original length,
                     since the data is correctly projected into memory
                     (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
                */

                sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
                instanceShader.SetBuffer(kernelIndex, "DepthFrame", sourceDataBuffer);
            }

            if (renderTexture == null)
            {
                InitRenderTexture(frame.Cols, frame.Rows);
                depthDataArray = new byte[frame.DataSize];
            }

            Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
            sourceDataBuffer.SetData(depthDataArray);

            instanceShader.SetFloat("maxDepthSensor", maxDepthSensor);
            instanceShader.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);

            return renderTexture;
        }
    }

    public override RenderTexture GetRenderTexture()
    {
        if (SourceFrame == null)
            return null;

        if (SystemInfo.supportsComputeShaders)
            return GetGPUTexture(SourceFrame);
        else
        {
            outRgbTexture = GetCPUTexture(SourceFrame);
            CopyTexture2DToRenderTexture();

            return renderTexture;
        }
    }

    public override Texture2D GetTexture2D()
    {
        if (SourceFrame == null)
            return null;

        if (!SystemInfo.supportsComputeShaders)
            return GetCPUTexture(SourceFrame);
        else
        {
            renderTexture = GetGPUTexture(SourceFrame);
            CopyRenderTextureToTexture2D();
            return outRgbTexture;
        }
    }
}