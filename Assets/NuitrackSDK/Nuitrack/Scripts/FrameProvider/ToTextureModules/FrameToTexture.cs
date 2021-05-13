﻿using UnityEngine;

namespace FrameProviderModules
{
    public abstract class FrameToTexture : MonoBehaviour
    {
#if UNITY_EDITOR
        bool DEBUG_USE_CPU = false;
#endif

        [SerializeField] ComputeShader computeShader;
        protected ComputeShader instanceShader;

        protected RenderTexture renderTexture;
        protected Texture2D texture2D;

        protected Rect rect;

        protected uint x, y, z;
        protected int kernelIndex;

        protected ulong lastTimeStamp = 0;

        protected bool GPUSupported
        {
            get
            {
#if UNITY_EDITOR
                return SystemInfo.supportsComputeShaders && !DEBUG_USE_CPU;
#else
                return SystemInfo.supportsComputeShaders;
#endif
            }
        }

        protected virtual void Awake()
        {
            if (!GPUSupported)
            {
#if UNITY_EDITOR && !UNITY_STANDALONE
            Debug.LogError("Compute shaders are not supported for the Android platform in the editor.\n" +
                "A software conversion will be used (may cause performance issues)\n" +
                "Switch the platform to Standalone (this is not relevant for the assembled project).");
#else
                Debug.LogError("Compute shaders are not supported. A software conversion will be used (may cause performance issues).");
#endif
            }
        }

        protected void InitShader(string kernelName)
        {
            if (instanceShader != null)
                Destroy(instanceShader);

            instanceShader = Instantiate(computeShader);
            kernelIndex = instanceShader.FindKernel(kernelName);
            instanceShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        }

        protected void InitRenderTexture(int width, int height)
        {
            renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            instanceShader.SetTexture(kernelIndex, "Result", renderTexture);
        }

        /// <summary>
        /// Get the frame as a RenderTexture. 
        /// Recommended method for platforms with ComputeShader support.
        /// </summary>
        /// <returns>Frame converted to RenderTexture</returns>
        public abstract RenderTexture GetRenderTexture();

        /// <summary>
        /// Get a frame in the form of Texture2D. 
        /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
        /// If possible, use GetRenderTexture.
        /// </summary>
        /// <returns>Frame converted to Texture2D</returns>
        public abstract Texture2D GetTexture2D();

        /// <summary>
        /// Convert Frame to Texture. 
        /// The method will select the most productive way to get the texture. 
        /// This can be either RenderTexture or Texture2D. 
        /// Use this method if you don't care about the texture type.
        /// </summary>
        /// <returns>Texture = (RenderTexture or Texture2D)</returns>
        public Texture GetTexture()
        {
            if (GPUSupported)
                return GetRenderTexture();
            else
                return GetTexture2D();
        }

        protected virtual void OnDestroy()
        {
            if (instanceShader != null)
                Destroy(instanceShader);

            if (renderTexture != null)
                Destroy(renderTexture);

            if (texture2D != null)
                Destroy(texture2D);
        }
    }
}