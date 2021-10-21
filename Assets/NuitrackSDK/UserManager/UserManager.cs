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


public class UserManager
{
    static ulong lastTimeStamp = 0;

    static Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public static int UserCount
    {
        get
        {
            return users.Count;
        }
    }

    public static Floor Floor
    {
        get; private set;
    }

    public static UserData CurrentUser
    {
        get
        {
            if (CurrentUserTracker.CurrentSkeleton != null && users.ContainsKey(CurrentUserTracker.CurrentSkeleton.ID))
                return users[CurrentUserTracker.CurrentSkeleton.ID];
            else
                return null;
        }
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

    static void CheckTimeStamp(ulong newTimeStamp)
    {
        if (lastTimeStamp != newTimeStamp)
        {
            lastTimeStamp = newTimeStamp;
            users.Clear();
        }
    }

    public static void AddData(nuitrack.SkeletonData skeletonData)
    {
        CheckTimeStamp(skeletonData.Timestamp);

        foreach (nuitrack.Skeleton skeleton in skeletonData.Skeletons)
        {
            if (!users.ContainsKey(skeleton.ID))
                users.Add(skeleton.ID, new UserData(skeleton.ID));

            users[skeleton.ID].SetSkeleton(skeleton);
        }

        Debug.Log("SkeletonData " + users.Count);
    }

    public static void AddData(nuitrack.HandTrackerData handTrackerData)
    {
        CheckTimeStamp(handTrackerData.Timestamp);

        foreach (nuitrack.UserHands hands in handTrackerData.UsersHands)
        {
            if (!users.ContainsKey(hands.UserId))
                users.Add(hands.UserId, new UserData(hands.UserId));

            users[hands.UserId].SetUserHands(hands);
        }
    }

    public static void AddData(nuitrack.GestureData gestureData)
    {
        CheckTimeStamp(gestureData.Timestamp);

        foreach (nuitrack.Gesture gesture in gestureData.Gestures)
        {
            if (!users.ContainsKey(gesture.UserID))
                users.Add(gesture.UserID, new UserData(gesture.UserID));

            users[gesture.UserID].SetGesture(gesture.Type);
        }
    }

    public static void AddData(JsonInfo jsonInfo)
    {
        if (jsonInfo == null || jsonInfo.Instances == null)
            return;

        CheckTimeStamp(jsonInfo.Timestamp);

        foreach (Instances instances in jsonInfo.Instances)
        {
            if (!users.ContainsKey(instances.id))
                users.Add(instances.id, new UserData(instances.id));

            if(instances.face != null)
                users[instances.id].SetFace(instances.face);
        }
    }
}
