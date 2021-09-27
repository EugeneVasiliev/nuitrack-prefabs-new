using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Reflection;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonBonesView
    {
        object skeletonDrawer = null;

        Type boneHandleType = typeof(Editor).Assembly.GetType("UnityEditor.Handles").GetNestedType("BoneRenderer", BindingFlags.NonPublic);
        Type skeletonDrawerType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSkeletonDrawer");

        public SkeletonBonesView()
        {
            skeletonDrawer = Activator.CreateInstance(boneHandleType);
        }

        public void DrawSkeleton(Transform root, List<Transform> mappedJointTransforms)
        {
            Dictionary<Transform, bool> validBones = SkeletonUtils.GetValidBones(root);

            foreach (Transform joint in mappedJointTransforms)
                if (validBones.ContainsKey(joint))
                    validBones[joint] = false;

            MethodInfo drawSkeletonMethodInfo = skeletonDrawerType.GetMethod(
                "DrawSkeleton", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(Transform), typeof(Dictionary<Transform, bool>), boneHandleType }, null);

            drawSkeletonMethodInfo.Invoke(null, new object[] { root, validBones, skeletonDrawer });
        }
    }
}
