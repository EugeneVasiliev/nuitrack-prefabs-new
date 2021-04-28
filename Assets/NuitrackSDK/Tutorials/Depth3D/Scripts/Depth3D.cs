using UnityEngine;
using System.Runtime.InteropServices;

public class Depth3D : MonoBehaviour
{
    [SerializeField] Camera mainCamera;

    [Header("RGB shader")]
    Texture2D dstRgbTexture2D;

    [Header("Segment shader")]
    ComputeBuffer sourceDataBuffer;
    byte[] depthDataArray = null;

    [Header("Mesh generator")]
    [SerializeField] MeshGenerator meshGenerator;

    [Header("Floor")]
    [SerializeField] Transform sensorSpace;

    Plane floorPlane;

    [SerializeField] float deltaHieght = 0.1f;
    [SerializeField] float deltaAndle = 3f;
    [SerializeField] float speedFloorCorrection = 8f;

    ulong frameTimestamp;

    void Update()
    {
        nuitrack.ColorFrame colorFrame = NuitrackManager.ColorFrame;
        nuitrack.DepthFrame depthFrame = NuitrackManager.DepthFrame;
        nuitrack.UserFrame userFrame = NuitrackManager.UserFrame;

        if (colorFrame == null || depthFrame == null  || userFrame == null || frameTimestamp == depthFrame.Timestamp)
            return;

        frameTimestamp = depthFrame.Timestamp;
 
        UpdateRGB(colorFrame);
        UpdateHieghtMap(depthFrame);
 
        UpdateFloor(userFrame);
    }

    void UpdateRGB(nuitrack.ColorFrame frame)
    {
        if (dstRgbTexture2D == null)
        {
            meshGenerator.Generate(frame.Cols, frame.Rows);

            dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
            meshGenerator.Material.SetTexture("_MainTex", dstRgbTexture2D);
        }

        dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
        dstRgbTexture2D.Apply(); 

        FitMeshIntoFrame(frame);
    }

    void FitMeshIntoFrame(nuitrack.ColorFrame frame)
    {        
        float frameAspectRatio = (float)frame.Cols / frame.Rows;
        float targetAspectRatio = mainCamera.aspect < frameAspectRatio ? mainCamera.aspect : frameAspectRatio;

        float v_angle = mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float scale = Vector3.Distance(meshGenerator.transform.position, mainCamera.transform.position) * Mathf.Tan(v_angle) * targetAspectRatio;

        meshGenerator.transform.localScale = new Vector3(scale * 2, scale * 2, 1);
    }

    void UpdateHieghtMap(nuitrack.DepthFrame frame)
    {
        if (sourceDataBuffer == null)
        {
            depthDataArray = new byte[frame.DataSize];

            //We put the source data in the buffer, but the buffer does not support types
            //that take up less than 4 bytes(instead of ushot(Int16), we specify uint(Int32))
            sourceDataBuffer = new ComputeBuffer(frame.DataSize / 2, sizeof(uint));

            meshGenerator.Material.SetInt("_textureWidth", frame.Cols);
            meshGenerator.Material.SetInt("_textureHeight", frame.Rows);
            meshGenerator.Material.SetBuffer("_DepthFrame", sourceDataBuffer);
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        sourceDataBuffer.SetData(depthDataArray);

        Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(mainCamera.transform.position);
        meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);
    }

    void UpdateFloor(nuitrack.UserFrame frame)
    {
        Vector3 floorPoint = frame.Floor.ToVector3() * 0.001f;
        Vector3 floorNormal = frame.FloorNormal.ToVector3().normalized;

        Plane newFloor = new Plane(floorNormal, floorPoint);

        if (floorPlane.Equals(default(Plane)))
            floorPlane = new Plane(floorNormal, floorPoint);

        Vector3 newFloorSensor = newFloor.ClosestPointOnPlane(Vector3.zero);
        Vector3 floorSensor = floorPlane.ClosestPointOnPlane(Vector3.zero);

        if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAndle || Mathf.Abs(newFloorSensor.y - floorSensor.y) >= deltaHieght)
            floorPlane = new Plane(floorNormal, floorPoint);

        Vector3 reflectNormal = Vector3.Reflect(-floorPlane.normal, Vector3.up);
        Vector3 forward = sensorSpace.forward;
        Vector3.OrthoNormalize(ref reflectNormal, ref forward);

        Quaternion targetRotation = Quaternion.LookRotation(forward, reflectNormal);
        mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, targetRotation, Time.deltaTime * speedFloorCorrection);

        Vector3 localRoot = mainCamera.transform.localPosition;
        localRoot.y = -floorSensor.y;
        mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, localRoot, Time.deltaTime * speedFloorCorrection);
    }

    private void OnDestroy()
    {
        Destroy(dstRgbTexture2D);
    }
}
