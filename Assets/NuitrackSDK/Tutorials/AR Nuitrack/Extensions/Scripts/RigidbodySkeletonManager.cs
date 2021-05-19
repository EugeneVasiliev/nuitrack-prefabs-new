﻿using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public class RigidbodySkeletonManager : MonoBehaviour
{
    [SerializeField] GameObject rigidBodySkeletonPrefab;
    [SerializeField] Transform space;

    Dictionary<int, RigidbodySkeletonController> skeletons = new Dictionary<int, RigidbodySkeletonController>();

    void Start()
    {
        NuitrackManager.onSkeletonTrackerUpdate += NuitrackManager_onSkeletonTrackerUpdate;
    }

    void OnDestroy()
    {
        NuitrackManager.onSkeletonTrackerUpdate -= NuitrackManager_onSkeletonTrackerUpdate;
    }

    void NuitrackManager_onSkeletonTrackerUpdate(nuitrack.SkeletonData skeletonData)
    {
        Dictionary<int, nuitrack.Skeleton> nuitrackSkeletons = NuitrackManager.SkeletonData.Skeletons.ToDictionary(k => k.ID, v => v);

        foreach (KeyValuePair<int, nuitrack.Skeleton> skeleton in nuitrackSkeletons)
            if (!skeletons.ContainsKey(skeleton.Key))
            {
                RigidbodySkeletonController rigidbodySkeleton = Instantiate(rigidBodySkeletonPrefab, space).GetComponent<RigidbodySkeletonController>();
                rigidbodySkeleton.Initialize(skeleton.Key, space);

                skeletons.Add(skeleton.Key, rigidbodySkeleton);
            }

        foreach (KeyValuePair<int, RigidbodySkeletonController> sk in new Dictionary<int, RigidbodySkeletonController>(skeletons))
            if (!nuitrackSkeletons.ContainsKey(sk.Key))
            {
                Destroy(skeletons[sk.Key].gameObject);
                skeletons.Remove(sk.Key);
            }
    }
}
