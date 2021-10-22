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
            public nuitrack.Joint RawJoint
            {
                get; private set;
            }

            public Joint(nuitrack.Joint joint)
            {
                RawJoint = joint;
            }

            public float Confidence
            {
                get
                {
                    return RawJoint.Confidence;
                }
            }

            public nuitrack.JointType NuitrackType
            {
                get
                {
                    return RawJoint.Type;
                }
            }

            /// <summary>
            /// The corresponding type of Unity bone
            /// </summary>
            public HumanBodyBones HumanBodyBoneType
            {
                get
                {
                    return RawJoint.Type.ToUnityBones();
                }
            }

            /// <summary>
            /// Joint position in meters
            /// </summary>
            public Vector3 Position
            {
                get
                {
                    return RawJoint.ToVector3() * 0.001f;
                }
            }

            /// <summary>
            /// Joint orientation
            /// </summary>
            public Quaternion Rotation
            {
                get
                {
                    return RawJoint.ToQuaternion(); //Quaternion.Inverse(CalibrationInfo.SensorOrientation)
                }
            }

            /// <summary>
            /// Mirrored orientation of the joint
            /// </summary>
            public Quaternion RotationMirrored
            {
                get
                {
                    return RawJoint.ToQuaternionMirrored();
                }
            }

            /// <summary>
            /// Projection and normalized joint coordinates
            /// </summary>
            public Vector2 Proj
            {
                get
                {
                    return new Vector2(Mathf.Clamp01(RawJoint.Proj.X), Mathf.Clamp01(RawJoint.Proj.Y));
                }
            }
        }

        public nuitrack.Skeleton RawSkeleton
        {
            get; private set;
        }

        public SkeletonData(nuitrack.Skeleton skeleton)
        {
            RawSkeleton = skeleton;
        }

        public Joint GetJoint(nuitrack.JointType jointType)
        {
            nuitrack.Joint joint = RawSkeleton.GetJoint(jointType);
            return new Joint(joint);
        }

        public Joint GetJoint(HumanBodyBones humanBodyBone)
        {
            return GetJoint(humanBodyBone.ToNuitrackJoint());
        }
    }


    nuitrack.UserHands rawUserHands = null;
    nuitrack.Gesture? rawGesture = null;

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
        get
        {
            return rawUserHands?.LeftHand;
        }
    }

    public nuitrack.HandContent? RightHand
    {
        get
        {
            return rawUserHands?.RightHand;
        }
    }

    public nuitrack.GestureType? GestureType
    {
        get
        {
            return rawGesture?.Type;
        }
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
        rawUserHands = userHands;
    }

    public void SetFace(Face face)
    {
        Face = face;
    }

    public void SetGesture(nuitrack.Gesture? gesture)
    {
        rawGesture = gesture;
    }

    public UserData(int id)
    {
        ID = id;
    }
}