using UnityEngine;


namespace NuitrackSDK.Avatar
{
    /// <summary>
    /// The base class of the avatar. Use it to create your own avatars.
    /// </summary>
    public abstract class BaseAvatar : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField, NuitrackSDKInspector] 
        bool useCurrentUserTracker = true;
        
        [SerializeField, NuitrackSDKInspector]
        int skeletonID = 1;

        [SerializeField, NuitrackSDKInspector] 
        float jointConfidence = 0.1f;

        /// <summary>
        /// If True, the current user tracker is used, otherwise the user specified by ID is used <see cref="SkeletonID"/>
        /// </summary>
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

        /// <summary>
        /// ID of the current user
        /// For the case when current user tracker <see cref="UseCurrentUserTracker"/> of is used, the ID of the active user will be returned
        /// If current user tracker is used and a new ID is set, tracking of the current user will stop
        /// </summary>
        public int SkeletonID
        {
            get
            {
                if(UseCurrentUserTracker)
                    return NuitrackManager.Users.Current != null ? NuitrackManager.Users.Current.ID : 0;
                else
                    return skeletonID;
            }
            set
            {
                if (value >= MinSkeletonID && value <= MaxSkeletonID)
                {
                    skeletonID = value;

                    if(useCurrentUserTracker)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUserTracker = false;
                }
                else
                    throw new System.Exception(string.Format("The Skeleton ID must be within the bounds of [{0}, {1}]", MinSkeletonID, MaxSkeletonID));
            }
        }

        /// <summary>
        /// Minimum allowed ID
        /// </summary>
        public int MinSkeletonID
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Maximum allowed ID
        /// </summary>
        public int MaxSkeletonID
        {
            get
            {
                return 6;
            }
        }

        /// <summary>
        /// Confidence threshold for detected joints
        /// </summary>
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

        /// <summary>
        /// True if there is a control skeleton.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return ControllerSkeleton != null;
            }
        }

        /// <summary>
        /// The controler skeleton. Maybe null
        /// </summary>
        public UserData.SkeletonData ControllerSkeleton
        {
            get
            {
                if (useCurrentUserTracker)
                    return NuitrackManager.Users.Current?.Skeleton;
                else
                    return NuitrackManager.Users.GetUser(skeletonID)?.Skeleton;
            }
        }

        /// <summary>
        /// Get a shell object for the specified joint
        /// </summary>
        /// <param name="jointType">Joint type</param>
        /// <returns>Shell object <see cref="JointTransform"/></returns>
        public UserData.SkeletonData.Joint GetJoint(nuitrack.JointType jointType)
        {
            if (!IsActive)
                return null;

            return ControllerSkeleton.GetJoint(jointType);
        }

        protected virtual void Update()
        {
            UserData.SkeletonData skeleton = ControllerSkeleton;

            if (skeleton == null)
                return;

            if(skeleton != null)
                ProcessSkeleton(skeleton);
        }

        /// <summary>
        /// Redefine this method to implement skeleton processing
        /// </summary>
        /// <param name="skeleton">Skeleton <see cref="nuitrack.Skeleton/>"/></param>
        protected abstract void ProcessSkeleton(UserData.SkeletonData skeleton);
    }
}