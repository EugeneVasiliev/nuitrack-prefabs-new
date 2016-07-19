using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class IMUMarkerRotation : MonoBehaviour 
{
  #if NUITRACK_MARKER
	Vector3 gyroGravity = Vector3.down;
	Vector3 gyroRateUnbiased = Vector3.zero;
	
	Vector3 crossProd = Vector3.zero;
	
  [SerializeField]float slerpVectorsCoeff = 20f;
  [SerializeField]float slerpRotationsCoeff = 0.5f;
  [SerializeField]float slerpMarkerRotationsCoeff = 5f;
  [SerializeField]float hardResetMarkerTreshold = 20f;

	Quaternion baseRotation = Quaternion.identity;
	Quaternion rotation = Quaternion.identity;
	Quaternion finalRotation = Quaternion.identity;
	public Quaternion Rotation {get {return finalRotation;}}

  static IMUMarkerRotation instance;

  bool havePrevMarkerRotation = false;
  Quaternion prevMarkerRotation = Quaternion.identity;

  [SerializeField]AnimationCurve rotationSpeedMult;

  [SerializeField]Renderer markerSignalRenderer = null;

  public delegate void OnMarkerSensorOrientationUpdate(Quaternion newSensorOrientation);

  public event OnMarkerSensorOrientationUpdate onMarkerSensorOrientationUpdate;

  public static IMUMarkerRotation Instance
  {
    get 
    {
      if (instance == null)
      {
        instance = FindObjectOfType<IMUMarkerRotation>();
        if (instance == null)
        {
          GameObject container = new GameObject();
          container.name = "IMUMarkerRotation";
          instance = container.AddComponent<IMUMarkerRotation>();
        }
        DontDestroyOnLoad(instance);
      }
      return instance;
    }
  }

  void Start () 
	{
    if (instance == null) instance = this;
    else if (instance != this) Destroy(this);

    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    Input.gyro.enabled = true;
  }

  void OnDestroy()
  {
    if (instance == this) instance = null;
  }

	void Update () 
	{
		Rotate();
  }

  void Rotate()
  {
		gyroGravity = Input.gyro.gravity;
		gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);

    Quaternion deltaRot = Quaternion.Euler(Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f)) * Mathf.Rad2Deg * Time.unscaledDeltaTime);
    rotation = rotation * deltaRot;
    if (havePrevMarkerRotation) prevMarkerRotation = prevMarkerRotation * deltaRot;

    //gravity correction:
    Vector3 rotatedGravity = rotation * gyroGravity;
    Quaternion correction  =  Quaternion.Euler(Mathf.Atan2(rotatedGravity.z, -rotatedGravity.y) * Mathf.Rad2Deg, 0f, Mathf.Atan2(-rotatedGravity.x, -rotatedGravity.y) * Mathf.Rad2Deg);

    rotation = Quaternion.Slerp(rotation, correction * rotation, slerpRotationsCoeff * Time.unscaledDeltaTime);

    const float maxAngularSpeedMarker = 10f;
    const float minRotationSpeedForCorrection = 10f;

    float rotationSpeed = Input.gyro.rotationRateUnbiased.magnitude * Mathf.Rad2Deg;

    #region MARKER

    if (MarkerData.haveData[CurrentUserTracker.CurrentUser] && (rotationSpeed < maxAngularSpeedMarker))
    {
      markerSignalRenderer.enabled = true;
      MarkerData.haveData[CurrentUserTracker.CurrentUser] = false;
      prevMarkerRotation = MarkerData.markerRotations[CurrentUserTracker.CurrentUser];
      UpdateSensorOrientation();
     
      float angularDiff = Quaternion.Angle(rotation, (Quaternion.Inverse(CalibrationInfo.SensorOrientation) * MarkerData.markerRotations[CurrentUserTracker.CurrentUser]));

      if ((!havePrevMarkerRotation) || (angularDiff > hardResetMarkerTreshold))
      {
        rotation = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * MarkerData.markerRotations[CurrentUserTracker.CurrentUser];
        havePrevMarkerRotation = true;
      }
    }
    else
    {
      markerSignalRenderer.enabled = false;
      MarkerData.haveData[CurrentUserTracker.CurrentUser] = false;
    }

    if (havePrevMarkerRotation)
    {
      Quaternion slerpResult = Quaternion.Slerp(rotation, Quaternion.Inverse(CalibrationInfo.SensorOrientation) * prevMarkerRotation, Time.unscaledDeltaTime * slerpMarkerRotationsCoeff);
      rotation = Quaternion.RotateTowards(rotation, slerpResult, rotationSpeedMult.Evaluate(rotationSpeed) * rotationSpeed * Time.unscaledDeltaTime);
    }

    #endregion

    finalRotation = baseRotation * rotation;
  }

  void UpdateSensorOrientation()
  {
    Vector3 rotatedGravity = prevMarkerRotation * gyroGravity;

    if (onMarkerSensorOrientationUpdate != null)
    {
      Quaternion newSensorOrientation =  Quaternion.Euler(-Mathf.Atan2(rotatedGravity.z, -rotatedGravity.y) * Mathf.Rad2Deg, 0f, 0f);
      onMarkerSensorOrientationUpdate(newSensorOrientation);
    }
  }
  #endif
}
