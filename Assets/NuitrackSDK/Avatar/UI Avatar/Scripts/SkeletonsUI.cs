﻿using UnityEngine;

using System.Collections.Generic;


namespace NuitrackSDK.Avatar
{
    public class SkeletonsUI : MonoBehaviour
    {
        [SerializeField] RectTransform spawnRectTransform;

        [SerializeField, Range(0, 6)] int skeletonCount = 6;         //Max number of skeletons tracked by Nuitrack
        [SerializeField] UIAvatar skeletonAvatar;

        List<UIAvatar> avatars = new List<UIAvatar>();

        void Start()
        {
            for (int i = 0; i < skeletonCount; i++)
            {
                GameObject newAvatar = Instantiate(skeletonAvatar.gameObject, spawnRectTransform);
                UIAvatar skeleton = newAvatar.GetComponent<UIAvatar>();
                skeleton.SkeletonID = i + 1;
                avatars.Add(skeleton);
            }

            NuitrackManager.SkeletonTracker.SetNumActiveUsers(skeletonCount);
        }

        void Update()
        {
            foreach(UIAvatar uIAvatar in avatars)
                uIAvatar.gameObject.SetActive(NuitrackManager.Users.GetUser(uIAvatar.SkeletonID) != null);
        }
    }
}