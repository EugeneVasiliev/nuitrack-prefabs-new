using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using Reflection = System.Reflection;


namespace NuitrackSDKEditor.Avatar
{
    public static class SkeletonUtils
    {
        #region Reflection methods
        
        /// <summary>
        /// See the Unity source code
        /// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/Avatar/AvatarSetupTool.cs#L352">
        /// UnityEditor.AvatarSetupTool.GetModelBones
        /// </see>
        /// </summary>
        static Reflection.MethodInfo GetModelBones
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool").
                    GetMethod("GetModelBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);
            }
        }

        /// <summary>
        /// See the Unity source code
        /// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/Avatar/AvatarAutoMapper.cs#L273">
        /// UnityEditor.AvatarAutoMapper.MapBones
        /// </see>
        /// </summary>
        static Reflection.MethodInfo MapBones
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarAutoMapper").GetMethod("MapBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);
            }
        }

        /// <summary>
        /// See the Unity source code
        /// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/Avatar/AvatarSetupTool.cs#L522">
        /// UnityEditor.AvatarSetupTool.GetHumanBones
        /// </see> 
        /// </summary>
        static Reflection.MethodInfo GetHumanBones
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool").
                    GetMethod(
                    "GetHumanBones", 
                    Reflection.BindingFlags.Public | Reflection.BindingFlags.Static, 
                    null, 
                    new System.Type[] { typeof(Dictionary<string, string>), typeof(Dictionary<Transform, bool>) }, 
                    null);
            }
        }

        /// <summary>
        /// See the Unity source code
        /// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/Avatar/AvatarSetupTool.cs#L1077">
        /// UnityEditor.AvatarSetupTool.MakePoseValid
        /// </see>
        /// </summary>
        static Reflection.MethodInfo MakePoseValid
        {
            get
            {
                return typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool").
                    GetMethod("MakePoseValid", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);
            }
        }

        #endregion


        /// <summary>
        /// Get a list of valid bones for the specified skeleton
        /// 
        /// <para>
        /// See also
        /// <seealso cref="GetModelBones"/>
        /// </para>
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <returns>Dictionary of found Transform and validity value</returns>
        public static Dictionary<Transform, bool> GetValidBones(Transform root)
        {
            return GetModelBones.Invoke(null, new object[] { root, false, null }) as Dictionary<Transform, bool>;
        }

        /// <summary>
        /// Get a bone map for the specified skeleton
        /// 
        /// <para>
        /// See also
        /// <seealso cref="MapBones"/> and <seealso cref="GetValidBones"/>
        /// </para>
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <returns>Dictionary of bone type and found Transform</returns>
        public static Dictionary<HumanBodyBones, Transform> GetBonesMap(Transform root)
        {
            Dictionary<Transform, bool> validBones = GetValidBones(root);

            if (validBones == null)
                return null;

            Dictionary<int, Transform> boneIDMap = MapBones.Invoke(null, new object[] { root, validBones }) as Dictionary<int, Transform>;
            Dictionary<HumanBodyBones, Transform> boneTransformMap = boneIDMap.ToDictionary(k => (HumanBodyBones)k.Key, v => v.Value);

            return boneTransformMap;
        }

        /// <summary>
        /// Put the avatar in the T-pose.
        /// 
        /// <para>
        /// See also <seealso cref="GetValidBones"/>, <seealso cref="GetHumanBones"/> and <see cref="MakePoseValid"/>
        /// </para>
        ///
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <param name="includeBones">Types of bones bones and their Transforms that are specified in the skeleton</param>
        public static void SetToTPose(Transform root, Dictionary<HumanBodyBones, Transform> includeBones)
        {
            if (!includeBones.ContainsKey(HumanBodyBones.Hips))
            {
                Debug.LogError(string.Format("It is impossible to set T-pose because the bone <color=red><b>{0}</b></color> is not set", HumanBodyBones.Hips));
                return;
            }

            Dictionary<Transform, bool> validBones = GetValidBones(root);

            Transform hipsTransform = includeBones.ContainsKey(HumanBodyBones.Hips) ? includeBones[HumanBodyBones.Hips] : null;

            Vector3 waistPosition = hipsTransform != null ? includeBones[HumanBodyBones.Hips].position : Vector3.zero;
            Quaternion waistRotation = hipsTransform != null ? includeBones[HumanBodyBones.Hips].rotation : Quaternion.identity;

            Dictionary<string, string> existingMappings = includeBones.ToDictionary(k => k.Key.ToString(), v => v.Value.name);

            object[] boneWrapper = GetHumanBones.Invoke(null, new object[] { existingMappings, validBones }) as object[];

            MakePoseValid.Invoke(null, new object[] { boneWrapper });

            if (hipsTransform != null)
            {
                hipsTransform.position = waistPosition;
                hipsTransform.rotation = waistRotation;
            }
        }
    }
}
