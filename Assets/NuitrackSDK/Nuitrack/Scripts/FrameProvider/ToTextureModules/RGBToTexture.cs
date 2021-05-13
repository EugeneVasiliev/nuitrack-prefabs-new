﻿/*
 * This script converts the source data of nuitrack.ColorFrame to textures (RenderTexture / Texture2D / Texture) 
 * using the fastest available method for this platform. 
 * If the platform supports ComputeShader, the conversion is performed using the GPU, which is several times faster than the CPU conversion.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

namespace FrameProviderModules
{
    public class RGBToTexture : FrameToTexture
    {
        Texture2D dstRgbTexture2D;
        byte[] colorDataArray = null;

        public nuitrack.ColorFrame SourceFrame
        {
            get
            {
                return NuitrackManager.ColorFrame;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(dstRgbTexture2D);

            colorDataArray = null;
        }

        Texture2D GetCPUTexture(nuitrack.ColorFrame frame)
        {
            if (frame.Timestamp == lastTimeStamp && texture2D != null)
                return texture2D;
            else
            {
                int datasize = frame.DataSize;

                if (colorDataArray == null)
                    colorDataArray = new byte[frame.Cols * frame.Rows * 4];

                Marshal.Copy(frame.Data, colorDataArray, 0, datasize);

                for (int i = datasize - 1, ptr = colorDataArray.Length - 1; i > 0; i -= 3, ptr -= 4)
                {
                    byte r = colorDataArray[i - 2];
                    byte g = colorDataArray[i - 1];
                    byte b = colorDataArray[i];

                    colorDataArray[ptr - 3] = 255;
                    colorDataArray[ptr - 2] = b;
                    colorDataArray[ptr - 1] = g;
                    colorDataArray[ptr] = r;
                }

                if (texture2D == null)
                    texture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                texture2D.LoadRawTextureData(colorDataArray);
                texture2D.Apply();

                lastTimeStamp = frame.Timestamp;

                return texture2D;
            }
        }

        RenderTexture GetGPUTexture(nuitrack.ColorFrame frame)
        {
            if (frame.Timestamp == lastTimeStamp && renderTexture != null)
                return renderTexture;
            else
            {
                lastTimeStamp = frame.Timestamp;

                if (instanceShader == null)
                    InitShader("RGB2BGR");

                if (renderTexture == null)
                {
                    dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                    instanceShader.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);

                    InitRenderTexture(frame.Cols, frame.Rows);
                }

                dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
                dstRgbTexture2D.Apply();

                instanceShader.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);

                return renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture.GetRenderTexture"/> 
        /// </summary>
        /// <returns>ColorFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture()
        {
            if (SourceFrame == null)
                return null;

            if (GPUSupported)
                return GetGPUTexture(SourceFrame);
            else
            {
                texture2D = GetCPUTexture(SourceFrame);
                FrameProvider.FrameUtils.Copy(texture2D, ref renderTexture);

                return renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture.GetTexture2D"/> 
        /// </summary>
        /// <returns>ColorFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D()
        {
            if (SourceFrame == null)
                return null;

            if (GPUSupported)
            {
                renderTexture = GetGPUTexture(SourceFrame);
                FrameProvider.FrameUtils.Copy(renderTexture, ref texture2D);
                return texture2D;
            }       
            else
                return GetCPUTexture(SourceFrame);
        }
    }
}