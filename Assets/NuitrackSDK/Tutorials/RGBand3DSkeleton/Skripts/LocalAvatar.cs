using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public class LocalAvatar : MonoBehaviour
{
    [Header ("Root")]
    [SerializeField] Transform root;
    [SerializeField] nuitrack.JointType rootJointType = nuitrack.JointType.Waist;

    [Header("Rigged model")]
    [SerializeField] List<ModelJoint> modelJoints;


    [Header("Space")]
    [Tooltip("(optional) Transform corresponding to the sensor coordinate system. If not specified, " +
       "the transformation is performed in the world coordinate system, where the sensor position = (0, 0, 0)")]
    [SerializeField] Transform sensorSpace;

    [Header("Options")]
    [Tooltip ("Aligns the positions of the model's joints with the joints of the user's skeleton.\n" +
        "It can cause model distortions and artifacts.")]
    [SerializeField] bool alignJointPosition = false;
    
    [Tooltip ("Aligns the size of the model's bones with the size of the bones of the user's skeleton, " +
        "ensuring that the model's size best matches the user's size.")]
    [SerializeField] bool alignBoneScale = true;

    ulong lastTimeStamp = 0;

    Quaternion SpaceRotation
    {
        get
        {
            return sensorSpace != null ? sensorSpace.rotation : Quaternion.identity;
        }
    }

    Vector3 SpaceToWorldPoint(Vector3 spacePoint)
    {
        return sensorSpace != null ? sensorSpace.TransformPoint(spacePoint) : spacePoint;
    }

    void Start()
    {
        modelJoints = SortClamp(modelJoints);

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

            if (modelJoint.parentJointType != nuitrack.JointType.None)
                modelJoint.parentBone = jointsRigged[modelJoint.parentJointType].bone;
        }
    }

    List<ModelJoint> SortClamp(List<ModelJoint> sourceModelJoints)
    {
        List<ModelJoint> outList = new List<ModelJoint>();

        Dictionary<nuitrack.JointType, ModelJoint> dict = sourceModelJoints.ToDictionary(k => k.jointType, v => v);
        List<nuitrack.JointType> jointTypes = dict.Keys.ToList().SortClamp();

        foreach(nuitrack.JointType jointType in jointTypes)
            outList.Add(dict[jointType]);

        return outList;
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
            
            if(alignJointPosition)
                modelJoint.bone.position = bonePosition;

            //Bone rotation
            Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * modelJoint.baseRotOffset;
            modelJoint.bone.rotation = SpaceRotation * jointOrient;

            //Bone scale
            if (alignBoneScale && modelJoint.parentBone != null)
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