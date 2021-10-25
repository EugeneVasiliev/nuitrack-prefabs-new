using System.Collections;
using System.Collections.Generic;

using nuitrack;


public class Users : IEnumerable
{
    readonly Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public IEnumerator GetEnumerator()
    {
        return users.Values.GetEnumerator();
    }

    public int Count
    {
        get
        {
            return users.Count;
        }
    }

    public int CurrentUserID
    {
        get; private set;
    }

    public UserData Current
    {
        get
        {
            return GetUser(CurrentUserID) ?? null;
        }
    }

    public UserData GetUser(int userID)
    {
        if (users.ContainsKey(userID))
            return users[userID];
        else
            return null;
    }

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