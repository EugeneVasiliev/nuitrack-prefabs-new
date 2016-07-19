using UnityEngine;
using System.Collections;

public class IMUMarkerRotationFollower : MonoBehaviour 
{
  #if NUITRACK_MARKER
  IMUMarkerRotation imuRotation;

  void Start()
  {
    imuRotation = IMUMarkerRotation.Instance;
  }

  void LateUpdate () 
  {
    transform.localRotation = imuRotation.Rotation;
  }
  #endif
}
