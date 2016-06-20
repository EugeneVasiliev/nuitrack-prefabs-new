using UnityEngine;
using System.Collections;

public class CalibrationInfo : MonoBehaviour 
{
  TPoseCalibration calibration;

  static Quaternion sensorOrientation = Quaternion.identity;
  public static Quaternion SensorOrientation {get {return sensorOrientation;}}

  void Start () 
  {
    DontDestroyOnLoad(gameObject);
    calibration = FindObjectOfType<TPoseCalibration>();
    calibration.onSuccess += Calibration_onSuccess;
  }

  //can be used for sensor (angles, floor distance, maybe?) / user calibration (height, lengths)
  void Calibration_onSuccess (Quaternion rotation)
  {
    Vector3 torso = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso).ToVector3();
    Vector3 neck = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Neck).ToVector3();
    Vector3 diff = neck - torso;

    sensorOrientation = Quaternion.Euler(Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);
  }
}
