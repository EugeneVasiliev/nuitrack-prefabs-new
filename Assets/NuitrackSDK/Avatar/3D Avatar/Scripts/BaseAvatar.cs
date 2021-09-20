using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar
{
    public class JointTransform
    {
        nuitrack.Joint joint;

        public bool IsActive
        {
            get;
            private set;
        }

        public HumanBodyBones HumanBodyBone
        {
            get
            {
                return joint.Type.ToUnityBones();
            }
        }

        public Vector3 Position
        {
            get
            {
                return joint.ToVector3() * 0.001f;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return joint.ToQuaternion(); //Quaternion.Inverse(CalibrationInfo.SensorOrientation)
            }
        }

        public Quaternion RotationMirrored
        {
            get
            {
                return joint.ToQuaternionMirrored();
            }
        }

        public Vector2 Proj
        {
            get
            {
                return new Vector2(Mathf.Clamp01(joint.Proj.X), Mathf.Clamp01(joint.Proj.Y));
            }
        }

        public JointTransform(bool isActive, nuitrack.Joint joint)
        {
            this.joint = joint;
            IsActive = isActive;
        }
    }

    public abstract class BaseAvatar : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] 
        protected bool useCurrentUserTracker = true;
        
        [SerializeField]
        protected int skeletonID = 1;

        [SerializeField] 
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
                    skeletonID = value;

                    if(useCurrentUserTracker)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUserTracker = false;
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

        public bool IsActive
        {
            get
            {
                return ControllerSkeleton != null;
            }
        }

        public nuitrack.Skeleton ControllerSkeleton
        {
            get
            {
                if (useCurrentUserTracker)
                    return CurrentUserTracker.CurrentSkeleton;
                else
                    return NuitrackManager.SkeletonData != null ? NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID) : null;
            }
        }

        public ref List<ModelJoint> ModelJoints
        {
            get
            {
                return ref modelJoints;
            }
        }

        public JointTransform GetJointTransform(nuitrack.JointType jointType)
        {
            if (!IsActive)
                return null;

            nuitrack.Joint joint = ControllerSkeleton.GetJoint(jointType);
            JointTransform jointTransform = new JointTransform(joint.Confidence >= jointConfidence, joint);

            return jointTransform;
        }

        protected virtual void Update()
        {
            nuitrack.Skeleton skeleton = ControllerSkeleton;

            if (skeleton == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            if(skeleton != null)
                ProcessSkeleton(skeleton);
        }

        protected abstract void ProcessSkeleton(nuitrack.Skeleton skeleton);
    }
}