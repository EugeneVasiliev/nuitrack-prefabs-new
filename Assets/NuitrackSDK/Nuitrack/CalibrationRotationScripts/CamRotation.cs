using UnityEngine;

public class CamRotation : MonoBehaviour
{
    TPoseCalibration poseCalibration;

    void NativeRecenter(Quaternion rot)
    {
        UnityEngine.XR.InputTracking.Recenter();
    }

    private void OnEnable()
    {
        poseCalibration = FindObjectOfType<TPoseCalibration>();
        poseCalibration.onSuccess += NativeRecenter;
    }

    void OnDisable()
    {
        poseCalibration.onSuccess -= NativeRecenter;
    }
}
