using System.Linq;
using System.Collections.Generic;

public class Users : System.Collections.IEnumerable
{
    readonly Dictionary<int, UserData> users = new Dictionary<int, UserData>();

    public System.Collections.IEnumerator GetEnumerator()
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
        return users.Values.ToList();
    }

    UserData TryGetUser(int id)
    {
        if (!users.ContainsKey(id))
            users.Add(id, new UserData(id));

        return users[id];
    }

    internal void Clear()
    {
        users.Clear();
    }

    internal void AddData(nuitrack.SkeletonData skeletonData)
    {
        foreach (nuitrack.Skeleton skeleton in skeletonData.Skeletons)
            TryGetUser(skeleton.ID).SetSkeleton(skeleton);

        if (skeletonData == null || skeletonData.NumUsers == 0)
        {
            CurrentUserID = 0;
            return;
        }

        if (CurrentUserID != 0)
            CurrentUserID = (GetUser(CurrentUserID) == null) ? 0 : CurrentUserID;

        if (CurrentUserID == 0)
            CurrentUserID = skeletonData.Skeletons[0].ID;
    }

    internal void AddData(nuitrack.HandTrackerData handTrackerData)
    {
        foreach (nuitrack.UserHands hands in handTrackerData.UsersHands)
            TryGetUser(hands.UserId).SetUserHands(hands);
    }

    internal void AddData(nuitrack.GestureData gestureData)
    {
        foreach (nuitrack.Gesture gesture in gestureData.Gestures)
            TryGetUser(gesture.UserID).SetGesture(gesture);
    }

    internal void AddData(JsonInfo jsonInfo)
    {
        if (jsonInfo == null || jsonInfo.Instances == null)
            return;

        foreach (Instances instances in jsonInfo.Instances)
            if (instances.face != null)
                TryGetUser(instances.id).SetFace(instances.face);
    }
}