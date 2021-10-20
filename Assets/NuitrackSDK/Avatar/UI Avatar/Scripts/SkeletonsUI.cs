﻿using System.Collections.Generic;
using UnityEngine;
using nuitrack;

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
            for (int i = 0; i < avatars.Count; i++)
                avatars[i].gameObject.SetActive(i < UserManager.UserCount);
        }
    }
}