using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HMDSkeletonAvatar : MonoBehaviour 
{
  /*
  [SerializeField]Transform headTransform;

	[SerializeField]float lerpFactor = 0.1f;
	[SerializeField]float maxJointSpeed = 5f;

	[SerializeField]bool limitMaxSpeed = false;

	nuitrack.JointType[] availableJoints;
	Dictionary<nuitrack.JointType, GameObject> joints;

  Dictionary<nuitrack.JointType, Quaternion> prevOrientations;
  Dictionary<nuitrack.JointType, Quaternion> baseRotationOffsets;

  [SerializeField]GameObject 
  basePivot, 
  torso,
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

	void Start ()
	{
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

    prevOrientations = new Dictionary<nuitrack.JointType, Quaternion>();

    for (int i = 0; i < availableJoints.Length; i++)
    {
      prevOrientations.Add(availableJoints[i], Quaternion.identity);
    }

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

    baseRotationOffsets = new Dictionary<nuitrack.JointType, Quaternion>();

    baseRotationOffsets.Add(nuitrack.JointType.Torso, torso.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.LeftCollar, collarLeft.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.RightCollar, collarRight.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.LeftShoulder, shoulderLeft.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.RightShoulder, shoulderRight.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.LeftElbow, elbowLeft.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.RightElbow, elbowRight.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.LeftHip, hipLeft.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.RightHip, hipRight.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.LeftKnee, kneeLeft.transform.rotation);
    baseRotationOffsets.Add(nuitrack.JointType.RightKnee, kneeRight.transform.rotation);
	}

	void FixedUpdate ()
	{
		JointsUpdate();
	}

	public Vector3 GetJointPosition(nuitrack.JointType joint)
	{
		return joints[joint].transform.position;
	}

	public Transform GetJointTransform(nuitrack.JointType joint)
	{
		return joints[joint].transform;
	}

  static Quaternion sensorOffset = Quaternion.Euler(0f, 0f, 0f);
  static Vector3 mirrorScale = new Vector3(-1f, 1f, -1f);

	void JointsUpdate()
	{
		if (NuitrackManager.CurrentUser != 0)
		{
      Vector3 torsoPos = 0.001f * (TPoseCalibration.SensorOrientation * Vector3.Scale(NuitrackManager.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso).ToVector3(), mirrorScale));
      Vector3 newTorsoPos = new Vector3(torsoPos.x, basePivot.transform.position.y, torsoPos.z);
      basePivot.transform.position = newTorsoPos;

      for (int i = 0; i < availableJoints.Length; i++)
			{
        nuitrack.Joint joint = NuitrackManager.CurrentSkeleton.GetJoint(availableJoints[i]);
        Quaternion jointOrient = TPoseCalibration.SensorOrientation * (joint.ToQuaternionMirrored() * sensorOffset);

        prevOrientations[availableJoints[i]] = Quaternion.Slerp(prevOrientations[availableJoints[i]], jointOrient, lerpFactor);
        joints[availableJoints[i]].transform.rotation = prevOrientations[availableJoints[i]] * baseRotationOffsets[availableJoints[i]];
			}
		}
	}
 */ 
}