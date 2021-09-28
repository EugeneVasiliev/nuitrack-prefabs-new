using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Reflection;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonBonesView
    {
        readonly object skeletonDrawer = null;

        readonly Type boneHandleType = typeof(Editor).Assembly.GetType("UnityEditor.Handles").GetNestedType("BoneRenderer", BindingFlags.NonPublic);
        readonly Type skeletonDrawerType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSkeletonDrawer");
        readonly MethodInfo drawSkeletonMethod = null;

        public SkeletonBonesView()
        {
            skeletonDrawer = Activator.CreateInstance(boneHandleType);

            drawSkeletonMethod = skeletonDrawerType.GetMethod(
                "DrawSkeleton", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(Transform), typeof(Dictionary<Transform, bool>), boneHandleType }, null);
        }

        public void DrawSkeleton(Transform root, List<Transform> mappedBoneTransforms)
        {
            Dictionary<Transform, bool> validBones = SkeletonUtils.GetValidBones(root);

            foreach (Transform joint in mappedBoneTransforms)
                if (validBones.ContainsKey(joint))
                    validBones[joint] = false;

            drawSkeletonMethod.Invoke(null, new object[] { root, validBones, skeletonDrawer });
        }
    }
}
