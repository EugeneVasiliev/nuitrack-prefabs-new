using UnityEngine;


namespace NuitrackSDK.Calibration
{
    public class CalibrationInfo : MonoBehaviour
    {
        TPoseCalibration calibration;

        static Quaternion sensorOrientation = Quaternion.identity;
        static Quaternion sensorOrientationMarker = Quaternion.identity;
        public static Quaternion SensorOrientation { get { return sensorOrientation; } }

        [SerializeField] bool useCalibrationSensorOrientation = false;

#if NUITRACK_MARKER
    [SerializeField]bool useMarkerSensorOrientation = false;
#endif

        //floor height requires UserTracker module to work at the moment, 
        [Tooltip("Floor height tracking requires enabled UserTracker module (in NuitrackManager component)")]
        [SerializeField] bool trackFloorHeight = false;

        public static float FloorHeight
        {
            get; private set;
        } = 1;

        public static void SetSensorHeightManually(float newHeight) //may be used when floor is not tracked / UserTracker not enabled
        {
            FloorHeight = newHeight;
        }

        void Start()
        {
            DontDestroyOnLoad(this);

            if (useCalibrationSensorOrientation)
            {
                calibration = FindObjectOfType<TPoseCalibration>();

                if (calibration != null)
                    calibration.onSuccess += Calibration_onSuccess;
            }

#if NUITRACK_MARKER
        if (useMarkerSensorOrientation)
        {
            IMUMarkerRotation markerRotation = FindObjectOfType<IMUMarkerRotation>();
            if (markerRotation != null) markerRotation.onMarkerSensorOrientationUpdate += OnMarkerCorrectionEvent;
        }
#endif
        }

        //can be used for sensor (angles, floor distance, maybe?) / user calibration (height, lengths)
        void Calibration_onSuccess(Quaternion rotation)
        {
            //sensor orientation:
            UserData.SkeletonData skeleton = NuitrackManager.Users.Current.Skeleton;

            Vector3 torso = skeleton.GetJoint(nuitrack.JointType.Torso).Position;
            Vector3 neck = skeleton.GetJoint(nuitrack.JointType.Neck).Position;
            Vector3 diff = neck - torso;
            sensorOrientation = Quaternion.Euler(-Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);

            //floor height:
            if (trackFloorHeight && NuitrackManager.Floor != null)
            {
                Plane floorPlane = (Plane)NuitrackManager.Floor;

                if (floorPlane.normal.sqrMagnitude > 0.01f) //
                    FloorHeight = floorPlane.GetDistanceToPoint(Vector3.zero);
            }
        }

        void OnMarkerCorrectionEvent(Quaternion newSensorOrientation)
        {
            sensorOrientationMarker = newSensorOrientation;
            sensorOrientation = Quaternion.Slerp(sensorOrientation, newSensorOrientation, 0.01f);
        }

        void Update()
        {
            const float minAngularSpeedForCorrection = 10f;
            const float slerpMult = 10f;
            float angularSpeed = Input.gyro.rotationRateUnbiased.magnitude * Mathf.Rad2Deg;
            if (angularSpeed > minAngularSpeedForCorrection)
            {
                sensorOrientation = Quaternion.Slerp(sensorOrientation, sensorOrientationMarker, Time.unscaledDeltaTime * slerpMult);
            }
        }
    }
}