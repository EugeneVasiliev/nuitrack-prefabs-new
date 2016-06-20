using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkeletonAvatar : MonoBehaviour 
{
  [SerializeField]GameObject jointPrefab, connectionPrefab;
  [SerializeField]Transform headtransform; //if not null, skeletonAvatar will move it
  [SerializeField]bool rotate180 = false;

  nuitrack.JointType[] jointsInfo = new nuitrack.JointType[]
  {
    nuitrack.JointType.Head,
    nuitrack.JointType.Neck,
    nuitrack.JointType.LeftCollar,
    nuitrack.JointType.Torso,
    nuitrack.JointType.Waist,
    nuitrack.JointType.LeftShoulder,
    nuitrack.JointType.RightShoulder,
    nuitrack.JointType.LeftElbow,
    nuitrack.JointType.RightElbow,
    nuitrack.JointType.LeftWrist,
    nuitrack.JointType.RightWrist,
    nuitrack.JointType.LeftHand,
    nuitrack.JointType.RightHand,
    nuitrack.JointType.LeftHip,
    nuitrack.JointType.RightHip,
    nuitrack.JointType.LeftKnee,
    nuitrack.JointType.RightKnee,
    nuitrack.JointType.LeftAnkle,
    nuitrack.JointType.RightAnkle
  };

  nuitrack.JointType[,] connectionsInfo = new nuitrack.JointType[,]
  {
    {nuitrack.JointType.Neck,           nuitrack.JointType.Head},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Neck},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.LeftShoulder},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.RightShoulder},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Torso},
    {nuitrack.JointType.Waist,          nuitrack.JointType.Torso},
    {nuitrack.JointType.Waist,          nuitrack.JointType.LeftHip},
    {nuitrack.JointType.Waist,          nuitrack.JointType.RightHip},
    {nuitrack.JointType.LeftShoulder,   nuitrack.JointType.LeftElbow},
    {nuitrack.JointType.LeftElbow,      nuitrack.JointType.LeftWrist},
    {nuitrack.JointType.LeftWrist,      nuitrack.JointType.LeftHand},
    {nuitrack.JointType.RightShoulder,  nuitrack.JointType.RightElbow},
    {nuitrack.JointType.RightElbow,     nuitrack.JointType.RightWrist},
    {nuitrack.JointType.RightWrist,     nuitrack.JointType.RightHand},
    {nuitrack.JointType.LeftHip,        nuitrack.JointType.LeftKnee},
    {nuitrack.JointType.LeftKnee,       nuitrack.JointType.LeftAnkle},
    {nuitrack.JointType.RightHip,       nuitrack.JointType.RightKnee},
    {nuitrack.JointType.RightKnee,      nuitrack.JointType.RightAnkle}
  };

  GameObject skeletonRoot;
  GameObject[] connections;
  Dictionary<nuitrack.JointType, GameObject> joints;
  Quaternion q180 = Quaternion.Euler(0f, 180f, 0f);
  Quaternion q0 = Quaternion.identity;

  void Start () 
  {
    CreateSkeletonParts();
  }

  void CreateSkeletonParts()
  {
    skeletonRoot = new GameObject();
    skeletonRoot.name = "SkeletonRoot";

    joints = new Dictionary<nuitrack.JointType, GameObject>();


    for (int i = 0; i < jointsInfo.Length; i++)
    {
      GameObject joint = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
      joints.Add(jointsInfo[i], joint);
      joint.transform.parent = skeletonRoot.transform;
      joint.SetActive(false);
    }

    connections = new GameObject[connectionsInfo.GetLength(0)];

    for (int i = 0; i < connections.Length; i++)
    {
      GameObject conn = (GameObject)Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
      connections[i] = conn;
      conn.transform.parent = skeletonRoot.transform;
      conn.SetActive(false);
    }
  }

  void DeleteSkeletonParts()
  {
    Destroy(skeletonRoot);
    joints = null;
    connections = null;
  }

  void Update()
  {
    if (CurrentUserTracker.CurrentSkeleton != null) ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
  }

  void ProcessSkeleton(nuitrack.Skeleton skeleton)
  {
    if (skeleton == null) return;

    if (headtransform != null)
    {
      headtransform.position = 0.001f * ((rotate180 ? q180 : q0) * (CalibrationInfo.SensorOrientation * skeleton.GetJoint(nuitrack.JointType.Head).ToVector3()));
    }

    if (!skeletonRoot.activeSelf) skeletonRoot.SetActive(true);

    for (int i = 0; i < jointsInfo.Length; i++)
    {
      nuitrack.Joint j = skeleton.GetJoint(jointsInfo[i]);
      if (j.Confidence > 0.5f)
      {
        if (!joints[jointsInfo[i]].activeSelf) joints[jointsInfo[i]].SetActive(true);

        joints[jointsInfo[i]].transform.position = 0.001f * ((rotate180 ? q180 : q0) * (CalibrationInfo.SensorOrientation * j.ToVector3()));

        //joints[i].Orient.Matrix:
        // 0,       1,      2, 
        // 3,       4,      5,
        // 6,       7,      8
        // -------
        // right(X),  up(Y),    forward(Z)

        //Vector3 jointRight =  new Vector3(  j.Orient.Matrix[0],  j.Orient.Matrix[3],  j.Orient.Matrix[6] );
        Vector3 jointUp =       new Vector3(   j.Orient.Matrix[1],  j.Orient.Matrix[4],  j.Orient.Matrix[7] );
        Vector3 jointForward =  new Vector3(  j.Orient.Matrix[2],  j.Orient.Matrix[5],  j.Orient.Matrix[8] );
        joints[jointsInfo[i]].transform.rotation = (rotate180 ? q180 : q0) * CalibrationInfo.SensorOrientation * Quaternion.LookRotation(jointForward, jointUp);
      }
      else
      {
        if (joints[jointsInfo[i]].activeSelf) joints[jointsInfo[i]].SetActive(false);
      }
    }

    for (int i = 0; i < connectionsInfo.GetLength(0); i++)
    {
      if (joints[connectionsInfo[i, 0]].activeSelf && joints[connectionsInfo[i, 1]].activeSelf)
      {
        if (!connections[i].activeSelf) connections[i].SetActive(true);

        Vector3 diff = joints[connectionsInfo[i, 1]].transform.position - joints[connectionsInfo[i, 0]].transform.position;

        connections[i].transform.position = joints[connectionsInfo[i, 0]].transform.position;
        connections[i].transform.rotation = Quaternion.LookRotation(diff);
        connections[i].transform.localScale = new Vector3(1f, 1f, diff.magnitude);
      }
      else
      {
        if (connections[i].activeSelf) connections[i].SetActive(false);
      }
    }
  }

  void OnDestroy()
  {
    DeleteSkeletonParts();
  }
}