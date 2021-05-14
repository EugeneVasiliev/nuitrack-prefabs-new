using UnityEngine;
using System.Collections.Generic;

public class LocalAvatar : MonoBehaviour
{
    [Header("Main")]
    [SerializeField, Range(1, 6)] int skeletonID = 1;
    [SerializeField] Transform space;

    [Header("Rigged model")]
    [SerializeField] List<ModelJoint> modelJoints;

    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();

    ulong lastTimeStamp = 0;

    void Start()
    {
        for (int i = 0; i < modelJoints.Count; i++)
        {
            modelJoints[i].baseRotOffset = Quaternion.Inverse(space.rotation) * modelJoints[i].bone.rotation;
            jointsRigged.Add(modelJoints[i].jointType.TryGetMirrored(), modelJoints[i]);
        }
    }

    void Update()
    {
        if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
            return;

        lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;

        nuitrack.Skeleton skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID);

        if (skeleton == null)
            return;

        foreach (var riggedJoint in jointsRigged)
        {
            nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);
            ModelJoint modelJoint = riggedJoint.Value;

            //Bone position
            Vector3 newPos = joint.ToVector3() * 0.001f;
            modelJoint.bone.position = space.TransformPoint(newPos);

            //Bone rotation
            Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * modelJoint.baseRotOffset;
            modelJoint.bone.rotation = space.rotation * jointOrient;
        }
    }
}
