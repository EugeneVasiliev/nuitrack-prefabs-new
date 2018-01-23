using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotation : MonoBehaviour {

    static bool versionChecked = false;
    public static bool cameraJointVersion = false;

    bool firstCheck = false;

    [SerializeField]Transform vrCamera;

    [Header("GearVR Camera settings")]
    [SerializeField] Transform headBase;
    [SerializeField] Transform yCorrection;
    [SerializeField] Transform gearvrCamera;

    void NativeRecenter(Quaternion rot)
    {
        if (cameraJointVersion) // in versions VicoVR below 1.5.0, this script should be disabled in the version check
        {
            float sensorOrientationAngle = Mathf.Deg2Rad * TPoseCalibration.SensorOrientation.eulerAngles.x;
            //float yAngle;

            //yAngle = Mathf.Deg2Rad * rot.eulerAngles.y;
            float yAngle = Mathf.Deg2Rad * (rot.eulerAngles.y + 180);

            nuitrack.PublicNativeImporter.nuitrack_Recenter(CurrentUserTracker.CurrentUser, sensorOrientationAngle, yAngle);
        }
    }

    void OnDestroy()
    {
        if(cameraJointVersion) FindObjectOfType<TPoseCalibration>().onSuccess -= NativeRecenter;
    }

    void Update () {

        if (CurrentUserTracker.CurrentUser == 0)
            return;

        //Run once
        if (!versionChecked)
        {
            nuitrack.Joint curCamJoint = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.None);

            Vector3 camVec = new Vector3(curCamJoint.Orient.Matrix[1], curCamJoint.Orient.Matrix[4], curCamJoint.Orient.Matrix[7]);

            if (camVec.magnitude > 0.9) //The norm should be equal to 1.
            {
                Debug.Log("VERSION with camera joint");
                cameraJointVersion = true;
            }
            else
            {
                Debug.Log("VERSION without camera joint");
            }
            versionChecked = true;
        }

        if (!firstCheck && versionChecked)
        {
            if (cameraJointVersion)
            {
                FindObjectOfType<TPoseCalibration>().onSuccess += NativeRecenter;
            }

            Debug.Log("cameraJointVersion: " + cameraJointVersion);

            firstCheck = true;
        }

        if (cameraJointVersion)
        {
            //Vector3 cameraPosition = 0.001f * (TPoseCalibration.SensorOrientation * CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.None).ToVector3());
            Quaternion cameraRotation = TPoseCalibration.SensorOrientation * CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.None).ToQuaternion();

            //transform.position = 0.001f * (TPoseCalibration.SensorOrientation * CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.None).ToVector3());
            if (GameVersion.currentPlatform == Platform.Default)
            {
                cameraRotation.eulerAngles = cameraRotation.eulerAngles + new Vector3(0, 180, 0);
                vrCamera.eulerAngles = cameraRotation.eulerAngles;
            }
            else
            {
                //headBase.position = cameraPosition;

                Vector3 gazeDirection = gearvrCamera.forward;
                Vector3 gazeDirHead = headBase.InverseTransformVector(gazeDirection);

                Quaternion currentRotation = Quaternion.Euler(0f, Mathf.Atan2(gazeDirHead.x, gazeDirHead.z) * Mathf.Rad2Deg, 0f);

                Vector3 vicovrDirection = cameraRotation * Vector3.forward; // из п.5
                Quaternion yPartRotation = Quaternion.Euler(0f, Mathf.Atan2(vicovrDirection.x, vicovrDirection.z) * Mathf.Rad2Deg, 0f);

                Quaternion correction = yPartRotation * Quaternion.Inverse(currentRotation);
                float delta = Mathf.Atan2((correction * Vector3.forward).x, (correction * Vector3.forward).z) * Mathf.Rad2Deg;
                Quaternion corrected = yCorrection.localRotation * correction;
                float slerpCoef = 0.01f;

                if (Mathf.Abs(delta) > 40.0)
                    slerpCoef = 1.0f;

                yCorrection.localRotation = Quaternion.Slerp(yCorrection.localRotation, corrected, slerpCoef);
            }
        }
	}
}
