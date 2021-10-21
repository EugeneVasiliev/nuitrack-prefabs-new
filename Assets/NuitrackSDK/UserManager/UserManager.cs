using System.Linq;
using System.Collections.Generic;

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

    public UserData(nuitrack.Skeleton skeleton, nuitrack.UserHands userHands, Face face, nuitrack.GestureType? gestureType)
    {
        ID = skeleton.ID;
        Skeleton = new SkeletonData(skeleton);

        if (userHands != null)
        {
            LeftHand = userHands.LeftHand;
            RightHand = userHands.RightHand;
        }

        Face = face;
        GestureType = gestureType;
    }
}


public class Floor
{
    public Vector3 Point
    {
        get; private set;
    }

    public Vector3 Normal
    {
        get; private set;
    }

    public Plane Plane
    {
        get
        {
            return new Plane(Normal, Point);
        }
    }

    public Floor(Vector3 point, Vector3 normal)
    {
        Point = point;
        Normal = normal;
    }
}


public class UserManager : MonoBehaviour
{
    [SerializeField] float jointConfidence = 0.1f;

    ulong lastTimeStamp = 0;

    static Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public static int UserCount
    {
        get;
        private set;
    }

    public static Floor Floor
    {
        get; private set;
    }

    public static UserData CurrentUser
    {
        get;
        private set;
    }

    public static UserData GetUser(int userID)
    {
        if (users.ContainsKey(userID))
            return users[userID];
        else
            return null;
    }

    public static List<UserData> GetUserList()
    {
        return users.Values.ToList();
    }


    void Awake()
    {
        Floor = new Floor(Vector3.zero, Vector3.zero);
    }

    void Update()
    {
        Vector3 floorPoint = NuitrackManager.UserFrame.Floor.ToVector3();
        Vector3 floorNormal = NuitrackManager.UserFrame.FloorNormal.ToVector3().normalized;

        Floor = new Floor(floorPoint, floorNormal);

        JsonInfo faceInfo = NuitrackManager.NuitrackJson;

        if (NuitrackManager.SkeletonData != null)
        {
            if (NuitrackManager.SkeletonData.Timestamp != lastTimeStamp)
            {
                users.Clear();
                CurrentUser = null;
                UserCount = NuitrackManager.SkeletonData.NumUsers;

                foreach (nuitrack.Skeleton skeleton in NuitrackManager.SkeletonData.Skeletons)
                {
                    nuitrack.UserHands userHands = GetUserHands(skeleton.ID);
                    Face face = GetFace(skeleton.ID, faceInfo);
                    nuitrack.GestureType? gestureType = GetGestureType(skeleton.ID);

                    UserData userData = new UserData(skeleton, userHands, face, gestureType);
                    users.Add(skeleton.ID, userData);

                    if (CurrentUserTracker.CurrentUser == skeleton.ID)
                        CurrentUser = userData;

                    
                }

                lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            }
        }
        else
            users.Clear();
    }


    nuitrack.UserHands GetUserHands(int userID)
    {
        nuitrack.UserHands userHands = null;

        if (NuitrackManager.HandTrackerData != null)
            userHands = NuitrackManager.HandTrackerData.GetUserHandsByID(userID);

        return userHands;
    }

    Face GetFace(int userID, JsonInfo faceInfo)
    {
        Face face = null;

        if (faceInfo != null && faceInfo.Instances != null)
        {
            foreach(Instances instances in faceInfo.Instances)
                if(instances.id == userID)
                    return instances.face;
        }

        return face;
    }

    nuitrack.GestureType? GetGestureType(int userID)
    {
        nuitrack.GestureType? gestureType = null;

        if (NuitrackManager.GestureRecognizer != null)
        {
            nuitrack.GestureData gestureData = NuitrackManager.GestureRecognizer.GetGestureData();

            if (gestureData != null)
            {
                foreach (nuitrack.Gesture ges in gestureData.Gestures)
                    if (ges.UserID == userID)
                        return ges.Type;
            }
        }

        return gestureType;
    }
}
