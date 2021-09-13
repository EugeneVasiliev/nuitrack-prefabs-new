using UnityEngine;

using System.Linq;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar
{
    public class GenericAvatar : BaseAvatar
    {
        [Header("Root")]
        [SerializeField] nuitrack.JointType rootJointType = nuitrack.JointType.Waist;

        [Header("Space")]
        [Tooltip("(optional) Transform corresponding to the sensor coordinate system. If not specified, " +
            "the transformation is performed in the world coordinate system, where the sensor position = (0, 0, 0)")]
        [SerializeField] protected Transform space;

        [Tooltip("Aligns the size of the model's bones with the size of the bones of the user's skeleton, " +
            "ensuring that the model's size best matches the user's size.")]
        [SerializeField] bool alignmentBoneLength = true;

        [Tooltip("Aligns the positions of the model's joints with the joints of the user's skeleton.\n" +
           "It can cause model distortions and artifacts.")]
        [SerializeField] bool alignJointPosition = false;

        Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = null;

        Quaternion SpaceRotation
        {
            get
            {
                return space == null ? Quaternion.identity : space.rotation;
            }
        }

        Vector3 SpaceToWorldPoint(Vector3 spacePoint)
        {
            return space == null ? spacePoint : space.TransformPoint(spacePoint);
        }

        nuitrack.JointType GetRecursiveParent(nuitrack.JointType startJoint, List<nuitrack.JointType> jointsDict)
        {
            nuitrack.JointType parent = startJoint.GetParent();

            if (parent == nuitrack.JointType.None || jointsDict.Contains(parent))
                return parent;
            else
                return GetRecursiveParent(parent, jointsDict);
        }

        void Awake()
        {
            foreach (ModelJoint modelJoint in modelJoints)
            {
                modelJoint.jointType = modelJoint.jointType.TryGetMirrored();
                modelJoint.parentJointType = modelJoint.jointType.GetParent();
            }

            jointsRigged = modelJoints.ToDictionary(k => k.jointType);

            foreach (KeyValuePair<nuitrack.JointType, ModelJoint> joint in jointsRigged)
            {
                ModelJoint modelJoint = joint.Value;

                if (modelJoint.bone != null)
                {
                    modelJoint.baseRotOffset = Quaternion.Inverse(SpaceRotation) * modelJoint.bone.rotation;

                    if (alignmentBoneLength && modelJoint.parentJointType != nuitrack.JointType.None && jointsRigged.ContainsKey(modelJoint.parentJointType))
                        modelJoint.parentBone = jointsRigged[modelJoint.parentJointType].bone;
                }
            }
        }

        protected override void ProcessSkeleton(nuitrack.Skeleton skeleton)
        {
            if (skeleton == null)
                return;

            foreach (ModelJoint modelJoint in modelJoints)
            {
                JointTransform jointTransform = GetJointTransform(modelJoint.jointType);

                if (!jointTransform.IsActive)
                    continue;

                //Bone rotation
                Quaternion jointOrient = jointTransform.Rotation * modelJoint.baseRotOffset;
                modelJoint.bone.rotation = SpaceRotation * jointOrient;

                //Bone position
                Vector3 jointPosition = SpaceToWorldPoint(jointTransform.Position);

                if (alignJointPosition || modelJoint.jointType == rootJointType)
                    modelJoint.bone.position = jointPosition;

                //Bone scale
                if (alignmentBoneLength && modelJoint.parentBone != null)
                {
                    nuitrack.Joint parentJoint = skeleton.GetJoint(modelJoint.parentJointType);
                    Vector3 parentJointPosition = SpaceToWorldPoint(parentJoint.ToVector3() * 0.001f);

                    float skeletonJointDistance = Vector3.Distance(parentJointPosition, jointPosition);
                    float modelJointDistance = Vector3.Distance(modelJoint.bone.position, modelJoint.parentBone.position);
                    float scaleK = skeletonJointDistance / modelJointDistance;

                    modelJoint.parentBone.localScale *= scaleK;
                }
            }
        }
    }
}