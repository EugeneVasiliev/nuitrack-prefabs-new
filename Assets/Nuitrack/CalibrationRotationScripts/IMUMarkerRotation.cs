using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class IMUMarkerRotation : MonoBehaviour 
{
	Vector3 gyroGravity = Vector3.down;
	Vector3 gyroRateUnbiased = Vector3.zero;
	
	Vector3 crossProd = Vector3.zero;
	
	Vector3 
		smoothedGravity = Vector3.zero;
	
  [SerializeField]float slerpVectorsCoeff = 20f;
  [SerializeField]float slerpRotationsCoeff = 0.5f;
  [SerializeField]float slerpMarkerRotationsCoeff = 0.5f;
  [SerializeField]bool useGyro = true;

	Quaternion baseRotation = Quaternion.identity;
	Quaternion rotation = Quaternion.identity;
	Quaternion finalRotation = Quaternion.identity;
	public Quaternion Rotation {get {return finalRotation;}}

  static IMUMarkerRotation instance;

  bool havePrevMarkerRotation = false;
  Quaternion prevMarkerRotation = Quaternion.identity;

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

  [SerializeField]Renderer markerSignalRenderer = null;

  void Rotate()
  {
		gyroGravity = Input.gyro.gravity;
		gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);

    smoothedGravity = Vector3.Slerp(smoothedGravity, gyroGravity, slerpVectorsCoeff * Time.unscaledDeltaTime);

    if (useGyro)
    {
      Quaternion deltaRot = Quaternion.Euler(Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f)) * Mathf.Rad2Deg * Time.unscaledDeltaTime);
      rotation = rotation * deltaRot;
      if (havePrevMarkerRotation) prevMarkerRotation = prevMarkerRotation * deltaRot;
    }
	
    //gravity correction:
    Quaternion gravityDiff = Quaternion.FromToRotation(rotation * gyroGravity, Vector3.down);
    Vector3 eulerDiff = gravityDiff.eulerAngles;
    Vector3 gravityDiffXZ = new Vector3(eulerDiff.x, 0f, 0f/*eulerDiff.z*/);
    Quaternion correction =  Quaternion.Euler(gravityDiffXZ);
    rotation = Quaternion.Slerp(rotation, correction * rotation, slerpRotationsCoeff * Time.unscaledDeltaTime);

    const float maxAngularSpeedMarker = 10f;
    const float minRotationSpeedForCorrection = 10f;

    float rotationSpeed = Input.gyro.rotationRateUnbiased.magnitude * Mathf.Rad2Deg;

    if (MarkerData.haveData[CurrentUserTracker.CurrentUser] && (rotationSpeed < maxAngularSpeedMarker))
    {
      markerSignalRenderer.enabled = true;
      MarkerData.haveData[CurrentUserTracker.CurrentUser] = false;
      prevMarkerRotation = MarkerData.markerRotations[CurrentUserTracker.CurrentUser];
      UpdateSensorOrientation();
      if (!havePrevMarkerRotation)
      {
        rotation = CalibrationInfo.SensorOrientation * MarkerData.markerRotations[CurrentUserTracker.CurrentUser];
      }
      havePrevMarkerRotation = true;
    }
    else
    {
      markerSignalRenderer.enabled = false;
      MarkerData.haveData[CurrentUserTracker.CurrentUser] = false;
    }

    if (havePrevMarkerRotation/* && (rotationSpeed > minRotationSpeedForCorrection) */)
    {
      //UpdateSensorOrientation();
      const float maxSpeedMult = 0.5f;
      Quaternion slerpResult = Quaternion.Slerp(rotation, Quaternion.Inverse(CalibrationInfo.SensorOrientation) * prevMarkerRotation, Time.unscaledDeltaTime * slerpMarkerRotationsCoeff);
      debugTxt.text = (Quaternion.Inverse(Quaternion.Inverse(CalibrationInfo.SensorOrientation) * prevMarkerRotation) * rotation).eulerAngles.ToString("0.00") + System.Environment.NewLine + 
        (Quaternion.Inverse(Quaternion.RotateTowards(rotation, slerpResult, maxSpeedMult * rotationSpeed * Time.unscaledDeltaTime)) * rotation).eulerAngles.ToString("0.00");
      rotation = Quaternion.RotateTowards(rotation, slerpResult, maxSpeedMult * rotationSpeed * Time.unscaledDeltaTime);
    }

    finalRotation = baseRotation * rotation;
  }

  //TODO: remove debug UI
  [SerializeField]UnityEngine.UI.Text debugTxt;

  void UpdateSensorOrientation()
  {
    Vector3 rotatedGravity = prevMarkerRotation * gyroGravity;
    //debugTxt.text = rotatedGravity.ToString("0.00");

    if (onMarkerSensorOrientationUpdate != null)
    {
      Quaternion newSensorOrientation =  Quaternion.Euler(Mathf.Atan2(-rotatedGravity.z, -rotatedGravity.y) * Mathf.Rad2Deg, 0f, 0f);
      //debugTxt.text = newSensorOrientation.eulerAngles.ToString("0.00");
      onMarkerSensorOrientationUpdate(newSensorOrientation);
    }
  }
}
