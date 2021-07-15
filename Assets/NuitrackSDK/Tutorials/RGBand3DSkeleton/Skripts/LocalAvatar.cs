using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public class LocalAvatar : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] Transform space;

    ulong lastTimeStamp = 0;

    [Header ("Skeleton")]
    [SerializeField] Transform root;
    [SerializeField] nuitrack.JointType rootJointType = nuitrack.JointType.Waist;

    [Header("Rigged model")]
    [SerializeField] List<ModelJoint> modelJoints;

    [Header("Options")]
    [SerializeField] bool alignModelToSkeleton = false;

    Quaternion SpaceRotation
    {
        get
        {
            return space != null ? space.rotation : Quaternion.identity;
        }
    }

    Vector3 SpaceToWorldPoint(Vector3 spacePoint)
    {
        return space != null ? space.TransformPoint(spacePoint) : spacePoint;
    }

    void Start()
    {
        foreach (ModelJoint modelJoint in modelJoints)
        {
            modelJoint.jointType = modelJoint.jointType.TryGetMirrored();
            modelJoint.parentJointType = modelJoint.parentJointType.TryGetMirrored();
        }

        Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = modelJoints.ToDictionary(k => k.jointType);

        foreach (KeyValuePair<nuitrack.JointType, ModelJoint> joint in jointsRigged)
        {
            nuitrack.JointType jointType = joint.Key;
            ModelJoint modelJoint = joint.Value;

            modelJoint.baseRotOffset = Quaternion.Inverse(SpaceRotation) * modelJoint.bone.rotation;

            //Adding base distances between the child bone and the parent bone 
            if (modelJoint.parentJointType != nuitrack.JointType.None)
                modelJoint.parentBone = jointsRigged[modelJoint.parentJointType].bone;
        }
    }

    void Update()
    {
        if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
            return;

        lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;

        if (CurrentUserTracker.CurrentSkeleton != null)
            ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
    }

    /// <summary>
    /// Getting skeleton data from thr sensor and updating transforms of the model bones
    /// </summary>
    void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        nuitrack.Joint rootJ = skeleton.GetJoint(rootJointType);
        root.position = SpaceToWorldPoint(rootJ.ToVector3() * 0.001f);

        foreach (ModelJoint modelJoint in modelJoints) // add sort
        {
            nuitrack.Joint joint = skeleton.GetJoint(modelJoint.jointType);

            //Bone position
            Vector3 bonePosition = SpaceToWorldPoint(joint.ToVector3() * 0.001f);
            
            if(alignModelToSkeleton)
                modelJoint.bone.position = bonePosition;

            //Bone rotation
            Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * modelJoint.baseRotOffset;
            modelJoint.bone.rotation = SpaceRotation * jointOrient;

            //Bone scale
            if (modelJoint.parentBone != null)
            {
                nuitrack.Joint parentJoint = skeleton.GetJoint(modelJoint.parentJointType);
                Vector3 parentJointPosition = SpaceToWorldPoint(parentJoint.ToVector3() * 0.001f);

                float skeletonJointDistance = Vector3.Distance(parentJointPosition, bonePosition);
                float modelJointDistance = Vector3.Distance(modelJoint.bone.position, modelJoint.parentBone.position);
                float scaleK = skeletonJointDistance / modelJointDistance;

                modelJoint.parentBone.localScale *= scaleK;
            }
        }
    }
}