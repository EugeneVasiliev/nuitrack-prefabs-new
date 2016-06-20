using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class IMURotation : MonoBehaviour 
{
	Vector3 magneticHeading = Vector3.zero;
	Vector3 gyroGravity = Vector3.down;
	Vector3 gyroRateUnbiased = Vector3.zero;
	
	Vector3 crossProd = Vector3.zero;
	
	Vector3 
		smoothedMagneticHeading = Vector3.zero, 
		smoothedGravity = Vector3.zero;
	
  [SerializeField]float slerpGravityMagneticCoeff = 10f;
  [SerializeField]float magneticCorrectionRotationCoeff = 10f;
	
	Quaternion baseRotation = Quaternion.identity;
	Quaternion rotation = Quaternion.identity;
	Quaternion finalRotation = Quaternion.identity;
	public Quaternion Rotation {get {return finalRotation;}}

  [SerializeField]bool magneticCorrection = true;

	bool correctionOn = false;
  [SerializeField]float magneticCorrectionMinAngleOn = 15f; //minimum angle (between rotation from gravity and magnetic heading and gyro) to turn magnetic correction on
  [SerializeField]float magneticCorretcionMaxAngleOff = 3f; //maximum angle (between rotation from gravity and magnetic heading and gyro) to turn magnetic correction off
  [SerializeField]float magneticCorrectionMinAngularVelocity = 15f; //minimum angular velocity to use magnetic correction (so it won't float when user stays stationary)

  static string ROOM_BASE_ROTATION = "Cookies.RoomBaseRotation"; //Cookies part of config file won't (or at least should not) change on version updates

  static IMURotation instance;

  public static IMURotation Instance
  {
    get 
    {
      if (instance == null)
      {
        instance = FindObjectOfType<IMURotation>();
        if (instance == null)
        {
          GameObject container = new GameObject();
          container.name = "SensorRotation";
          instance = container.AddComponent<IMURotation>();
        }
        DontDestroyOnLoad(instance);
      }
      return instance;
    }
  }

	IEnumerator Start () 
	{
    if (instance == null) instance = this;
    else if (instance != this) Destroy(this);

    Screen.sleepTimeout = SleepTimeout.NeverSleep;
    Input.compass.enabled = true;
    Input.gyro.enabled = true;

    yield return null;
    LoadBaseRotation();
    InitRotation();
	}

  void OnDestroy()
  {
    if (instance == this) instance = null;
  }

  void InitRotation()
  {
    magneticHeading = Input.compass.rawVector;
    magneticHeading = new Vector3(-magneticHeading.y, magneticHeading.x, -magneticHeading.z); // for landscape left
    
    gyroGravity = Input.gyro.gravity;
    gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);
    
    smoothedMagneticHeading = magneticHeading;
    smoothedGravity = gyroGravity;
    
    crossProd = Vector3.Cross(smoothedMagneticHeading, smoothedGravity).normalized;
    
    rotation = Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity));
  }

  void LoadBaseRotation()
  {
    Debug.Log("Loading baseRotation (nuitrack.Nuitrack.GetConfigValue(ROOM_BASE_ROTATION))");
    string configValue = nuitrack.Nuitrack.GetConfigValue(ROOM_BASE_ROTATION);
    Debug.Log("Config value: " + configValue);
    if (configValue == "") return;
    
    byte[] calibrationInfo = Convert.FromBase64String(configValue);
    int index = 0;
    float x, y, z, w;
    x = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    y = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    z = BitConverter.ToSingle(calibrationInfo, index);
    index += sizeof(float);
    w = BitConverter.ToSingle(calibrationInfo, index);
    Quaternion newBaseRotation = new Quaternion(x, y, z, w);
    baseRotation = newBaseRotation;
    Debug.Log("baseRotation: " + baseRotation.ToString());
  }
  
  void SaveBaseRotation()
  {
    Debug.Log("Saving baseRotation (nuitrack.Nuitrack.SetConfigValue(ROOM_BASE_ROTATION, val))");
    List<byte> calibratedBaseRotation = new List<byte>();
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.x));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.y));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.z));
    calibratedBaseRotation.AddRange(BitConverter.GetBytes(baseRotation.w));
    string val = Convert.ToBase64String(calibratedBaseRotation.ToArray());
    nuitrack.Nuitrack.SetConfigValue(ROOM_BASE_ROTATION, val);
  }
  
  public void SetBaseRotation(Quaternion additionalRotation)
  {
    baseRotation = additionalRotation * Quaternion.Inverse(rotation);
    SaveBaseRotation();
  }
	
	void Update () 
	{
		Rotate();
  }

  void Rotate()
  {
    magneticHeading = Input.compass.rawVector;
		magneticHeading = new Vector3(-magneticHeading.y, magneticHeading.x, -magneticHeading.z); // for landscape left
		
		gyroGravity = Input.gyro.gravity;
		gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);
		

    smoothedMagneticHeading = Vector3.Slerp(smoothedMagneticHeading, magneticHeading, slerpGravityMagneticCoeff * Time.unscaledDeltaTime);
    smoothedGravity = Vector3.Slerp(smoothedGravity, gyroGravity, slerpGravityMagneticCoeff * Time.unscaledDeltaTime);

    /*
    Debug.Log(string.Format("Slerp arg: {0:0.00000}", dampCoeffVectors * Time.unscaledDeltaTime));

    Debug.Log(string.Format("Grav_mag_angle: {0:0.00}; Smooth_grav_mag_angle: {1:0.00}", 
      Vector3.Angle(magneticHeading, gyroGravity), 
      Vector3.Angle(smoothedMagneticHeading, smoothedGravity)));
    */
    
		crossProd = Vector3.Cross (smoothedMagneticHeading, smoothedGravity).normalized;

    if (crossProd == Vector3.zero)
    {
      crossProd = Vector3.forward;
    }

    //Quaternion cardboardNewRotation = Cardboard.SDK.HeadPose.Orientation;
    //Quaternion deltaRot = Quaternion.Inverse(prevCardboardHeadOrientation) * cardboardNewRotation;
    //prevCardboardHeadOrientation = cardboardNewRotation;

    Quaternion deltaRot = Quaternion.Euler(Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f)) * Mathf.Rad2Deg * Time.unscaledDeltaTime);
    //Debug.Log(deltaRot.eulerAngles.ToString("0.00"));
    rotation = rotation * deltaRot;
	
    //Debug.Log("Rotation (without base): " + rotation.ToString("0.000"));

    //angle between current rotation and magnetic:
    if (magneticCorrection)
    {
      float deltaAngle = Quaternion.Angle(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)));
      if (deltaAngle > magneticCorrectionMinAngleOn)
      {
        correctionOn = true;
      }
      if (deltaAngle < magneticCorretcionMaxAngleOff)
      {
        correctionOn = false;
      }

      float gyroCorrectionMinSpeed = Mathf.Deg2Rad * magneticCorrectionMinAngularVelocity; //don't correct orientation when angular velocity is lower then that treshold

      gyroRateUnbiased = Input.gyro.rotationRateUnbiased;

      if (correctionOn && (gyroRateUnbiased.magnitude > gyroCorrectionMinSpeed))
      {
        rotation = Quaternion.RotateTowards(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)), Time.unscaledDeltaTime * magneticCorrectionRotationCoeff * deltaAngle);
      }
    }

    finalRotation = baseRotation * rotation;

    //gravity correction :
    Quaternion gravityDiff = Quaternion.FromToRotation(finalRotation * gyroGravity, Vector3.down);
    Vector3 gravityDiffXZ = new Vector3(gravityDiff.x, 0f, gravityDiff.z);
    Quaternion correction =  Quaternion.Euler(gravityDiffXZ);
    finalRotation = correction * finalRotation;
    
  }
}
