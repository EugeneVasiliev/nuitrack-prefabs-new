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

        protected virtual void OnEnable()
        {
            skeletonMapper = new SkeletonMapperGUI<Transform>();
            skeletonMapper.onDrop += SkeletonMapper_onDrop;
            skeletonMapper.onSelected += SkeletonMapper_onSelected;

            skeletonJointListUI = new SkeletonMapperListGUI<Transform>();
            skeletonJointListUI.onDrop += SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected += SkeletonMapper_onSelected;

            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            List<JointType> avatarJoints = avatar.ModelJoints.Select(k => k.jointType).ToList();
  
            int index = 0;

            foreach (SkeletonMapperStyles.GUIBodyPart guiBodyPart in SkeletonMapperStyles.BodyParts.Values)
                foreach (SkeletonMapperStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    if (!avatarJoints.Contains(guiJoint.JointType))
                    {
                        ModelJoint modelJoint = new ModelJoint() { jointType = guiJoint.JointType };
                        avatar.ModelJoints.Insert(index, modelJoint);
                    }

                    index++;
                }
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
            if (newJoint != null)
            {
                AddJoint(jointType, newJoint);

                EditorGUIUtility.PingObject(newJoint);
                SelectJoint = jointType;

                skeletonMapper.SelectJoint = SelectJoint;
                skeletonJointListUI.SelectJoint = SelectJoint;
            }
            else
                RemoveJoint(jointType);
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            SelectJoint = jointType;
            skeletonMapper.SelectJoint = SelectJoint;
            skeletonJointListUI.SelectJoint = SelectJoint;

            Dictionary<JointType, ModelJoint> jointsDict = avatar.ModelJoints.ToDictionary(k => k.jointType);

            if (jointsDict[jointType].bone != null)
                EditorGUIUtility.PingObject(jointsDict[jointType].bone);
        }

        protected virtual void AddJoint(JointType jointType, Transform objectTransform)
        {
            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            Undo.RecordObject(avatar, "Avatar mapping modified");

            Dictionary<JointType, ModelJoint> jointsDict = avatar.ModelJoints.ToDictionary(k => k.jointType);
            jointsDict[jointType].bone = objectTransform;
        }

        protected virtual void RemoveJoint(JointType jointType)
        {
            NuitrackSDK.Avatar.Avatar myScript = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            ref List<ModelJoint> modelJoints = ref myScript.ModelJoints;
            Dictionary<JointType, ModelJoint> jointsDict = modelJoints.ToDictionary(k => k.jointType);

            if (jointsDict.ContainsKey(jointType))
            {
                Undo.RecordObject(myScript, "Remove avatar joint object");
                jointsDict[jointType].bone = null;
            }
        }

        protected override void DrawSubAvatarGUI()
        {
            EditorGUILayout.Space();

            SerializedProperty vrModeProperty = serializedObject.FindProperty("vrMode");
            vrModeProperty.boolValue = EditorGUILayout.Toggle("VR mode", vrModeProperty.boolValue);
            serializedObject.ApplyModifiedProperties();

            if(vrModeProperty.boolValue)
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
                List<JointType> activeJoints = myScript.ModelJoints.Where(j => j.bone != null).Select(j => j.jointType).ToList();
                skeletonMapper.Draw(activeJoints);
            }

            if (skeletonJointListUI != null)
            {
                Dictionary<JointType, Transform> jointDict = myScript.ModelJoints.Where(v => v.bone != null).ToDictionary(k => k.jointType, v => v.bone);
                skeletonJointListUI.Draw(jointDict);
            }
        }
    }
}