//using System.Linq;
using System.Collections.Generic;
using UnityEngine;


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

        //Dictionary<nuitrack.JointType, JointItem> jointItemsDict;

        public ref List<JointItem> JointItems
        {
            get
            {
                return ref jointItems;
            }
        }

        void Awake()
        {
            foreach (JointItem ji in jointItems)
                ji.RotationOffset = Quaternion.Inverse(space.rotation) * ji.boneTransform.rotation;

            //jointItemsDict = jointItems.ToDictionary(k => k.jointType);
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