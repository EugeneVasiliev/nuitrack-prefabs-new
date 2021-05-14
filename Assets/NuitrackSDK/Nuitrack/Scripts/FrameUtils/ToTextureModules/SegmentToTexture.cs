/*
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

        Texture2D GetCPUTexture(nuitrack.UserFrame frame, Color[] userColors = null)
        {
            if (frame.Timestamp == lastTimeStamp && texture2D != null)
                return texture2D;
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

                if (texture2D == null)
                    texture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.ARGB32, false);

                texture2D.LoadRawTextureData(outSegment);
                texture2D.Apply();
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
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T)"/> 
        /// </summary>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture(nuitrack.UserFrame frame)
        {
            return GetRenderTexture(frame, defaultColors);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetRenderTexture(T)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <returns>UserFrame converted to RenderTexture</returns>
        public RenderTexture GetRenderTexture(nuitrack.UserFrame frame, Color[] userColors)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
                return GetGPUTexture(frame, userColors);
            else
            {
                texture2D = GetCPUTexture(frame, userColors);
                FrameUtils.TextureUtils.Copy(texture2D, ref renderTexture);

                return renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T)"/> 
        /// </summary>
        /// <returns>UserFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D(nuitrack.UserFrame frame)
        {
            return GetTexture2D(frame, defaultColors);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture2D(T)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <returns>UserFrame converted to Texture2D</returns>
        public Texture2D GetTexture2D(nuitrack.UserFrame frame, Color[] userColors)
        {
            if (frame == null)
                return null;

            if (GPUSupported)
            {
                renderTexture = GetGPUTexture(frame, userColors);
                FrameUtils.TextureUtils.Copy(renderTexture, ref texture2D);
                return texture2D;
            }     
            else
                return GetCPUTexture(frame, userColors);
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T)"/> 
        /// </summary>
        /// <param name="userColors">Colors for user segments.</param>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public Texture GetTexture(nuitrack.UserFrame frame, Color[] userColors)
        {
            if (GPUSupported)
                return GetRenderTexture(frame, userColors);
            else
                return GetTexture2D(frame, userColors);
        }
    }
}