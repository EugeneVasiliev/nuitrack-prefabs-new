using UnityEngine;

namespace NuitrackSDK.Frame
{
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu("NuitrackSDK/SensorEnvironment/Sensor Space")]
    public class SensorSpace : MonoBehaviour
    {
        new Camera camera;

        [Header("Camera")]
        [SerializeField] bool cameraFovAlign = true;

        [Tooltip ("(optional) If not specified, the screen size is used.")]
        [SerializeField] Canvas viewCanvas;
        RectTransform canvasRect;

        ulong lastTimeStamp = 0;

        [Header("Floor")]
        [SerializeField] bool floorTracking = false;

        [SerializeField] Transform sensorSpace;

        [SerializeField, Range(0.001f, 1f)] float deltaHeight = 0.1f;
        [SerializeField, Range(0.1f, 90f)] float deltaAngle = 3f;
        [SerializeField, Range(0.1f, 32f)] float floorCorrectionSpeed = 8f;

        Plane floorPlane;

        public Camera Camera
        {
            get
            {
                if (camera == null)
                    camera = GetComponent<Camera>();

                return camera;
            }
        }

        RectTransform CanvasRect
        {
            get
            {
                if (viewCanvas != null && canvasRect == null)
                    canvasRect = viewCanvas.GetComponent<RectTransform>();
                
                return canvasRect;
            }
        }

        void Update()
        {
            if (cameraFovAlign)
            {
                if (NuitrackManager.ColorFrame == null || NuitrackManager.ColorFrame.Timestamp == lastTimeStamp)
                    return;

                lastTimeStamp = NuitrackManager.ColorFrame.Timestamp;

                NuitrackManager_onColorUpdate(NuitrackManager.ColorFrame);
            }

            if (floorTracking)
                UpdateFloor();
        }

        float ViewWidth
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.width;
                else
                    return Screen.width;
            }
        }

        float ViewHeight
        {
            get
            {
                if (CanvasRect != null)
                    return CanvasRect.rect.height;
                else
                    return Screen.height;
            }
        }

        void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
        {
            float frameAspectRatio = (float)frame.Cols / frame.Rows;

            nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

            //The frame from the sensor fills the screen and the FOV is
            //determined for the axis along which the frame reaches the edges of the screen.
            //If the screen is wider than the frame from the sensor, then the alignment will
            //occur according to the inverse aspect ratio of the frame(otherwise the screen).
            float targetAspectRatio = ViewWidth / ViewHeight > frameAspectRatio ?
                (float)frame.Rows / frame.Cols : ViewHeight / ViewWidth;

            //Setting the camera's vFOV equal to the depth sensor's vFOV. 
            // Nuitrack does not yet have a representation of vFOV, so we use the hFOV to vFOV conversion.
            float vFOV = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV * 0.5f) * targetAspectRatio);

            Camera.fieldOfView = vFOV * Mathf.Rad2Deg;
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