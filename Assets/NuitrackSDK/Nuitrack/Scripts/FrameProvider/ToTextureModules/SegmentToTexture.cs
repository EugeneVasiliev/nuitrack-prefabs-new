﻿/*
 * This script converts source data of nuitrack.UserFrame to RenderTexture using the GPU,
 * which is several times faster than CPU conversions.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

public class SegmentToTexture : FrameToTexture
{
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

    ComputeBuffer userColorsBuffer;
    ComputeBuffer sourceDataBuffer;

    byte[] segmentDataArray = null;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (userColorsBuffer != null)
        {
            userColorsBuffer.Release();
            userColorsBuffer = null;
        }

        if (sourceDataBuffer != null)
        {
            sourceDataBuffer.Release();
            userColorsBuffer = null;
        }

        segmentDataArray = null;
    }

    public nuitrack.UserFrame SourceFrame
    {
        get
        {
            return NuitrackManager.UserFrame;
        }
    }

    Texture2D GetCPUTexture(nuitrack.UserFrame frame, Color[] userColors = null)
    {
        if (frame.Timestamp == lastTimeStamp && texture2D != null)
            return texture2D;
        else
        {
            if (userColors == null)
                userColors = defaultColors;

            texture2D = frame.ToTexture2D(userColors);
            lastTimeStamp = frame.Timestamp;

            return texture2D;
        }
    }

    RenderTexture GetGPUTexture(nuitrack.UserFrame frame, Color[] userColors = null)
    {
        if (frame.Timestamp == lastTimeStamp && renderTexture != null)
            return renderTexture;
        else
        {
            lastTimeStamp = frame.Timestamp;

            if (instanceShader == null)
            {
                InitShader("Segment2Texture");
                instanceShader.SetInt("textureWidth", frame.Cols);

                /*
                   We put the source data in the buffer, but the buffer does not support types 
                   that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

                   For optimization, we specify a length half the original length,
                   since the data is correctly projected into memory
                   (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
                */

                sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));
                instanceShader.SetBuffer(kernelIndex, "UserIndexes", sourceDataBuffer);
            }

            if (userColors == null)
                userColors = defaultColors;

            if (userColorsBuffer == null || userColorsBuffer.count != userColors.Length)
            {
                userColorsBuffer = new ComputeBuffer(userColors.Length, sizeof(float) * 4);
                instanceShader.SetBuffer(kernelIndex, "UserColors", userColorsBuffer);
            }

            userColorsBuffer.SetData(userColors);

            if (renderTexture == null)
            {
                InitRenderTexture(frame.Cols, frame.Rows);          
                segmentDataArray = new byte[frame.DataSize];
            }

            Marshal.Copy(frame.Data, segmentDataArray, 0, frame.DataSize);
            sourceDataBuffer.SetData(segmentDataArray);

            instanceShader.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);

            return renderTexture;
        }
    }

    /// <summary>
    /// Get the UserFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>UserFrame converted to RenderTexture</returns>
    public override RenderTexture GetRenderTexture()
    {
        return GetRenderTexture(defaultColors);
    }

    /// <summary>
    /// Get the UserFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <param name="userColors">Colors for user segments.</param>
    /// <returns>UserFrame converted to RenderTexture</returns>
    public RenderTexture GetRenderTexture(Color[] userColors)
    {
        if (SourceFrame == null)
            return null;

        if (SystemInfo.supportsComputeShaders)
            return GetGPUTexture(SourceFrame, userColors);
        else
        {
            texture2D = GetCPUTexture(SourceFrame, userColors);
            CopyTexture(texture2D, ref renderTexture);

            return renderTexture;
        }
    }

    /// <summary>
    /// Get a UserFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>UserFrame converted to Texture2D</returns>
    public override Texture2D GetTexture2D()
    {
        return GetTexture2D(defaultColors);
    }

    /// <summary>
    /// Get a UserFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <param name="userColors">Colors for user segments.</param>
    /// <returns>UserFrame converted to Texture2D</returns>
    public Texture2D GetTexture2D(Color[] userColors)
    {
        if (SourceFrame == null)
            return null;

        if (!SystemInfo.supportsComputeShaders)
            return GetCPUTexture(SourceFrame, userColors);
        else
        {
            renderTexture = GetGPUTexture(SourceFrame, userColors);
            CopyTexture(renderTexture, ref texture2D);
            return texture2D;
        }
    }

    /// <summary>
    /// Convert UserFrame to Texture. 
    /// The method will select the most productive way to get the texture. 
    /// This can be either RenderTexture or Texture2D. 
    /// Use this method if you don't care about the texture type.
    /// </summary>
    /// <param name="userColors">Colors for user segments.</param>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public Texture GetTexture(Color[] userColors)
    {
        if (SystemInfo.supportsComputeShaders)
            return GetRenderTexture(userColors);
        else
            return GetTexture2D(userColors);
    }
}