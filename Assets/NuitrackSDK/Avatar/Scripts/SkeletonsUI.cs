using System.Collections.Generic;
using UnityEngine;
using nuitrack;

public class SkeletonsUI : MonoBehaviour
{
    [Range(0, 6)]
    public int skeletonCount = 6;         //Max number of skeletons tracked by Nuitrack
    [SerializeField] UIAvatar skeletonAvatar;

    List<UIAvatar> avatars = new List<UIAvatar>();

    void Start()
    {
        for (int i = 0; i < skeletonCount; i++)
        {
            GameObject newAvatar = Instantiate(skeletonAvatar.gameObject, transform);
            UIAvatar skeleton = newAvatar.GetComponent<UIAvatar>();
            skeleton.autoProcessing = false;
            avatars.Add(skeleton);
        }

        NuitrackManager.SkeletonTracker.SetNumActiveUsers(skeletonCount);

        NuitrackManager.onSkeletonTrackerUpdate += OnSkeletonUpdate;
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            if (i < skeletonData.Skeletons.Length)
            {
                avatars[i].gameObject.SetActive(true);
                avatars[i].ProcessSkeleton(skeletonData.Skeletons[i]);
            }
            else
            {
                avatars[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        NuitrackManager.onSkeletonTrackerUpdate -= OnSkeletonUpdate;
    }
}
