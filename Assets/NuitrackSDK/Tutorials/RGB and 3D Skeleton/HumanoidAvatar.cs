using UnityEngine;

using System;
using System.Collections.Generic;

namespace NuitrackAvatar
{
    [RequireComponent (typeof(Animator))]
    public class HumanoidAvatar : Avatar
    {
        Animator animator;

        Dictionary<nuitrack.JointType, HumanBodyBones> boneMapping;
        List<ModelJoint> joints;

        Quaternion SpaceRotation
        {
            get
            {
                if (space != null)
                    return Quaternion.identity;
                else
                    return space.rotation;
            }
        }

        void Awake()
        {
            animator = GetComponent<Animator>();

            boneMapping = new Dictionary<nuitrack.JointType, HumanBodyBones>();
            joints = new List<ModelJoint>();

            foreach (nuitrack.JointType jointType in jointTypes)
            {
                HumanBodyBones unityBone = jointType.ToUnityBones();

                if (unityBone == HumanBodyBones.LastBone)
                    continue;

                Transform bone = animator.GetBoneTransform(unityBone);
                Transform parentBone = null;

                if (bone == null)
                {
                    Debug.LogError(string.Format("No match found for the bone: {0}. Check the avatar mapping in the import settings.", jointType.ToString()));
                    continue;
                }

                nuitrack.JointType parentJoint = jointType.GetParent();
                HumanBodyBones unityParentBone = parentJoint.ToUnityBones();

                if (parentJoint != nuitrack.JointType.None && unityParentBone != HumanBodyBones.LastBone)
                    parentBone = animator.GetBoneTransform(unityParentBone);

                boneMapping.Add(jointType, unityBone);

                ModelJoint modelJoint = new ModelJoint()
                {
                    jointType = jointType,
                    parentJointType = jointType.GetParent(),
                    bone = bone,
                    parentBone = parentBone,
                    baseRotOffset = Quaternion.Inverse(SpaceRotation) * bone.rotation
                };

                joints.Add(modelJoint);

                Debug.Log(string.Format("{0} = {1}", jointType.ToString(), bone.name));
            }
        }

        protected override void ProcessSkeleton(nuitrack.Skeleton skeleton)
        {
            if (skeleton == null)
                return;

            foreach (ModelJoint modelJoint in joints)
            {
                nuitrack.Joint joint = skeleton.GetJoint(modelJoint.jointType.TryGetMirrored());

                //Bone position
                Vector3 newPos = joint.ToVector3() * 0.001f;
                modelJoint.bone.position = space.TransformPoint(newPos);

                //Bone rotation
                Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * modelJoint.baseRotOffset;
                modelJoint.bone.rotation = space.rotation * jointOrient;
            }
        }
    }
}
