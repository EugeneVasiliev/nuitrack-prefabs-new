using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public class LocalAvatar : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] Transform space;

    ulong lastTimeStamp = 0;

    [Header("Rigged model")]
    [SerializeField] List<ModelJoint> modelJoints;

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

        Dictionary<nuitrack.JointType, ModelJoint>  jointsRigged = modelJoints.ToDictionary(k => k.jointType);

        foreach (ModelJoint modelJoint in modelJoints)
        {
            modelJoint.baseRotOffset = Quaternion.Inverse(SpaceRotation) * modelJoint.bone.rotation;

            //Adding base distances between the child bone and the parent bone 
            if (modelJoint.parentJointType != nuitrack.JointType.None)
            {
                modelJoint.parentBone = jointsRigged[modelJoint.parentJointType].bone;
                modelJoint.baseDistanceToParent = Vector3.Distance(modelJoint.bone.position, modelJoint.parentBone.position);
                modelJoint.parentBone.parent = transform;
            }
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
        foreach (ModelJoint modelJoint in modelJoints)
        {
            nuitrack.Joint joint = skeleton.GetJoint(modelJoint.jointType);

            //Bone position
            Vector3 bonePosition = SpaceToWorldPoint(joint.ToVector3() * 0.001f);
            modelJoint.bone.position = bonePosition;

            //Bone rotation
            Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * modelJoint.baseRotOffset;
            modelJoint.bone.rotation = SpaceRotation * jointOrient;

            //Bone scale
            //if (modelJoint.parentBone != null)
            //{
            //    //calculate how many times the distance between the child bone and its parent bone has changed compared to the base distance (which was recorded at the start)
            //    float scaleDif = modelJoint.baseDistanceToParent / Vector3.Distance(bonePosition, modelJoint.parentBone.position);
            //    //change the size of the bone to the resulting value (On default bone size (1,1,1))
            //    modelJoint.parentBone.localScale = Vector3.one / scaleDif;
            //}
        }
    }
}