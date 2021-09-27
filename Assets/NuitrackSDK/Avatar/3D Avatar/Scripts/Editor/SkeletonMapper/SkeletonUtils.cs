using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using Reflection = System.Reflection;

namespace NuitrackSDKEditor.Avatar
{
    public static class SkeletonUtils
    {
        public static Dictionary<Transform, bool> GetValidBones(Transform root)
        {
            System.Type avatarSetupType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool");
            Reflection.MethodInfo avatarSetuMethodInfo = avatarSetupType.GetMethod("GetModelBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<Transform, bool> validBones = avatarSetuMethodInfo.Invoke(null, new object[] { root, false, null }) as Dictionary<Transform, bool>;

            return validBones;
        }

        public static Dictionary<HumanBodyBones, Transform> MapBones(Transform root)
        {
            Dictionary<Transform, bool> validBones = GetValidBones(root);

            if (validBones == null)
                return null;

            System.Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarAutoMapper");
            Reflection.MethodInfo methodInfo = toolbarType.GetMethod("MapBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<int, Transform> outData = methodInfo.Invoke(null, new object[] { root, validBones }) as Dictionary<int, Transform>;

            Dictionary<HumanBodyBones, Transform> boneTransform = outData.ToDictionary(k => (HumanBodyBones)k.Key, v => v.Value);

            return boneTransform;
        }
    }
}
