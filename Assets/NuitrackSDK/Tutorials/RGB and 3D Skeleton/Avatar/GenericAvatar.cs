using UnityEngine;

using System.Linq;
using System.Collections.Generic;


namespace NuitrackAvatar
{
    public class GenericAvatar : Avatar
    {
        [System.Serializable]
        public class JointItem
        {
            public nuitrack.JointType jointType;
            public Transform boneTransform;

            public Quaternion RotationOffset { get; set; }
        }

        [SerializeField, HideInInspector] List<JointItem> jointItems;

        public ref List<JointItem> JointItems
        {
            get
            {
                return ref jointItems;
            }
        }

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

        List<JointItem> SortClamp(List<JointItem> sourceModelJoints)
        {
            List<JointItem> outList = new List<JointItem>();

            Dictionary<nuitrack.JointType, JointItem> dict = sourceModelJoints.ToDictionary(k => k.jointType, v => v);
            List<nuitrack.JointType> jointTypes = dict.Keys.ToList().SortClamp();

            foreach (nuitrack.JointType jointType in jointTypes)
                outList.Add(dict[jointType]);

            return outList;
        }

        void Awake()
        {
            jointItems = SortClamp(jointItems);

            //foreach (JointItem jointItem in jointItems)
            //{
            //    jointItem.jointType = jointItem.jointType.TryGetMirrored();
            //    jointItem.parentJointType = jointItem.parentJointType.TryGetMirrored();
            //}

            //Dictionary<nuitrack.JointType, JointItem> jointsRigged = jointItems.ToDictionary(k => k.jointType);

            //foreach (KeyValuePair<nuitrack.JointType, JointItem> joint in jointsRigged)
            //{
            //    JointItem jointItem = joint.Value;

            //    jointItem.RotationOffset = Quaternion.Inverse(SpaceRotation) * jointItem.boneTransform.rotation;

            //    if (jointItem.parentJointType != nuitrack.JointType.None)
            //        jointItem.parentBone = jointsRigged[jointItem.parentJointType].bone;
            //}
        }

        protected override void ProcessSkeleton(nuitrack.Skeleton skeleton)
        {
            if (skeleton == null)
                return;

            foreach (JointItem jointItem in jointItems)
            {
                nuitrack.Joint joint = skeleton.GetJoint(jointItem.jointType.TryGetMirrored());


                //Bone position
                Vector3 newPos = joint.ToVector3() * 0.001f;
                jointItem.boneTransform.position = space.TransformPoint(newPos);

                //Bone rotation
                Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion() * jointItem.RotationOffset;
                jointItem.boneTransform.rotation = space.rotation * jointOrient;
            }
        }
    }
}