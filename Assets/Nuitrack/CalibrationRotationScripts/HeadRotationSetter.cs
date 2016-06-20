using UnityEngine;
using System.Collections;

public class HeadRotationSetter : MonoBehaviour 
{
  TPoseCalibration tPoseCalibration;
  IMURotation imuRotation;

	void Start () 
  {
    tPoseCalibration = FindObjectOfType<TPoseCalibration>();
    if (tPoseCalibration != null) tPoseCalibration.onSuccess += TPoseCalibration_onSuccess;
    imuRotation = IMURotation.Instance;
	}

  void TPoseCalibration_onSuccess (Quaternion rotation)
  {
    imuRotation.SetBaseRotation(rotation);
  }
}
