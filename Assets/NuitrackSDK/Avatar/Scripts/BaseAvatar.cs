﻿using UnityEngine;


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
        int userID = 1;

        [SerializeField, NuitrackSDKInspector] 
        float jointConfidence = 0.1f;

        /// <summary>
        /// If True, the current user tracker is used, otherwise the user specified by ID is used <see cref="UserID"/>
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
        public int UserID
        {
            get
            {
                if(UseCurrentUserTracker)
                    return NuitrackManager.Users.Current != null ? NuitrackManager.Users.Current.ID : 0;
                else
                    return userID;
            }
            set
            {
                if (value >= Users.MinID && value <= Users.MaxID)
                {
                    userID = value;

                    if(useCurrentUserTracker)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUserTracker = false;
                }
                else
                    throw new System.Exception(string.Format("The User ID must be within the bounds of [{0}, {1}]", Users.MinID, Users.MaxID));
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
                return ControllerUser != null;
            }
        }

        /// <summary>
        /// The controler user. Maybe null
        /// </summary>
        public UserData ControllerUser
        {
            get
            {
                if (useCurrentUserTracker)
                    return NuitrackManager.Users.Current;
                else
                    return NuitrackManager.Users.GetUser(userID);
            }
        }

        /// <summary>
        /// Get a shell object for the specified joint
        /// </summary>
        /// <param name="jointType">Joint type</param>
        /// <returns>Shell object <see cref="UserData.SkeletonData.Joint"/></returns>
        public UserData.SkeletonData.Joint GetJoint(nuitrack.JointType jointType)
        {
            if (!IsActive || ControllerUser.Skeleton == null)
                return null;

            return ControllerUser.Skeleton.GetJoint(jointType);
        }

        protected virtual void Update()
        {
            UserData userData = ControllerUser;

            if(userData != null)
                Process(userData);
        }

        /// <summary>
        /// Redefine this method to implement skeleton processing
        /// </summary>
        /// <param name="userData">User data <see cref="UserData/>"/></param>
        protected abstract void Process(UserData userData);
    }
}