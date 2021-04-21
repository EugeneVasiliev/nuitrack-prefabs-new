using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

public class ArSegment : MonoBehaviour
{
    [SerializeField] DataProvider dataProvider;
    [SerializeField] Camera mainCamera;

    [Header ("RGB shader")]
    Texture2D dstRgbTexture2D;

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

    void Start()
    {
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
        //nuitrack.ColorFrame frame = NuitrackManager.ColorFrame;

        //if (frame == null)
        //    return;

        DataProviderFrame frame = dataProvider.RGBFrame;

        if (dstRgbTexture2D == null)
        {     
            dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            meshGenerator.Generate(frame.Cols, frame.Rows);
        }

        dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        dstRgbTexture2D.Apply();

        outMat.SetTexture("_MainTex", dstRgbTexture2D);
    }

    void UpdateHieghtMap()
    {
        //nuitrack.DepthFrame frame = NuitrackManager.DepthFrame;

        //if (frame == null)
        //    return;

        DataProviderFrame frame = dataProvider.DepthFrame;

        if (depthRenderTexture == null || depthRenderTexture.width != frame.Cols || depthRenderTexture.height != frame.Rows)
        {
            depthRenderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            depthRenderTexture.enableRandomWrite = true;
            depthRenderTexture.filterMode = FilterMode.Point;
            
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

            int dataSize = frame.DataSize;
            sourceDataBuffer = new ComputeBuffer(dataSize / 2, sizeof(uint));
            depthToTexture.SetBuffer(depthKernelIndex, "DepthFrame", sourceDataBuffer);

            depthDataArray = new byte[dataSize];
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        sourceDataBuffer.SetData(depthDataArray);

        depthToTexture.SetFloat("contrast", contrast);
        depthToTexture.Dispatch(depthKernelIndex, depthRenderTexture.width / (int)xDepth, depthRenderTexture.height / (int)yDepth, (int)zDepth);

        outMat.SetTexture("_HeightMap", depthRenderTexture);
    }

    private void OnDestroy()
    {
        Destroy(depthRenderTexture);
        Destroy(dstRgbTexture2D);
    }
}
