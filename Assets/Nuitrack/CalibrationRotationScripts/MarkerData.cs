using UnityEngine;
using System.Collections;
public class MarkerData : MonoBehaviour 
{
  #if NUITRACK_MARKER

  public static Quaternion[] markerRotations;
  public static bool[] haveData;

  nuitrack.NativeImporterMarker.MarkerCallback callback;

  void Start () 
  {
    markerRotations = new Quaternion[6];
    haveData = new bool[6];
    callback = Callback;
    nuitrack.NativeImporterMarker.nuitrack_OnMarkerUpdate(callback);
	}
	
  void Callback(float[] data, int len)
  {
    for (int userId = 1; userId < (len / 9) + 1; userId++)
    {
      Vector3 mRight =    new Vector3( data[(userId - 1) * 9 + 0],  data[(userId - 1) * 9 + 3],  data[(userId - 1) * 9 + 6]);
      Vector3 mUp =       new Vector3( data[(userId - 1) * 9 + 1],  data[(userId - 1) * 9 + 4],  data[(userId - 1) * 9 + 7]);
      Vector3 mForward =  new Vector3( data[(userId - 1) * 9 + 2],  data[(userId - 1) * 9 + 5],  data[(userId - 1) * 9 + 8]);
      haveData[userId] = (mRight.sqrMagnitude > 0.01f);
      if (haveData[userId])
      {
        markerRotations[userId] = Quaternion.LookRotation(mForward, mUp);
      }
    }
  }

  #endif
}
