using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(NuitrackSDK.Avatar.Avatar), true)]
    public class AvatarEditor : BaseAvatarEditor
    {
        SkeletonMapperGUI<Transform> skeletonMapper = null;
        SkeletonMapperListGUI<Transform> skeletonJointListUI = null;
        SkeletonBonesView skeletonBonesView = null;

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

        bool ShowSkeletonView
        {
            get
            {
                return EditorPrefs.GetBool(target.GetType().FullName + "showSkeletonView", true);
            }
            set
            {
                EditorPrefs.SetBool(target.GetType().FullName + "showSkeletonView", value);
            }
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

            skeletonBonesView = new SkeletonBonesView();
        }

        protected virtual void OnDisable()
        {
            skeletonMapper.onDrop -= SkeletonMapper_onDrop;
            skeletonMapper.onSelected -= SkeletonMapper_onSelected;
            skeletonMapper = null;

            skeletonJointListUI.onDrop -= SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected -= SkeletonMapper_onSelected;
            skeletonJointListUI = null;

            skeletonBonesView = null;
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

        void OnSceneGUI()
        {
            if (ShowSkeletonView)
            {
                NuitrackSDK.Avatar.Avatar avatar = target as NuitrackSDK.Avatar.Avatar;

                List<Transform> jointDict = jointFieldMap.Keys.Where(k => GetTransformFromField(k) != null).Select(v => GetTransformFromField(v)).ToList();
                skeletonBonesView.DrawSkeleton(avatar.transform, jointDict);
            }
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

            bool newVal = EditorGUILayout.Toggle("Show unused bone", ShowSkeletonView);

            if(newVal != ShowSkeletonView)
            {
                ShowSkeletonView = newVal;
                SceneView.RepaintAll();
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

        void AutoMapping()
        {
            NuitrackSDK.Avatar.Avatar avatar = target as NuitrackSDK.Avatar.Avatar;

            Dictionary<HumanBodyBones, Transform> outData = SkeletonUtils.MapBones(avatar.transform);

            if (outData == null || outData.Count == 0)
                Debug.LogError("It is not possible to automatically fill in the skeleton map. Check the correctness of your model.");
            else
            {
                foreach (KeyValuePair<HumanBodyBones, Transform> boneData in outData)
                {
                    JointType jointType = boneData.Key.ToNuitrackJoint();

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