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

        readonly Dictionary<JointType, string> jointFieldMap = new Dictionary<JointType, string>()
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

        bool ShowUnusedBone
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

        protected override JointType SelectJoint 
        {
            get 
            {
                return base.SelectJoint;
            }
            set
            {
                base.SelectJoint = value;

                if(skeletonMapper != null)
                    skeletonMapper.SelectedJoint = SelectJoint;

                if (skeletonJointListUI != null)
                    skeletonJointListUI.SelectedJoint = SelectJoint;
            }
        }

        protected virtual void OnEnable()
        {
            List<JointType> jointMask = jointFieldMap.Keys.ToList();

            skeletonMapper = new SkeletonMapperGUI<Transform>(jointMask);
            skeletonMapper.OnDrop += SkeletonMapper_onDrop;
            skeletonMapper.OnSelected += SkeletonMapper_onSelected;

            skeletonJointListUI = new SkeletonMapperListGUI<Transform>(jointMask);
            skeletonJointListUI.OnDrop += SkeletonMapper_onDrop;
            skeletonJointListUI.OnSelected += SkeletonMapper_onSelected;

            skeletonBonesView = new SkeletonBonesView();
        }

        protected virtual void OnDisable()
        {
            skeletonMapper.OnDrop -= SkeletonMapper_onDrop;
            skeletonMapper.OnSelected -= SkeletonMapper_onSelected;
            skeletonMapper = null;

            skeletonJointListUI.OnDrop -= SkeletonMapper_onDrop;
            skeletonJointListUI.OnSelected -= SkeletonMapper_onSelected;
            skeletonJointListUI = null;

            skeletonBonesView = null;
        }

        void SkeletonMapper_onDrop(Transform newJoint, JointType jointType)
        {
            if (!jointFieldMap.ContainsKey(jointType))
                return;

            EditJoint(jointType, newJoint);

            if (newJoint != null)
            {
                EditorGUIUtility.PingObject(newJoint);
                SelectJoint = jointType;
            }
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            if (!jointFieldMap.ContainsKey(jointType))
                return;

            SelectJoint = jointType;

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
            if (ShowUnusedBone)
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

            IEnumerable<JointType> activeJoints = jointFieldMap.Keys.Where(k => GetTransformFromField(k) != null);

            if (skeletonMapper != null)
                skeletonMapper.Draw(activeJoints.ToList());

            bool newUnusedBoneVal = EditorGUILayout.Toggle("Show unused bone", ShowUnusedBone);

            if(newUnusedBoneVal != ShowUnusedBone)
            {
                ShowUnusedBone = newUnusedBoneVal;
                SceneView.RepaintAll();
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (GUILayout.Button("Automap"))
                AutoMapping();

            bool disableClearbutton = activeJoints.Count() == 0;

            EditorGUI.BeginDisabledGroup(disableClearbutton);
            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Skeleton map", "Do you really want to clear the skeleton map?", "Yes", "No"))
                    Clear();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (skeletonJointListUI != null)
            {
                Dictionary<JointType, Transform> jointDict = activeJoints.ToDictionary(k => k, v => GetTransformFromField(v));           
                skeletonJointListUI.Draw(jointDict);
            }
        }

        #region AutoMapping

        readonly List<JointType> excludeAutoFillJoints = new List<JointType>()
        {
            JointType.LeftCollar,
            JointType.RightCollar
        };

        void AutoMapping()
        {
            NuitrackSDK.Avatar.Avatar avatar = target as NuitrackSDK.Avatar.Avatar;

            Dictionary<HumanBodyBones, Transform> skeletonBonesMap = SkeletonUtils.GetBonesMap(avatar.transform);

            if (skeletonBonesMap == null || skeletonBonesMap.Count == 0)
            {
                Debug.LogError("It is not possible to automatically fill in the skeleton map. Check the correctness of your model.");
                return;
            }

            List<HumanBodyBones> failFoundBones = new List<HumanBodyBones>();

            foreach (JointType jointType in jointFieldMap.Keys)
            {
                HumanBodyBones humanBodyBones = jointType.ToUnityBones();

                if (GetTransformFromField(jointType) == null)
                {
                    if (excludeAutoFillJoints.Contains(jointType) || !skeletonBonesMap.ContainsKey(humanBodyBones))
                        failFoundBones.Add(humanBodyBones);
                    else
                        EditJoint(jointType, skeletonBonesMap[humanBodyBones]);
                }
            }

            if (failFoundBones.Count > 0)
                Debug.Log(string.Format("For bones: <color=orange><b>{0}</b></color>, could not be found object Transforms", string.Join(", ", failFoundBones)));
        }

        void Clear()
        {
            foreach (JointType jointType in jointFieldMap.Keys)
                EditJoint(jointType, null);
        }

        #endregion

    }
}