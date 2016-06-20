using UnityEngine;
using System.Collections;

public class IMURotationFollower : MonoBehaviour 
{
  IMURotation imuRotation;

  void Start()
  {
    imuRotation = IMURotation.Instance;
  }

	void LateUpdate () 
  {
    transform.localRotation = imuRotation.Rotation;
	}
}
