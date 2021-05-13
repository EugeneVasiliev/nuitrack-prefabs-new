/*
 * This script converts the source data of nuitrack.DepthFrame to textures (RenderTexture / Texture2D / Texture) 
 * using the fastest available method for this platform. 
 * If the platform supports ComputeShader, the conversion is performed using the GPU, which is several times faster than the CPU conversion.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/



using UnityEngine;
using System.Runtime.InteropServices;

namespace FrameProviderModules
{
    public class DepthToTexture : FrameToTexture
    {
        [Range(0f, 32.0f)]
        [SerializeField] float maxDepthSensor = 10f;

        ComputeBuffer sourceDataBuffer;

        byte[] depthDataArray = null;
        byte[] outDepth = null;

        public nuitrack.DepthFrame SourceFrame
        {
            get
            {
                return NuitrackManager.DepthFrame;
            }
        }

        public float MaxSensorDepth
        {
            get
            {
                return maxDepthSensor;
            }
        }

        /// <summary>
        /// Get the hFOV of the DepthFrame in degrees
        /// </summary>
        public float hFOV
        {
            get
            {
                nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
                return mode.HFOV * Mathf.Rad2Deg;
            }
        }

        /// <summary>
        /// Get the vFOV of the DepthFrame in degrees
        /// </summary>
        public float vFOV
        {
            get
            {
                nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                float vFOV = mode.HFOV * ((float)mode.YRes / mode.XRes);
                return vFOV * Mathf.Rad2Deg;

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
            outDepth = null;
        }

        Texture2D GetCPUTexture(nuitrack.DepthFrame frame)
        {
            if (frame.Timestamp == lastTimeStamp && texture2D != null)
                return texture2D;
            else
            {
                if (outDepth == null || outDepth.Length != (frame.DataSize / 2) * 3)
                    outDepth = new byte[(frame.DataSize / 2) * 3];

                for (int i = 0; i < frame.DataSize / 2; i++)
                {
                    float depthVal = frame[i] / (1000 * maxDepthSensor);
                    byte depth = (byte)(256 * depthVal);

                    Color32 currentColor = new Color32(depth, depth, depth, 255);

                    int ptr = i * 3;

                    outDepth[ptr] = currentColor.r;
                    outDepth[ptr + 1] = currentColor.g;
                    outDepth[ptr + 2] = currentColor.b;
                }

                if (texture2D == null || texture2D.width != frame.Cols || texture2D.height != frame.Rows)
                    texture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);

                texture2D.LoadRawTextureData(outDepth);
                texture2D.Apply();

                lastTimeStamp = frame.Timestamp;

                return texture2D;
            }
        }

        RenderTexture GetGPUTexture(nuitrack.DepthFrame frame)
        {
            if (frame.Timestamp == lastTimeStamp && renderTexture != null)
                return renderTexture;
            else
            {
                lastTimeStamp = frame.Timestamp;

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

        /// <summary>
        /// See the method description: <see cref="FrameToTexture.GetRenderTexture"/> 
        /// </summary>
        /// <returns>DepthFrame converted to RenderTexture</returns>
        public override RenderTexture GetRenderTexture()
        {
            if (SourceFrame == null)
                return null;

            if (SystemInfo.supportsComputeShaders)
                return GetGPUTexture(SourceFrame);
            else
            {
                texture2D = GetCPUTexture(SourceFrame);
                CopyTexture(texture2D, ref renderTexture);

                return renderTexture;
            }
        }

        /// <summary>
        /// See the method description: <see cref="FrameToTexture.GetTexture2D"/> 
        /// </summary>
        /// <returns>DepthFrame converted to Texture2D</returns>
        public override Texture2D GetTexture2D()
        {
            if (SourceFrame == null)
                return null;

            if (!SystemInfo.supportsComputeShaders)
                return GetCPUTexture(SourceFrame);
            else
            {
                renderTexture = GetGPUTexture(SourceFrame);
                CopyTexture(renderTexture, ref texture2D);
                return texture2D;
            }
        }
    }
}