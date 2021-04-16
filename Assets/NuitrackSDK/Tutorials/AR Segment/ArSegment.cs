using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

public class ArSegment : MonoBehaviour
{
    [SerializeField] DataProvider dataProvider;
    [SerializeField] Camera mainCamera;

    [Header ("RGB shader")]
    [SerializeField] ComputeShader BGR2RGBShader;

    Texture2D dstRgbTexture2D;
    RenderTexture rgbRenderTexture;

    uint xRGB, yRGB, zRGB;
    int rgbKernelIndex;

    [Header("Segment shader")]
    [SerializeField] ComputeShader depthToTexture;

    [Range(-1.0f, 1.0f)]
    [SerializeField] float contrast = 0f;

    ComputeBuffer sourceDataBuffer;

    byte[] depthDataArray = null;
    RenderTexture depthRenderTexture;

    uint xDepth, yDepth, zDepth;
    int depthKernelIndex;

    [Header ("Out")]
    [SerializeField] Material outMat;

    [Header ("Mesh generator")]
    [SerializeField] MeshGenerator meshGenerator;
    bool isGenerated = false;

    void Start()
    {
        // RGB
        rgbKernelIndex = BGR2RGBShader.FindKernel("RGB2BGR");
        BGR2RGBShader.GetKernelThreadGroupSizes(rgbKernelIndex, out xRGB, out yRGB, out zRGB);

        // Segment map

        depthKernelIndex = depthToTexture.FindKernel("Depth2Texture");
        depthToTexture.GetKernelThreadGroupSizes(depthKernelIndex, out xDepth, out yDepth, out zDepth);
    }

    void Update()
    {
        Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(mainCamera.transform.position);
        outMat.SetVector("_CameraPosition", localCameraPosition);

        UpdateRGB();
        UpdateHieghtMap();
    }

    void UpdateRGB()
    {
        if (rgbRenderTexture == null || rgbRenderTexture.width != dataProvider.rgbCols || rgbRenderTexture.height != dataProvider.rgbRows)
        {
            dstRgbTexture2D = new Texture2D(dataProvider.rgbCols, dataProvider.rgbRows, TextureFormat.RGB24, false);
            BGR2RGBShader.SetTexture(rgbKernelIndex, "Texture", dstRgbTexture2D);

            rgbRenderTexture = new RenderTexture(dstRgbTexture2D.width, dstRgbTexture2D.height, 0, RenderTextureFormat.ARGB32);
            rgbRenderTexture.enableRandomWrite = true;
            rgbRenderTexture.Create();

            BGR2RGBShader.SetTexture(rgbKernelIndex, "Result", rgbRenderTexture);
        }

        if (!isGenerated)
        {
            isGenerated = true;
            meshGenerator.Generate(dataProvider.rgbCols, dataProvider.rgbRows);
        }

        dstRgbTexture2D.LoadRawTextureData(dataProvider.RGB);
        dstRgbTexture2D.Apply();

        BGR2RGBShader.Dispatch(rgbKernelIndex, dstRgbTexture2D.width / (int)xRGB, dstRgbTexture2D.height / (int)yRGB, (int)zRGB);

        outMat.SetTexture("_MainTex", rgbRenderTexture);
    }

    void UpdateHieghtMap()
    {
        if (depthRenderTexture == null || depthRenderTexture.width != dataProvider.depthCols || depthRenderTexture.height != dataProvider.depthRows)
        {
            depthRenderTexture = new RenderTexture(dataProvider.depthCols, dataProvider.depthRows, 0, RenderTextureFormat.ARGB32);
            depthRenderTexture.enableRandomWrite = true;
            depthRenderTexture.Create();

            depthToTexture.SetInt("textureWidth", depthRenderTexture.width);
            depthToTexture.SetTexture(depthKernelIndex, "Result", depthRenderTexture);

            /*
            We put the source data in the buffer, but the buffer does not support types 
            that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32)).

            For optimization, we specify a length half the original length,
            since the data is correctly projected into memory
            (sizeof(ushot) * sourceDataBuffer / 2 == sizeof(uint) * sourceDataBuffer / 2)
            */

            int dataSize = dataProvider.Depth.Length;
            sourceDataBuffer = new ComputeBuffer(dataSize / 2, sizeof(uint));
            depthToTexture.SetBuffer(depthKernelIndex, "DepthFrame", sourceDataBuffer);

            depthDataArray = new byte[dataSize];
        }

        //Marshal.Copy(dataProvider.Depth, depthDataArray, 0, depthDataArray.Length);

        depthDataArray = dataProvider.Depth;
        sourceDataBuffer.SetData(depthDataArray);

        depthToTexture.SetFloat("contrast", contrast);
        depthToTexture.Dispatch(depthKernelIndex, depthRenderTexture.width / (int)xDepth, depthRenderTexture.height / (int)yDepth, (int)zDepth);

        outMat.SetTexture("_HeightMap", depthRenderTexture);
    }

    private void OnDestroy()
    {
        Destroy(depthRenderTexture);
        Destroy(rgbRenderTexture);
    }
}
