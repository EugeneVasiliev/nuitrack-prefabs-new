using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class RecenterOnCalibration_Gvr_OVR : MonoBehaviour 
{
  TPoseCalibration calibration;

	void Start () 
  {
    calibration = FindObjectOfType<TPoseCalibration>();
    if (calibration != null)
    {
      calibration.onSuccess += Calibration_onSuccess;
    }
	}

  void Calibration_onSuccess (Quaternion rotation)
  {
    //but really, DON'T USE REFLECTION
    //just use 

    //OVRManager.display.RecenterPose(); //or something like this if OVR plugin changed

    //for OVR plugin
    //and

    //GvrViewer sdk = GvrViewer.Instance;
    //if (sdk) 
    //{
    //  sdk.Recenter();
    //}  

    //for GoogleVR plugin

    Debug.Log("Trying to find OVR or GoogleVR plugins and invoke Recenter method. " +
      "If you are reading this then it's probably easier to look into " +
      "RecenterOnCalibration_Gvr_OVR script, add usual Recenter method for the used plugin " +
      "and remove reflection part from it");
    
    try //try to find and call OVR plugin Recenter
    {
      Type ovrManT = Type.GetType("OVRManager");

      object display = ovrManT.GetProperty("display").GetValue(null, null);
      Type dispT = Type.GetType("OVRDisplay");
      dispT.GetMethod("RecenterPose").Invoke(display, null);
      return;
    }
    catch (TypeLoadException ex)
    {
      Debug.Log("No OVR plugin found (may be just a different version of plugin), trying to find GoogleVR plugin next");
    }

    try //try to find and call GoogleVR plugin recenter
    {
      Type gvrViewerT = Type.GetType("GvrViewer");
      object gvrViewer = gvrViewerT.GetProperty("Instance").GetValue(null, null);
      gvrViewerT.GetMethod("Recenter").Invoke(gvrViewer, null);
    }
    catch (TypeLoadException ex)
    {
      Debug.Log("No GoogleVR plugin found (may be just a different version of plugin). No recentering will be done.");
    }
  }
}
