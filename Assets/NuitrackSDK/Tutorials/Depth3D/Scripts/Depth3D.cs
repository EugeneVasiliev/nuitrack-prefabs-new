using System.Runtime.InteropServices;

using UnityEngine;

public class Depth3D : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [Header("RGB shader")]
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

    [Header("Mesh generator")]
    [SerializeField] MeshGenerator meshGenerator;

    [Header("Floor")]
    Plane floorPlane;
    [SerializeField] Transform sensorSpace;

    [SerializeField] float deltaHieght = 0.1f;
    [SerializeField] float deltaAndle = 5f;
    [SerializeField] float lerpMove = 4f;

    void Start()
    {
        depthKernelIndex = depthToTexture.FindKernel("Depth2Texture");
        depthToTexture.GetKernelThreadGroupSizes(depthKernelIndex, out xDepth, out yDepth, out zDepth);
    }

    Vector3 floorPoint;
    Vector3 floorNormal;

    void Update()
    {
        UpdateRGB();
        FitMeshIntoFrame();

        UpdateHieghtMap();
        UpdateFloor();
    }

    void FitMeshIntoFrame()
    {
        nuitrack.ColorFrame frame = NuitrackManager.ColorFrame;

        if (frame == null)
            return;

        float cameraToPlaneDist = Vector3.Distance(meshGenerator.transform.position, mainCamera.transform.position);
        float frameAspectRatio = (float)frame.Cols / frame.Rows;

        float v_angle = mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;

        float scale;

        if (mainCamera.aspect < frameAspectRatio)
        {
            float radHFOV = Mathf.Atan(Mathf.Tan(v_angle) * mainCamera.aspect);
            scale = cameraToPlaneDist * Mathf.Tan(radHFOV);
        }
        else
        {
            scale = cameraToPlaneDist * Mathf.Tan(v_angle);
            scale *= (float)frame.Cols / frame.Rows;
        }

        meshGenerator.transform.localScale = new Vector3(scale * 2, scale * 2, 1);
    }

    void UpdateRGB()
    {
        nuitrack.ColorFrame frame = NuitrackManager.ColorFrame;

        if (frame == null)
            return;

        if (dstRgbTexture2D == null)
        {
            dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            meshGenerator.Generate(frame.Cols, frame.Rows);
        }

        dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        dstRgbTexture2D.Apply();

        meshGenerator.Material.SetTexture("_MainTex", dstRgbTexture2D);
    }

    void UpdateHieghtMap()
    {
        nuitrack.DepthFrame frame = NuitrackManager.DepthFrame;

        if (frame == null)
            return;

        if (depthRenderTexture == null || depthRenderTexture.width != frame.Cols || depthRenderTexture.height != frame.Rows)
        {
            depthRenderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            depthRenderTexture.enableRandomWrite = true;
            depthRenderTexture.filterMode = FilterMode.Point;

            depthRenderTexture.Create();

            depthToTexture.SetInt("textureWidth", depthRenderTexture.width);
            depthToTexture.SetTexture(depthKernelIndex, "Result", depthRenderTexture);
            depthToTexture.SetFloat("contrast", 0);

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

        depthToTexture.Dispatch(depthKernelIndex, depthRenderTexture.width / (int)xDepth, depthRenderTexture.height / (int)yDepth, (int)zDepth);

        meshGenerator.Material.SetTexture("_HeightMap", depthRenderTexture);

        Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(mainCamera.transform.position);
        meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);
    }

    void UpdateFloor()
    {
        nuitrack.UserFrame userFrame = NuitrackManager.UserFrame;

        if (userFrame == null)
            return;

        Vector3 newFloorPoint = userFrame.Floor.ToVector3() * 0.001f;
        Vector3 newFloorNormal = userFrame.FloorNormal.ToVector3().normalized;

        Plane newFloor = new Plane(newFloorNormal, newFloorPoint);

        if (floorPlane.Equals(default(Plane)))
        {
            floorPoint = newFloorPoint;
            floorNormal = newFloorNormal;
            floorPlane = new Plane(floorNormal, floorPoint);
        }

        Vector3 newFloorSensor = newFloor.ClosestPointOnPlane(Vector3.zero);
        Vector3 floorSensor = floorPlane.ClosestPointOnPlane(Vector3.zero);

        if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAndle || Mathf.Abs(newFloorSensor.y - floorSensor.y) >= deltaHieght)
        {
            floorPoint = newFloorPoint;
            floorNormal = newFloorNormal;
            floorPlane = new Plane(floorNormal, floorPoint);
        }

        Vector3 reflectNormal = Vector3.Reflect(-floorNormal, Vector3.up);
        Vector3 forward = Vector3.forward;
        Vector3.OrthoNormalize(ref reflectNormal, ref forward);

        mainCamera.transform.rotation = Quaternion.RotateTowards(mainCamera.transform.rotation, Quaternion.LookRotation(forward, reflectNormal), Time.deltaTime * lerpMove);

        Vector3 localRoot = mainCamera.transform.position;
        localRoot.y = -floorSensor.y;
        mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, localRoot, Time.deltaTime * lerpMove);
    }

    private void OnDestroy()
    {
        Destroy(depthRenderTexture);
        Destroy(dstRgbTexture2D);
    }
}
