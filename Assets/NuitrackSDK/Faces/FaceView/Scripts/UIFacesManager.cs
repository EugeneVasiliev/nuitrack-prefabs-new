using UnityEngine;

using System.Collections.Generic;


public class UIFacesManager : MonoBehaviour
{
    [SerializeField] RectTransform spawnRectTransform;

    [SerializeField, Range(0, 6)] int faceCount = 6;         //Max number of skeletons tracked by Nuitrack
    [SerializeField] UIFaceInfo faceFrame;

    List<UIFaceInfo> uiFaces = new List<UIFaceInfo>();

    void Start()
    {
        for (int i = 0; i < faceCount; i++)
        {
            GameObject newFrame = Instantiate(faceFrame.gameObject, spawnRectTransform);
            newFrame.SetActive(false);

            UIFaceInfo faceInfo = newFrame.GetComponent<UIFaceInfo>();
            faceInfo.Initialize(spawnRectTransform);
            faceInfo.autoProcessing = false;

            uiFaces.Add(faceInfo);
        }
    }

    void Update()
    {
        List<UserData> userData = NuitrackManager.Users.GetList();

        for (int i = 0; i < uiFaces.Count; i++)
        {
            if (i < NuitrackManager.Users.Count)
            {
                uiFaces[i].gameObject.SetActive(true);
                uiFaces[i].ProcessFace(userData[i]);
            }
            else
            {
                uiFaces[i].gameObject.SetActive(false);
            }
        }
    }
}
