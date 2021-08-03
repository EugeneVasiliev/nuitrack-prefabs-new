﻿using UnityEngine;

public class CameraRecenter : MonoBehaviour
{
    [SerializeField] Transform headBasisTransform;
    [SerializeField] Transform yAxisCorrectionTransform;
    [SerializeField] Transform gazeDirectionTransform;

    TPoseCalibration poseCalibration;

    static Quaternion correctionQ = Quaternion.identity;

    private void Start()
    {
        yAxisCorrectionTransform.localRotation = correctionQ;
        if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            Debug.Log("Note: For IOS and Android you can use latest Google Cardboard: https://developers.google.com/cardboard/develop/unity/quickstart");
    }

    void Recenter(Quaternion rotation)
    {
        Vector3 gazeDirection = gazeDirectionTransform.forward;

        Vector3 gazeDirHead = headBasisTransform.InverseTransformVector(gazeDirection);
        Quaternion currentRotation = Quaternion.Euler(0f, Mathf.Atan2(gazeDirHead.x, gazeDirHead.z) * Mathf.Rad2Deg, 0f);

        Vector3 handsDirection = rotation * Vector3.forward;
        Quaternion yPartRotation = Quaternion.Euler(0f, Mathf.Atan2(handsDirection.x, handsDirection.z) * Mathf.Rad2Deg, 0f);

        Quaternion correction = yPartRotation * Quaternion.Inverse(currentRotation);
        yAxisCorrectionTransform.localRotation = yAxisCorrectionTransform.localRotation * correction;
        yAxisCorrectionTransform.localEulerAngles += new Vector3(0, 180, 0);

        correctionQ = yAxisCorrectionTransform.localRotation;
    }

    private void OnEnable()
    {
        poseCalibration = FindObjectOfType<TPoseCalibration>();
        poseCalibration.onSuccess += Recenter;
    }

    void OnDisable()
    {
        poseCalibration.onSuccess -= Recenter;
    }
}
