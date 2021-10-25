using System.Collections;
using System.Collections.Generic;

using nuitrack;


public class Users : IEnumerable
{
    /// <summary>
    /// Minimum allowed ID
    /// </summary>
    public static int MinID
    {
        get
        {
            return 1;
        }
    }

    /// <summary>
    /// Maximum allowed ID
    /// </summary>
    public static int MaxID
    {
        get
        {
            return 6;
        }
    }

    readonly Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public IEnumerator GetEnumerator()
    {
        return users.Values.GetEnumerator();
    }

    /// <summary>
    /// The number of users detected at the moment.
    /// </summary>
    public int Count
    {
        get
        {
            return users.Count;
        }
    }

    /// <summary>
    /// ID of the current user. <see cref="Current"/>
    /// </summary>
    public int CurrentUserID
    {
        get; private set;
    }

    /// <summary>
    /// UserData of the current user. Maybe null.
    ///
    /// <para>
    /// The queue rule is in effect, the current user is considered the first to enter the frame.
    /// When the current user leaves the frame, control is transferred to the next detected one)
    /// </para>
    /// </summary>
    public UserData Current
    {
        get
        {
            return GetUser(CurrentUserID) ?? null;
        }
    }

    /// <summary>
    /// Get a user by ID. Maybe null.
    /// </summary>
    /// <param name="userID">ID of the required user</param>
    /// <returns>User data, if the user exists otherwise null.</returns>
    public UserData GetUser(int userID)
    {
        if (users.ContainsKey(userID))
            return users[userID];
        else
            return null;
    }

    /// <summary>
    /// Get a list of all users in the form of a List<UserData>
    /// </summary>
    /// <returns>List of all users</returns>
    public List<UserData> GetList()
    {
        return new List<UserData>(users.Values);
    }

    UserData TryGetUser(int id)
    {
        if (!users.ContainsKey(id))
            users.Add(id, new UserData(id));

        return users[id];
    }

    internal void UpdateData(SkeletonData skeletonData, HandTrackerData handTrackerData, GestureData gestureData, JsonInfo jsonInfo)
    {
        foreach (UserData user in this)
            user.Dispose();

        users.Clear();

        if (skeletonData != null)
        {
            foreach (Skeleton skeleton in skeletonData.Skeletons)
                TryGetUser(skeleton.ID).AddData(skeleton);

            if (skeletonData == null || skeletonData.NumUsers == 0)
                CurrentUserID = 0;
            else
            {
                if (CurrentUserID != 0)
                    CurrentUserID = (GetUser(CurrentUserID) == null) ? 0 : CurrentUserID;

                if (CurrentUserID == 0)
                    CurrentUserID = skeletonData.Skeletons[0].ID;
            }
        }

        if (handTrackerData != null)
        {
            foreach (UserHands hands in handTrackerData.UsersHands)
                TryGetUser(hands.UserId).AddData(hands);
        }

        if (gestureData != null)
        {
            foreach (Gesture gesture in gestureData.Gestures)
                TryGetUser(gesture.UserID).AddDtata(gesture);
        }

        if (jsonInfo != null && jsonInfo.Instances != null)
        {
            foreach (Instances instances in jsonInfo.Instances)
                if (instances.face != null)
                    TryGetUser(instances.id).AddData(instances.face);
        }
    }
}