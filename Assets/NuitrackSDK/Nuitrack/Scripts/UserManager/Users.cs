using System.Collections;
using System.Collections.Generic;

using nuitrack;


public class Users : IEnumerable
{
    public delegate void UserHandler(UserData user);

    public event UserHandler OnUserEnter;
    public event UserHandler OnUserExit;

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

    Dictionary<int, UserData> users = new Dictionary<int, UserData>();

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

    UserData TryGetUser(int id, ref Dictionary<int, UserData> usersDict)
    {
        if (!usersDict.ContainsKey(id))
            usersDict.Add(id, new UserData(id));

        return usersDict[id];
    }

    internal void UpdateData(SkeletonData skeletonData, HandTrackerData handTrackerData, GestureData gestureData, JsonInfo jsonInfo)
    {
        Dictionary<int, UserData> newUsers = new Dictionary<int, UserData>();

        if (skeletonData != null)
        {
            foreach (Skeleton skeleton in skeletonData.Skeletons)
                TryGetUser(skeleton.ID, ref newUsers).AddData(skeleton);

            if (skeletonData == null || skeletonData.NumUsers == 0)
                CurrentUserID = 0;
            else
            {
                if (CurrentUserID != 0)
                    CurrentUserID = newUsers.ContainsKey(CurrentUserID) ? 0 : CurrentUserID;

                if (CurrentUserID == 0)
                    CurrentUserID = skeletonData.Skeletons[0].ID;
            }
        }

        if (handTrackerData != null)
            foreach (UserHands hands in handTrackerData.UsersHands)
                TryGetUser(hands.UserId, ref newUsers).AddData(hands);

        if (gestureData != null)
            foreach (Gesture gesture in gestureData.Gestures)
                TryGetUser(gesture.UserID, ref newUsers).AddDtata(gesture);

        if (jsonInfo != null && jsonInfo.Instances != null)
            foreach (Instances instances in jsonInfo.Instances)
                if (!instances.face.IsEmpty)
                    TryGetUser(instances.id, ref newUsers).AddData(instances.face);

        foreach (UserData user in this)
            if (!newUsers.ContainsKey(user.ID))
                OnUserExit?.Invoke(user);

        Dictionary<int, UserData> oldUsers = users;
        users = newUsers;

        foreach (UserData user in this)
            if (!oldUsers.ContainsKey(user.ID))
                OnUserEnter?.Invoke(user);

        foreach (UserData user in oldUsers.Values)
            user.Dispose();

        oldUsers.Clear();
    }
}