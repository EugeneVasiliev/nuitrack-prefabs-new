﻿/*
 * This script converts the source data of nuitrack.UserFrame to textures (RenderTexture / Texture2D / Texture) 
 * using the fastest available method for this platform. 
 * If the platform supports ComputeShader, the conversion is performed using the GPU, which is several times faster than the CPU conversion.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;
using System.Runtime.InteropServices;

namespace FrameProviderModules
{
    public class SegmentToTexture : FrameToTexture<nuitrack.UserFrame, ushort>
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
        byte[] outSegment = null;

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
            outSegment = null;
        }

        Texture2D GetCPUTexture(nuitrack.UserFrame frame, TextureCache textureCache, Color[] userColors = null)
        {
            ref Texture2D destTexture = ref textureCache.texture2D;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.texture2D && localCache.texture2D != null)
                return localCache.texture2D;
            else
            {
                if (userColors == null)
                    userColors = defaultColors;

                if (outSegment == null)
                    outSegment = new byte[frame.Cols * frame.Rows * 4];

                Marshal.Copy(frame.Data, outSegment, 0, frame.DataSize);

                //The conversion can be performed without an additional array, 
                //since after copying, the bytes are clumped at the beginning of the array.
                //Let's start the crawl from the end of the array by "stretching" the source data.
                for (int i = frame.DataSize - 1, ptr = outSegment.Length - 1; i > 0; i -= 2, ptr -= 4)
                {
                    int userIndex = outSegment[i] << 8 | outSegment[i - 1];
                    Color currentColor = userColors[userIndex];

                    outSegment[ptr - 3] = (byte)(255f * currentColor.a);
                    outSegment[ptr - 2] = (byte)(255f * currentColor.r);
                    outSegment[ptr - 1] = (byte)(255f * currentColor.g);
                    outSegment[ptr] = (byte)(255f * currentColor.b);
                }

                if (destTexture == null)
                    destTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                destTexture.LoadRawTextureData(outSegment);
                destTexture.Apply();


                textureCache.timeStamp = frame.Timestamp;

                return destTexture;
            }
        }

        RenderTexture GetGPUTexture(nuitrack.UserFrame frame, TextureCache textureCache, Color[] userColors = null)
        {
            ref RenderTexture destTexture = ref textureCache.renderTexture;

            if (frame.Timestamp == textureCache.timeStamp && destTexture == localCache.renderTexture && localCache.renderTexture != null)
                return localCache.renderTexture;
            else
            {
                textureCache.timeStamp = frame.Timestamp;

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

                if (destTexture == null)
                    destTexture = InitRenderTexture(frame.Cols, frame.Rows);

                instanceShader.SetTexture(kernelIndex, "Result", destTexture);

                if (segmentDataArray == null)
                    segmentDataArray = new byte[frame.DataSize];

                Marshal.Copy(frame.Data, segmentDataArray, 0, frame.DataSize);
                sourceDataBuffer.SetData(segmentDataArray);

                instanceShader.Dispatch(kernelIndex, destTexture.width / (int)x, destTexture.height / (int)y, (int)z);

                return destTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(nuitrack.UserFrame frame, TextureCache textureCache = null)
        {
            return GetRenderTexture(frame, defaultColors, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public RenderTexture GetRenderTexture(nuitrack.UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
                return GetGPUTexture(frame, textureCache != null ? textureCache : localCache, userColors);
            else
            {
                TextureCache cache = textureCache != null ? textureCache : localCache;

                cache.texture2D = GetCPUTexture(frame, cache, userColors);
                FrameUtils.TextureUtils.Copy(cache.texture2D, ref cache.renderTexture);

                return cache.renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <returns>UserFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(nuitrack.UserFrame frame, TextureCache textureCache = null)
        {
            return GetTexture2D(frame, defaultColors, textureCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>UserFrame converted to Texture2D</returns>
        public Texture2D GetTexture2D(nuitrack.UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
            {
                TextureCache cache = textureCache != null ? textureCache : localCache;

                cache.renderTexture = GetGPUTexture(frame, cache, userColors);
                FrameUtils.TextureUtils.Copy(cache.renderTexture, ref cache.texture2D);
                return cache.texture2D;
            }
            else
                return GetCPUTexture(frame, textureCache != null ? textureCache : localCache);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T, TextureCache)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <param name="textureCache">(optional) If you want to get a separate copy of the texture, 
        /// and not a cached version, pass a reference to the local texture (may affect performance)</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public Texture GetTexture(nuitrack.UserFrame frame, Color[] userColors, TextureCache textureCache = null)
        {
            if (GPUSupported)
                return GetRenderTexture(frame, userColors, textureCache);
            else
                return GetTexture2D(frame, userColors, textureCache);
        }
    }
}