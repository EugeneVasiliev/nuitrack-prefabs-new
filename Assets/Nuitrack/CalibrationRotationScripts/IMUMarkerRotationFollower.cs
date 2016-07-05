using UnityEngine;
using System.Collections;

public class IMUMarkerRotationFollower : MonoBehaviour 
{
  IMUMarkerRotation imuRotation;

  void Start()
  {
    imuRotation = IMUMarkerRotation.Instance;
  }

  void LateUpdate () 
  {
    transform.localRotation = imuRotation.Rotation;
  }
}
