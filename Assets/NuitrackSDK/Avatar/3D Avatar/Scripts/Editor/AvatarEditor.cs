using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;

using Reflection = System.Reflection;

namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(NuitrackSDK.Avatar.Avatar), true)]
    public class AvatarEditor : BaseAvatarEditor
    {
        SkeletonMapperGUI<Transform> skeletonMapper = null;
        SkeletonMapperListGUI<Transform> skeletonJointListUI = null;

        Dictionary<JointType, string> jointFieldMap = new Dictionary<JointType, string>()
        {
            { JointType.Waist, "waist" },
            { JointType.Torso, "torso" },
            { JointType.LeftCollar, "collar" },
            { JointType.RightCollar, "collar" },
            {  JointType.Neck, "neck" },
            { JointType.Head , "head" },

            { JointType.LeftShoulder, "leftShoulder" },
            { JointType.LeftElbow, "leftElbow" },
            { JointType.LeftWrist, "leftWrist" },

            { JointType.RightShoulder, "rightShoulder" },
            { JointType.RightElbow, "rightElbow" },
            { JointType.RightWrist, "rightWrist" },

            { JointType.LeftHip, "leftHip" },
            { JointType.LeftKnee, "leftKnee" },
            { JointType.LeftAnkle, "leftAnkle" },

            { JointType.RightHip, "rightHip" },
            { JointType.RightKnee, "rightKnee" },
            { JointType.RightAnkle, "rightAnkle" }
        };

        Transform GetTransformFromField(JointType jointType)
        {
            return GetJointProperty(jointType).objectReferenceValue as Transform;
        }

        SerializedProperty GetJointProperty(JointType jointType)
        {
            return serializedObject.FindProperty(jointFieldMap[jointType]);
        }

        protected virtual void OnEnable()
        {
            List<JointType> jointMask = jointFieldMap.Keys.ToList();

            skeletonMapper = new SkeletonMapperGUI<Transform>(jointMask);
            skeletonMapper.onDrop += SkeletonMapper_onDrop;
            skeletonMapper.onSelected += SkeletonMapper_onSelected;

            skeletonJointListUI = new SkeletonMapperListGUI<Transform>(jointMask);
            skeletonJointListUI.onDrop += SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected += SkeletonMapper_onSelected;
        }

        protected virtual void OnDisable()
        {
            skeletonMapper.onDrop -= SkeletonMapper_onDrop;
            skeletonMapper.onSelected -= SkeletonMapper_onSelected;
            skeletonMapper = null;

            skeletonJointListUI.onDrop -= SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected -= SkeletonMapper_onSelected;
            skeletonJointListUI = null;
        }

        void SkeletonMapper_onDrop(Transform newJoint, JointType jointType)
        {
            EditJoint(jointType, newJoint);

            if (newJoint != null)
            {
                EditorGUIUtility.PingObject(newJoint);
                SelectJoint = jointType;

                skeletonMapper.SelectedJoint = SelectJoint;
                skeletonJointListUI.SelectedJoint = SelectJoint;
            }
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            SelectJoint = jointType;
            skeletonMapper.SelectedJoint = SelectJoint;
            skeletonJointListUI.SelectedJoint = SelectJoint;

            Transform selectTransform = GetTransformFromField(jointType);

            if(selectTransform != null)
                EditorGUIUtility.PingObject(selectTransform);
        }

        void EditJoint(JointType jointType, Transform objectTransform)
        {
            SerializedProperty jointProperty = GetJointProperty(jointType);
            jointProperty.objectReferenceValue = objectTransform;
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawAvatarGUI()
        {
            EditorGUILayout.Space();

            SerializedProperty vrModeProperty = serializedObject.FindProperty("vrMode");
            vrModeProperty.boolValue = EditorGUILayout.Toggle("VR mode", vrModeProperty.boolValue);
            serializedObject.ApplyModifiedProperties();

            if (vrModeProperty.boolValue)
            {
                SerializedProperty vrHeadProperty = serializedObject.FindProperty("vrHead");
                EditorGUILayout.ObjectField(vrHeadProperty, typeof(GameObject));

                SerializedProperty headTransformProperty = serializedObject.FindProperty("headTransform");
                EditorGUILayout.ObjectField(headTransformProperty, typeof(Transform));
            }

            DrawSkeletonMap();
        }

        protected void DrawSkeletonMap()
        {
            NuitrackSDK.Avatar.Avatar myScript = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Avatar map", EditorStyles.boldLabel); 

            if (skeletonMapper != null)
            {
                List<JointType> activeJoints = jointFieldMap.Keys.Where(k => GetTransformFromField(k) != null).ToList();
                skeletonMapper.Draw(activeJoints);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (GUILayout.Button("Automap"))
                AutoMapping();

            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Skeleton map", "Do you really want to clear the skeleton map?", "Yes", "No"))
                    Clear();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (skeletonJointListUI != null)
            {
                Dictionary<JointType, Transform> jointDict = jointFieldMap.Keys.Where(k => GetTransformFromField(k) != null).ToDictionary(k => k, v => GetTransformFromField(v));
                    
                skeletonJointListUI.Draw(jointDict);
            }
        }


        #region AutoMapping

        List<JointType> excludeAutoFillJoints = new List<JointType>()
        {
            JointType.LeftCollar,
            JointType.RightCollar
        };

        Dictionary<int, Transform> MapBones()
        {
            NuitrackSDK.Avatar.Avatar avatar = target as NuitrackSDK.Avatar.Avatar;

            System.Type avatarSetupType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool");
            Reflection.MethodInfo avatarSetuMethodInfo = avatarSetupType.GetMethod("GetModelBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<Transform, bool> validBones = avatarSetuMethodInfo.Invoke(null, new object[] { avatar.transform, false, null }) as Dictionary<Transform, bool>;

            if (validBones == null)
                return null;

            System.Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.AvatarAutoMapper");
            Reflection.MethodInfo methodInfo = toolbarType.GetMethod("MapBones", Reflection.BindingFlags.Public | Reflection.BindingFlags.Static);

            Dictionary<int, Transform> outData = methodInfo.Invoke(null, new object[] { avatar.transform, validBones }) as Dictionary<int, Transform>;

            return outData;
        }

        void AutoMapping()
        {
            Dictionary<int, Transform> outData = MapBones();

            if (outData == null)
                Debug.LogError("It is not possible to automatically fill in the skeleton map. Check the correctness of your model.");
            else
            {
                foreach (KeyValuePair<int, Transform> boneData in outData)
                {
                    JointType jointType = ((HumanBodyBones)boneData.Key).ToNuitrackJoint();

                    if (jointFieldMap.ContainsKey(jointType) && !excludeAutoFillJoints.Contains(jointType))
                    {
                        SerializedProperty jointProperty = GetJointProperty(jointType);

                        if (jointProperty.objectReferenceValue == null)
                            EditJoint(jointType, boneData.Value);
                    }
                }
            }
        }

        void Clear()
        {
            foreach (JointType jointType in jointFieldMap.Keys)
                EditJoint(jointType, null);
        }

        #endregion
    }
}