using UnityEngine;


public class UserData
{
    public class SkeletonData
    {
        /// <summary>
        /// Wrapper for Nuitrack Joint <see cref="nuitrack.Joint"/>
        /// </summary>
        public class Joint
        {
            nuitrack.Joint joint;

            public Joint(nuitrack.Joint joint)
            {
                this.joint = joint;
            }

            public float Confidence
            {
                get
                {
                    return joint.Confidence;
                }
            }

            public nuitrack.JointType JointType
            {
                get
                {
                    return joint.Type;
                }
            }

            /// <summary>
            /// The corresponding type of Unity bone
            /// </summary>
            public HumanBodyBones HumanBodyBone
            {
                get
                {
                    return joint.Type.ToUnityBones();
                }
            }

            /// <summary>
            /// Joint position in meters
            /// </summary>
            public Vector3 Position
            {
                get
                {
                    return joint.ToVector3() * 0.001f;
                }
            }

            /// <summary>
            /// Joint orientation
            /// </summary>
            public Quaternion Rotation
            {
                get
                {
                    return joint.ToQuaternion(); //Quaternion.Inverse(CalibrationInfo.SensorOrientation)
                }
            }

            /// <summary>
            /// Mirrored orientation of the joint
            /// </summary>
            public Quaternion RotationMirrored
            {
                get
                {
                    return joint.ToQuaternionMirrored();
                }
            }

            /// <summary>
            /// Projection and normalized joint coordinates
            /// </summary>
            public Vector2 Proj
            {
                get
                {
                    return new Vector2(Mathf.Clamp01(joint.Proj.X), Mathf.Clamp01(joint.Proj.Y));
                }
            }
        }

        nuitrack.Skeleton sourceSkeleton;

        public SkeletonData(nuitrack.Skeleton skeleton)
        {
            sourceSkeleton = skeleton;
        }

        public Joint GetJoint(nuitrack.JointType jointType)
        {
            nuitrack.Joint joint = sourceSkeleton.GetJoint(jointType);
            return new Joint(joint);
        }

        //public Joint GetJoint(HumanBodyBones humanBodyBone)
        //{
        //    return GetJoint(humanBodyBone.ToNuitrackJoint());
        //}
    }

    public int ID
    {
        get;
        private set;
    }

    public SkeletonData Skeleton
    {
        get; private set;
    }

    public nuitrack.HandContent? LeftHand
    {
        get; private set;
    }

    public nuitrack.HandContent? RightHand
    {
        get; private set;
    }

    public nuitrack.GestureType? GestureType
    {
        get; private set;
    }

    public Face Face
    {
        get; private set;
    }

    public void SetSkeleton(nuitrack.Skeleton skeleton)
    {
        Skeleton = new SkeletonData(skeleton);
    }

    public void SetUserHands(nuitrack.UserHands userHands)
    {
        if (userHands != null)
        {
            LeftHand = userHands.LeftHand;
            RightHand = userHands.RightHand;
        }
    }

    public void SetFace(Face face)
    {
        Face = face;
    }

    public void SetGesture(nuitrack.GestureType gestureType)
    {
        GestureType = gestureType;
    }

    public UserData(int id)
    {
        ID = id;
    }
}