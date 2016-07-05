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
  [SerializeField]bool useGyro = true;

	Quaternion baseRotation = Quaternion.identity;
	Quaternion rotation = Quaternion.identity;
	Quaternion finalRotation = Quaternion.identity;
	public Quaternion Rotation {get {return finalRotation;}}

  static IMUMarkerRotation instance;

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
    //TODO: gravity influence on sensor orientation


		gyroGravity = Input.gyro.gravity;
		gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);

    smoothedGravity = Vector3.Slerp(smoothedGravity, gyroGravity, slerpVectorsCoeff * Time.unscaledDeltaTime);

    if (useGyro)
    {
      Quaternion deltaRot = Quaternion.Euler(Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f)) * Mathf.Rad2Deg * Time.unscaledDeltaTime);
      rotation = rotation * deltaRot;
    }
	
    //gravity correction:
    /*
    Quaternion gravityDiff = Quaternion.FromToRotation(rotation * gyroGravity, Vector3.down);
    Vector3 eulerDiff = gravityDiff.eulerAngles;
    Vector3 gravityDiffXZ = new Vector3(eulerDiff.x, 0f, eulerDiff.z);
    Quaternion correction =  Quaternion.Euler(gravityDiffXZ);
    rotation = Quaternion.Slerp(rotation, correction * rotation, slerpRotationsCoeff * Time.unscaledDeltaTime);
    */

    if (MarkerData.haveData[CurrentUserTracker.CurrentUser])
    {
      rotation = MarkerData.markerRotations[CurrentUserTracker.CurrentUser];
    }
    //

    finalRotation = baseRotation * rotation;
  }
}
