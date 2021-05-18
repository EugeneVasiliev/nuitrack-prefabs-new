using UnityEngine;
using System.Collections.Generic;


namespace NuitrackAvatar
{
    public abstract class Avatar : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected List<nuitrack.JointType> jointTypes;

        [SerializeField, Range(1, 7)]
        protected int skeletonID = 1;

        [SerializeField]
        protected bool alignmentBoneLength;

        [SerializeField] protected Transform space;

        protected ulong lastTimeStamp = 0;

        public List<nuitrack.JointType> JointTypes
        {
            get
            {
                return jointTypes;
            }
        }

        void Update()
        {
            if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            nuitrack.Skeleton skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID);

            ProcessSkeleton(skeleton);
        }

        protected abstract void ProcessSkeleton(nuitrack.Skeleton skeleton);
    }
}