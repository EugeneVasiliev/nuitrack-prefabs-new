using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar
{
    public abstract class BaseAvatar : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField, HideInInspector] 
        protected bool useCurrentUserTracker = true;
        
        [SerializeField, HideInInspector]
        protected int skeletonID = 1;

        [SerializeField, HideInInspector] 
        protected float jointConfidence = 0.1f;

        [SerializeField] 
        protected List<ModelJoint> modelJoints;

        protected ulong lastTimeStamp = 0;

        public bool UseCurrentUserTracker
        {
            get
            {
                return useCurrentUserTracker;
            }
            set
            {
                useCurrentUserTracker = value;
            }
        }

        public int SkeletonID
        {
            get
            {
                return skeletonID;
            }
            set
            {
                if (value >= MinSkeletonID && value <= MaxSkeletonID)
                {
                    useCurrentUserTracker = false;
                    skeletonID = value;
                }
                else
                    throw new System.Exception(string.Format("The Skeleton ID must be within the bounds of [{0}, {1}]", MinSkeletonID, MaxSkeletonID));
            }
        }

        public int MinSkeletonID
        {
            get
            {
                return 1;
            }
        }

        public int MaxSkeletonID
        {
            get
            {
                return 6;
            }
        }

        public float JointConfidence
        {
            get
            {
                return jointConfidence;
            }
            set
            {
                jointConfidence = value;
            }
        }

        public ref List<ModelJoint> ModelJoints
        {
            get
            {
                return ref modelJoints;
            }
        }

        protected virtual void Update()
        {
            if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            nuitrack.Skeleton skeleton = useCurrentUserTracker ? CurrentUserTracker.CurrentSkeleton : NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID);

            if(skeleton != null)
                ProcessSkeleton(skeleton);
        }

        protected abstract void ProcessSkeleton(nuitrack.Skeleton skeleton);
    }
}