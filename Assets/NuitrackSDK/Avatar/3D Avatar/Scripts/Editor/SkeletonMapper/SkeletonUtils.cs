using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using Reflection = System.Reflection;


namespace NuitrackSDKEditor.Avatar
{
    public static class SkeletonUtils
    {
        static System.Type AvatarSetupToolType
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool");
            }
        }

        static System.Type AvatarAutoMapperType
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarAutoMapper");
            }
        }

        /// <summary>
        /// Get a list of valid bones for the specified skeleton
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <returns>Dictionary of found Transform and validity value</returns>
        public static Dictionary<Transform, bool> GetValidBones(Transform root)
        {
            Reflection.MethodInfo avatarSetuMethodInfo = AvatarSetupToolType.GetMethod("GetModelBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

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

            Reflection.MethodInfo methodInfo = AvatarAutoMapperType.GetMethod("MapBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<int, Transform> boneIDMap = methodInfo.Invoke(null, new object[] { root, validBones }) as Dictionary<int, Transform>;
            Dictionary<HumanBodyBones, Transform> boneTransformMap = boneIDMap.ToDictionary(k => (HumanBodyBones)k.Key, v => v.Value);

            return boneTransformMap;
        }

        public static void SetToTPose(Transform root, Dictionary<HumanBodyBones, Transform> includeJoints)
        {
            if (!includeJoints.ContainsKey(HumanBodyBones.Hips))
            {
                Debug.LogError(string.Format("It is impossible to set T-pose because the joint <color=red><b>{0}</b></color> is not set", HumanBodyBones.Hips));
                return;
            }

            Dictionary<Transform, bool> validBones = GetValidBones(root);

            Reflection.MethodInfo getHumanBonesMethodInfo = AvatarSetupToolType.GetMethod(
                "GetHumanBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static,
                null, new System.Type[] { typeof(Dictionary<string, string>), typeof(Dictionary<Transform, bool>) }, null);

            Transform hipsTransform = includeJoints.ContainsKey(HumanBodyBones.Hips) ? includeJoints[HumanBodyBones.Hips] : null;

            Vector3 waistPosition = hipsTransform != null ? includeJoints[HumanBodyBones.Hips].position : Vector3.zero;
            Quaternion waistRotation = hipsTransform != null ? includeJoints[HumanBodyBones.Hips].rotation : Quaternion.identity;

            Dictionary<string, string> existingMappings = includeJoints.ToDictionary(k => k.Key.ToString(), v => v.Value.name);

            object[] boneWrapper = getHumanBonesMethodInfo.Invoke(null, new object[] { existingMappings, validBones }) as object[];

            Reflection.MethodInfo makePoseValidMethodInfo = AvatarSetupToolType.GetMethod("MakePoseValid", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            makePoseValidMethodInfo.Invoke(null, new object[] { boneWrapper });

            if (hipsTransform != null)
            {
                hipsTransform.position = waistPosition;
                hipsTransform.rotation = waistRotation;
            }
        }
    }
}
