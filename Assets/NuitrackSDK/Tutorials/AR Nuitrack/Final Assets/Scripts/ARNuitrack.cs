using UnityEngine;
using NuitrackSDK.Frame;


namespace NuitrackSDK.Tutorials.ARNuitrack
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/AR Nuitrack")]
    public class ARNuitrack : MonoBehaviour
    {
        Texture2D rgbTexture;
        Texture2D depthTexture;

        [Header("Mesh generator")]
        [SerializeField] MeshGenerator meshGenerator;
        [SerializeField] new Camera camera;

        [Header("Floor")]
        [SerializeField] Transform sensorSpace;

        Plane floorPlane;

        [SerializeField, Range(0.001f, 1f)] float deltaHeight = 0.1f;
        [SerializeField, Range(0.1f, 90f)] float deltaAngle = 3f;
        [SerializeField, Range(0.1f, 32f)] float floorCorrectionSpeed = 8f;

        void Update()
        {
            rgbTexture = NuitrackManager.ColorFrame.ToTexture2D();
            depthTexture = NuitrackManager.DepthFrame.ToTexture2D();

            if (rgbTexture == null || depthTexture == null)
                return;

            if (meshGenerator.Mesh == null)
                meshGenerator.Generate(depthTexture.width, depthTexture.height);

            meshGenerator.Material.SetTexture("_MainTex", rgbTexture);
            meshGenerator.Material.SetTexture("_DepthTex", depthTexture);

            meshGenerator.transform.localPosition = Vector3.forward * FrameUtils.DepthToTexture.MaxSensorDepth;

            Vector3 localCameraPosition = meshGenerator.transform.InverseTransformPoint(camera.transform.position);
            meshGenerator.Material.SetVector("_CameraPosition", localCameraPosition);

            FitMeshIntoFrame();

            UpdateFloor();
        }

        void FitMeshIntoFrame()
        {
            float frameAspectRatio = (float)depthTexture.width / depthTexture.height;
            float targetAspectRatio = camera.aspect < frameAspectRatio ? camera.aspect : frameAspectRatio;

            float vAngle = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float scale = Vector3.Distance(meshGenerator.transform.position, camera.transform.position) * Mathf.Tan(vAngle) * targetAspectRatio;

            meshGenerator.transform.localScale = new Vector3(scale * 2, scale * 2, 1);
        }

        void UpdateFloor()
        {
            if (NuitrackManager.Floor == null)
                return;

            Plane newFloor = (Plane)NuitrackManager.Floor;

            if (floorPlane.Equals(default(Plane)))
                floorPlane = newFloor;

            Vector3 newFloorSensor = newFloor.ClosestPointOnPlane(Vector3.zero);
            Vector3 floorSensor = floorPlane.ClosestPointOnPlane(Vector3.zero);

            if (Vector3.Angle(newFloor.normal, floorPlane.normal) >= deltaAngle || Mathf.Abs(newFloorSensor.y - floorSensor.y) >= deltaHeight)
                floorPlane = newFloor;

            Vector3 reflectNormal = Vector3.Reflect(-floorPlane.normal, Vector3.up);
            Vector3 forward = sensorSpace.forward;
            Vector3.OrthoNormalize(ref reflectNormal, ref forward);

            Quaternion targetRotation = Quaternion.LookRotation(forward, reflectNormal);
            camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, targetRotation, Time.deltaTime * floorCorrectionSpeed);

            Vector3 localRoot = camera.transform.localPosition;
            localRoot.y = -floorSensor.y;
            camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localRoot, Time.deltaTime * floorCorrectionSpeed);
        }
    }
}