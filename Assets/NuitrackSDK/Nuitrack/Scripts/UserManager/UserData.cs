using UnityEngine;


public class UserData
{
    /// <summary>
    /// Wrapper for Nuitrack Skeleton <see cref="nuitrack.Skeleton"/>
    /// </summary>
    public class SkeletonData
    {
        /// <summary>
        /// Wrapper for Nuitrack Joint <see cref="nuitrack.Joint"/>
        /// </summary>
        public class Joint
        {
            /// <summary>
            /// Raw nuitrack.Joint data
            /// </summary>
            public nuitrack.Joint RawJoint
            {
                get; private set;
            }

            public Joint(nuitrack.Joint joint)
            {
                RawJoint = joint;
            }

            /// <summary>
            /// Confidence for this joint
            /// </summary>
            public float Confidence
            {
                get
                {
                    return RawJoint.Confidence;
                }
            }

            /// <summary>
            /// Joint type from nuitrack
            /// </summary>
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

        /// <summary>
        /// Raw nuitrack.Skeleton data
        /// </summary>
        public nuitrack.Skeleton RawSkeleton
        {
            get; private set;
        }

        public SkeletonData(nuitrack.Skeleton skeleton)
        {
            RawSkeleton = skeleton;
        }

        /// <summary>
        /// Get a wrapper object for the specified joint
        /// </summary>
        /// <param name="jointType">Joint type <see cref="nuitrack.JointType"/></param>
        /// <returns>Shell object <see cref="Joint"/></returns>
        public Joint GetJoint(nuitrack.JointType jointType)
        {
            nuitrack.Joint joint = RawSkeleton.GetJoint(jointType);
            return new Joint(joint);
        }

        /// <summary>
        /// Get a wrapper object for the specified joint
        /// </summary>
        /// <param name="humanBodyBone">Bone type <see cref="HumanBodyBones"/></param>
        /// <returns>Shell object <see cref="Joint"/></returns>
        public Joint GetJoint(HumanBodyBones humanBodyBone)
        {
            return GetJoint(humanBodyBone.ToNuitrackJoint());
        }
    }

    /// <summary>
    /// Wrapper for Nuitrack HandContent <see cref=nuitrack.HandContent"/>
    /// </summary>
    public class Hand
    {
        /// <summary>
        /// Raw nuitrack.HandContent data
        /// </summary>
        public nuitrack.HandContent RawHandContent
        {
            get; private set;
        }

        /// <summary>
        /// Projection coordinates of the hand with the starting point in the upper left corner.
        /// </summary>
        public Vector2 ProjPosition
        {
            get
            {
                Debug.Log(new Vector2(RawHandContent.X, 1 - RawHandContent.Y).ToString());
                Debug.Log(Position.ToString());
                return new Vector2(RawHandContent.X, 1 - RawHandContent.Y);
            }
        }

        /// <summary>
        /// Real 3D hand position with depth
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return new Vector3(RawHandContent.XReal, RawHandContent.YReal, RawHandContent.ZReal) * 0.001f;
            }
        }

        /// <summary>
        /// Is the click currently being executed
        /// </summary>
        public bool Click
        {
            get
            {
                return RawHandContent.Click;
            }
        }

        /// <summary>
        /// Compression of the hand (in percent), where 0 - corresponds to an open palm, 1- a clenched fist
        /// </summary>
        public int Pressure
        {
            get
            {
                return RawHandContent.Pressure;
            }
        }

        public Hand(nuitrack.HandContent handContent)
        {
            RawHandContent = handContent;
        }
    }

    public nuitrack.UserHands RawUserHands
    {
        get; private set;
    }

    public nuitrack.Gesture? RawGesture
    {
        get; private set;
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

    public Hand LeftHand
    {
        get; private set;
    }

    public Hand RightHand
    {
        get; private set;
    }

    /// <summary>
    /// The user's last gesture. The data is up-to-date for one Update frame after the event occurs.
    /// </summary>
    public nuitrack.GestureType? GestureType
    {
        get
        {
            return RawGesture?.Type;
        }
    }

    public Face Face
    {
        get; private set;
    }

    public UserData(int id)
    {
        ID = id;
    }

    internal void AddData(nuitrack.Skeleton skeleton)
    {
        Skeleton = new SkeletonData(skeleton);
    }

    internal void AddData(nuitrack.UserHands userHands)
    {
        RawUserHands = userHands;

        if (userHands.LeftHand != null)
            LeftHand = new Hand(userHands.LeftHand.Value);

        if (userHands.RightHand != null)
            RightHand = new Hand(userHands.RightHand.Value);
    }

    internal void AddData(Face face)
    {
        Face = face;
    }

    internal void AddDtata(nuitrack.Gesture? gesture)
    {
        RawGesture = gesture;
    }
}