using UnityEngine;
using System;

//logic (and number of tracked users) may change from app to app
//for developer's needs
//in current case it's an ID of first found skeleton from skeleton tracker
//and reset only if we have a frame with no current skeleton ID

public class CurrentUserTracker : MonoBehaviour
{
    public static int CurrentUser
    {
        get; private set;
    }

    [Obsolete("Use NuitrackManager.Users.Current.Skeleton", false)]
    public static nuitrack.Skeleton CurrentSkeleton
    {
        get; private set;
    }

    void Start()
    {
        DontDestroyOnLoad(this);
        NuitrackManager.onSkeletonTrackerUpdate += NuitrackManager_onSkeletonTrackerUpdate;
    }

    void NuitrackManager_onSkeletonTrackerUpdate(nuitrack.SkeletonData skeletonData)
    {
        if ((skeletonData == null) || (skeletonData.NumUsers == 0))
        {
            CurrentUser = 0;
            CurrentSkeleton = null;
            return;
        }

        if (CurrentUser != 0)
        {
            CurrentSkeleton = skeletonData.GetSkeletonByID(CurrentUser);
            CurrentUser = (CurrentSkeleton == null) ? 0 : CurrentUser;
        }

        if (CurrentUser == 0)
        {
            CurrentUser = skeletonData.Skeletons[0].ID;
            CurrentSkeleton = skeletonData.Skeletons[0];
        }
    }
}