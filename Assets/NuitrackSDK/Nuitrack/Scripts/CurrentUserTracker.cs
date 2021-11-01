using System;

//logic (and number of tracked users) may change from app to app
//for developer's needs
//in current case it's an ID of first found skeleton from skeleton tracker
//and reset only if we have a frame with no current skeleton ID

namespace NuitrackSDK
{
    [Obsolete("This class will be removed in the future. The functionality has been moved to Use NuitrackManager.Users")]
    public class CurrentUserTracker
    {
        [Obsolete("Use NuitrackManager.Users.CurrentUserID", false)]
        public static int CurrentUser
        {
            get
            {
                return NuitrackManager.Users.CurrentUserID;
            }
        }

        [Obsolete("Use NuitrackManager.Users.Current.Skeleton", false)]
        public static nuitrack.Skeleton CurrentSkeleton
        {
            get
            {
                UserData user = NuitrackManager.Users.Current;

                if (user != null && user.Skeleton != null)
                    return user.Skeleton.RawSkeleton;
                else
                    return null;
            }
        }
    }
}