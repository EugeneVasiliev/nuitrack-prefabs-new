using System.Linq;
using System.Collections.Generic;


public class Users
{
    ulong lastTimeStamp = 0;

    readonly Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public int Count
    {
        get
        {
            return users.Count;
        }
    }

    public UserData Current
    {
        get
        {
            if (CurrentUserTracker.CurrentSkeleton != null && users.ContainsKey(CurrentUserTracker.CurrentSkeleton.ID))
                return users[CurrentUserTracker.CurrentSkeleton.ID];
            else
                return null;
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
        return users.Values.ToList();
    }

    void CheckTimeStamp(ulong newTimeStamp)
    {
        if (lastTimeStamp != newTimeStamp)
        {
            lastTimeStamp = newTimeStamp;
            users.Clear();
        }
    }

    UserData TryGetUser(int id)
    {
        if (!users.ContainsKey(id))
            users.Add(id, new UserData(id, lastTimeStamp));

        return users[id];
    }

    internal void AddData(nuitrack.SkeletonData skeletonData)
    {
        CheckTimeStamp(skeletonData.Timestamp);

        foreach (nuitrack.Skeleton skeleton in skeletonData.Skeletons)
            TryGetUser(skeleton.ID).SetSkeleton(skeleton);
    }

    internal void AddData(nuitrack.HandTrackerData handTrackerData)
    {
        CheckTimeStamp(handTrackerData.Timestamp);

        foreach (nuitrack.UserHands hands in handTrackerData.UsersHands)
            TryGetUser(hands.UserId).SetUserHands(hands);
    }

    internal void AddData(nuitrack.GestureData gestureData)
    {
        CheckTimeStamp(gestureData.Timestamp);

        foreach (nuitrack.Gesture gesture in gestureData.Gestures)
            TryGetUser(gesture.UserID).SetGesture(gesture.Type);
    }

    internal void AddData(JsonInfo jsonInfo)
    {
        if (jsonInfo == null || jsonInfo.Instances == null)
            return;

        CheckTimeStamp(jsonInfo.Timestamp);

        foreach (Instances instances in jsonInfo.Instances)
            if (instances.face != null)
                TryGetUser(instances.id).SetFace(instances.face);
    }
}