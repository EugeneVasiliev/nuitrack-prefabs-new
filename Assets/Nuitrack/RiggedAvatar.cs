using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiggedAvatar : MonoBehaviour
{
    [SerializeField] Transform headTransform;
    //[SerializeField] Transform headDirectionTransform;//part of head preab that rotates 
    [SerializeField] Transform headPivot;
    [SerializeField] bool rotate180 = true;
    Vector3 basePivotOffset;
    TPoseCalibration tPC;

    Quaternion q180 = Quaternion.Euler(0f, 180f, 0f);
    Quaternion q0 = Quaternion.identity;

    nuitrack.JointType[] availableJoints;
    Dictionary<nuitrack.JointType, GameObject> joints;

    Dictionary<nuitrack.JointType, Quaternion> baseRotationOffsets;

    Quaternion neckRotationOffset;
    [SerializeField] Vector3 startPoint;

    [SerializeField]
    GameObject
    basePivot,
    torso,
    neck,
    hipLeft,
    hipRight,
    kneeLeft,
    kneeRight,
    shoulderLeft,
    shoulderRight,
    elbowLeft,
    elbowRight,
    collarLeft,
    collarRight;

    void Awake()
    {
        tPC = FindObjectOfType<TPoseCalibration>();
    }

    void OnEnable()
    {
        tPC.onSuccess += OnSuccessCalib;
    }

    void OnDisable()
    {
        tPC.onSuccess -= OnSuccessCalib;
    }

    private void OnSuccessCalib(Quaternion rotation)
    {
        StartCoroutine(CalculateOffset());
    }

    public IEnumerator CalculateOffset()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        basePivotOffset = startPoint - GetPivotPos() + basePivotOffset;
        basePivotOffset.x = 0;
    }

    void Start()
    {
        //
        availableJoints = new nuitrack.JointType[]
        {
            nuitrack.JointType.Torso,

            nuitrack.JointType.LeftCollar,
            nuitrack.JointType.RightCollar,
            nuitrack.JointType.LeftShoulder,
            nuitrack.JointType.RightShoulder,
            nuitrack.JointType.LeftElbow,
            nuitrack.JointType.RightElbow,
            //nuitrack.JointType.LeftWrist,
            //nuitrack.JointType.RightWrist,

            nuitrack.JointType.LeftHip,
            nuitrack.JointType.RightHip,
            nuitrack.JointType.LeftKnee,
            nuitrack.JointType.RightKnee,
                //nuitrack.JointType.LeftAnkle,
                //nuitrack.JointType.RightAnkle
        };

        //Добавление костей модели и JointType ключей
        joints = new Dictionary<nuitrack.JointType, GameObject>();

        joints.Add(nuitrack.JointType.Torso, torso);
        joints.Add(nuitrack.JointType.LeftCollar, collarLeft);
        joints.Add(nuitrack.JointType.RightCollar, collarRight);
        joints.Add(nuitrack.JointType.LeftShoulder, shoulderLeft);
        joints.Add(nuitrack.JointType.RightShoulder, shoulderRight);
        joints.Add(nuitrack.JointType.LeftElbow, elbowLeft);
        joints.Add(nuitrack.JointType.RightElbow, elbowRight);
        joints.Add(nuitrack.JointType.LeftHip, hipLeft);
        joints.Add(nuitrack.JointType.RightHip, hipRight);
        joints.Add(nuitrack.JointType.LeftKnee, kneeLeft);
        joints.Add(nuitrack.JointType.RightKnee, kneeRight);

        //Добавление сдвигов поворотов костей модели и JointType ключей
        baseRotationOffsets = new Dictionary<nuitrack.JointType, Quaternion>();

        Quaternion basePivotInverse = Quaternion.Inverse(basePivot.transform.rotation);

        baseRotationOffsets.Add(nuitrack.JointType.Torso, basePivotInverse * torso.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.LeftCollar, basePivotInverse * collarLeft.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.RightCollar, basePivotInverse * collarRight.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.LeftShoulder, basePivotInverse * shoulderLeft.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.RightShoulder, basePivotInverse * shoulderRight.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.LeftElbow, basePivotInverse * elbowLeft.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.RightElbow, basePivotInverse * elbowRight.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.LeftHip, basePivotInverse * hipLeft.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.RightHip, basePivotInverse * hipRight.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.LeftKnee, basePivotInverse * kneeLeft.transform.rotation);
        baseRotationOffsets.Add(nuitrack.JointType.RightKnee, basePivotInverse * kneeRight.transform.rotation);

        neckRotationOffset = basePivotInverse * neck.transform.rotation;
    }

    void Update()
    {
        JointsUpdate();
        if ((neck != null)/* && (headDirectionTransform != null) */ && (headPivot != null)) NeckUpdate();
    }

    public Vector3 GetJointPosition(nuitrack.JointType joint)
    {
        return joints[joint].transform.position;
    }

    public Transform GetJointTransform(nuitrack.JointType joint)
    {
        return joints[joint].transform;
    }

    bool firstOffset = false;

    void JointsUpdate()
    {
        if (CurrentUserTracker.CurrentUser != 0)
        {
            if (!firstOffset)
            {
                firstOffset = true;
                StartCoroutine(CalculateOffset());
            }

            Quaternion inverseSensorOrientation = Quaternion.Inverse(CalibrationInfo.SensorOrientation);

            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

            Vector3 torsoPos = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation *
                (0.001f * skeleton.GetJoint(nuitrack.JointType.Torso).ToVector3()));
            basePivot.transform.position = torsoPos + basePivotOffset;

            //for (int i = 0; i < availableJoints.Length; i++)
            //{
            //    nuitrack.Joint joint = skeleton.GetJoint(availableJoints[i]);
            //    Quaternion jointOrient = CalibrationInfo.SensorOrientation * (joint.ToQuaternionMirrored());

            //    joints[availableJoints[i]].transform.rotation = (rotate180 ? q0 : q180) * inverseSensorOrientation * jointOrient * basePivot.transform.rotation * baseRotationOffsets[availableJoints[i]];
            //}

            foreach (var modelJoint in joints)
            {
                nuitrack.Joint joint = skeleton.GetJoint(modelJoint.Key);
                Quaternion jointOrient = CalibrationInfo.SensorOrientation * (joint.ToQuaternionMirrored());

                modelJoint.Value.transform.rotation = (rotate180 ? q0 : q180) * inverseSensorOrientation * jointOrient * basePivot.transform.rotation * baseRotationOffsets[modelJoint.Key];
            }
        }
    }

    public Vector3 GetPivotPos()
    {
        return basePivot.transform.position;
    }

    void NeckUpdate()
    {
        Quaternion inverseSensorOrientation = Quaternion.Inverse(CalibrationInfo.SensorOrientation);

        neck.transform.rotation = (rotate180 ? q0 : q180) * /*inverseSensorOrientation **//* headDirectionTransform.localRotation * */ basePivot.transform.rotation * neckRotationOffset;
        if (headTransform != null) headTransform.position = headPivot.position;
    }
}