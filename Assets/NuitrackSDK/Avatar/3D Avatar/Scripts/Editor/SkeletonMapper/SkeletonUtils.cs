using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using Reflection = System.Reflection;

namespace NuitrackSDKEditor.Avatar
{
    public static class SkeletonUtils
    {
        /// <summary>
        /// Get a list of valid bones for the specified skeleton
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <returns>Dictionary of found Transform and validity value</returns>
        public static Dictionary<Transform, bool> GetValidBones(Transform root)
        {
            System.Type avatarSetupType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool");
            Reflection.MethodInfo avatarSetuMethodInfo = avatarSetupType.GetMethod("GetModelBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<Transform, bool> validBones = avatarSetuMethodInfo.Invoke(null, new object[] { root, false, null }) as Dictionary<Transform, bool>;

            return validBones;
        }

        /// <summary>
        /// Get a bone map for the specified skeleton
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <returns>Dictionary of bone type and found Transform</returns>
        public static Dictionary<HumanBodyBones, Transform> GetBonesMap(Transform root)
        {
            Dictionary<Transform, bool> validBones = GetValidBones(root);

            if (validBones == null)
                return null;

            System.Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarAutoMapper");
            Reflection.MethodInfo methodInfo = toolbarType.GetMethod("MapBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<int, Transform> boneIDMap = methodInfo.Invoke(null, new object[] { root, validBones }) as Dictionary<int, Transform>;
            Dictionary<HumanBodyBones, Transform> boneTransformMap = boneIDMap.ToDictionary(k => (HumanBodyBones)k.Key, v => v.Value);

            return boneTransformMap;
        }
    }
}
