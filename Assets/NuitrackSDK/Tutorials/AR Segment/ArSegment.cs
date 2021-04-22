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

    Plane floorPlane;
    [SerializeField] Transform rootMainScene;
    [SerializeField] Transform floorRoot;

    [SerializeField] float deltaHieght = 0.1f;
    [SerializeField] float deltaAndle = 5f;
    [SerializeField] float lerpMove = 4f;

    [SerializeField] float planeToCameraDistance;

    void Start()
    {
        // Segment map

        depthKernelIndex = depthToTexture.FindKernel("Depth2Texture");
        depthToTexture.GetKernelThreadGroupSizes(depthKernelIndex, out xDepth, out yDepth, out zDepth);

        UpdateSkelet();
    }

    Vector3 floorPoint;
    Vector3 floorNormal;

    void Update()
    {
        UpdateRGB();
        UpdateHieghtMap();
        UpdateFloor();
        //UpdateSkelet();
    }

    private void OnDrawGizmos()
    {
        UnityEditor.Handles.DrawWireDisc(floorPoint, floorNormal, 2f);
    }

    void UpdateRGB()
    {
        //nuitrack.ColorFrame frame = NuitrackManager.ColorFrame;
        DataProvider.DPFrame frame = dataProvider.RGBFrame;

        if (frame == null)
            return;

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
        DataProvider.DPFrame frame = dataProvider.DepthFrame;

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

            outMat.SetTexture("_HeightMap", depthRenderTexture);
        }

        Marshal.Copy(frame.Data, depthDataArray, 0, depthDataArray.Length);
        sourceDataBuffer.SetData(depthDataArray);

        
        depthToTexture.Dispatch(depthKernelIndex, depthRenderTexture.width / (int)xDepth, depthRenderTexture.height / (int)yDepth, (int)zDepth);

        

        Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(mainCamera.transform.position);
        outMat.SetVector("_CameraPosition", localCameraPosition);
    }

    void UpdateFloor()
    {
        DataProvider.DPUserFrame userFrame = dataProvider.UserFrame;
        //nuitrack.UserFrame userFrame = NuitrackManager.UserFrame;

        if (userFrame == null)
            return;

        Vector3 newFloorPoint = userFrame.Floor.ToVector3() * 0.001f;
        Vector3 newFloorNormal = userFrame.FloorNormal.ToVector3().normalized;
        Plane newFloor = new Plane(newFloorNormal, newFloorPoint);

        if (floorPlane.Equals(default))
        {
            floorPoint = newFloorPoint;
            floorNormal = newFloorNormal;
            floorPlane = new Plane(floorNormal, floorPoint);
        }

        Vector3 newFloorSensor = newFloor.ClosestPointOnPlane(rootMainScene.position);
        Vector3 floorSensor = floorPlane.ClosestPointOnPlane(rootMainScene.position);

        if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAndle || Mathf.Abs(newFloorSensor.y - floorSensor.y) >= deltaHieght)
        {
            floorPoint = newFloorPoint;
            floorNormal = newFloorNormal;
            floorPlane = new Plane(floorNormal, floorPoint);

        }

        Vector3 forward = rootMainScene.forward;
        Vector3.OrthoNormalize(ref floorNormal, ref forward);

        rootMainScene.localRotation = Quaternion.RotateTowards(rootMainScene.localRotation, Quaternion.LookRotation(forward, floorNormal), Time.deltaTime * lerpMove);

        Vector3 localRoot = floorRoot.localPosition;
        localRoot.y = floorSensor.y;
        floorRoot.localPosition = Vector3.MoveTowards(floorRoot.localPosition, localRoot, Time.deltaTime * lerpMove);
    }

    [SerializeField] Transform sensorCenter;
    [SerializeField] GameObject jointObj;

    void UpdateSkelet()
    {
        //nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
        DataProvider.DPSkeleton skeleton = dataProvider.CurrentSkeleton;

        if (skeleton == null)
            return;

        foreach(nuitrack.JointType jointType in System.Enum.GetValues(typeof(nuitrack.JointType)))
        {
            Vector3 position = skeleton.GetJoint(jointType).Real.ToVector3() * 0.001f;

            GameObject joint = Instantiate(jointObj, sensorCenter);
            joint.transform.localPosition = position;
        }
    }

    private void OnDestroy()
    {
        Destroy(depthRenderTexture);
        Destroy(dstRgbTexture2D);
    }
}
