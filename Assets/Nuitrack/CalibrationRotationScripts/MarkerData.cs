using UnityEngine;
using System.Collections;

public class MarkerData : MonoBehaviour 
{
  public static Quaternion[] markerRotations;
  public static bool[] haveData;

  nuitrack.NativeImporterMarker.MarkerCallback callback;

  void Start () 
  {

    //callback = Callback; //this should work too, imo
    markerRotations = new Quaternion[6];
    haveData = new bool[6];
    callback = Callback;
    nuitrack.NativeImporterMarker.nuitrack_OnMarkerUpdate(callback); //that line makes it crash ("stripping assemblies" option enabled makes it crash, to be a bit more precise)
    //yield return new WaitForSeconds(10f);
    //
    //F/mono    (32335): * Assertion at class.c:4293, condition `class' not met
    //F/libc    (32335): Fatal signal 11 (SIGSEGV) at 0x00007e4f (code=0), thread 32376 (UnityMain)
	}
	
  void Callback(float[] data, int len)
  {
    //Debug.Log("data.Length = " + data.Length.ToString() +"; len: " + len.ToString());
    //Debug.Log("Marker callback in Unity."); //eh... we do not get here at all :(
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
}
