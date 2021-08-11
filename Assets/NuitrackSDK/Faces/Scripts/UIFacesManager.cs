using System.Collections.Generic;
using UnityEngine;
using nuitrack;

public class UIFacesManager : MonoBehaviour
{
    [Range(0, 6)]
    public int faceCount = 6;         //Max number of skeletons tracked by Nuitrack
    [SerializeField] UIFaceInfo faceFrame;

    List<UIFaceInfo> uiFaces = new List<UIFaceInfo>();

    void Start()
    {
        for (int i = 0; i < faceCount; i++)
        {
            GameObject newFrame = Instantiate(faceFrame.gameObject, transform);
            newFrame.SetActive(false);
            UIFaceInfo faceInfo = newFrame.GetComponent<UIFaceInfo>();
            faceInfo.autoProcessing = false;
            uiFaces.Add(faceInfo);
        }

        NuitrackManager.onSkeletonTrackerUpdate += OnSkeletonUpdate;
    }

    void OnSkeletonUpdate(SkeletonData skeletonData)
    {
        for (int i = 0; i < uiFaces.Count; i++)
        {
            if (i < skeletonData.Skeletons.Length)
            {
                uiFaces[i].gameObject.SetActive(true);
                uiFaces[i].ProcessFace(skeletonData.Skeletons[i]);
            }
            else
            {
                uiFaces[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        NuitrackManager.onSkeletonTrackerUpdate -= OnSkeletonUpdate;
    }
}
