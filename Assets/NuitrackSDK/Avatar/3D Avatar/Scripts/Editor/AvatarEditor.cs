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
        readonly List<string> subExcludeFields = new List<string>()
        {
            "vrMode",
            "vrHead",
            "headTransform"
        };

        protected override void OnEnable()
        {
            base.OnEnable();

            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            List<JointType> avatarJoints = avatar.ModelJoints.Select(k => k.jointType).ToList();
  
            int index = 0;

            foreach (Styles.GUIBodyPart guiBodyPart in Styles.BodyParts.Values)
                foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    if (!avatarJoints.Contains(guiJoint.jointType))
                    {
                        ModelJoint modelJoint = new ModelJoint() { jointType = guiJoint.jointType };
                        avatar.ModelJoints.Insert(index, modelJoint);
                    }

                    index++;
                }
        }

        protected override List<string> GetExcludeFields()
        {
            List<string> excludeList = new List<string>();

            excludeList.AddRange(base.GetExcludeFields());
            excludeList.AddRange(subExcludeFields);

            return excludeList;
        }

        protected override void AddJoint(JointType jointType, Transform objectTransform)
        {
            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            Undo.RecordObject(avatar, "Avatar mapping modified");

            Dictionary<JointType, ModelJoint> jointsDict = avatar.ModelJoints.ToDictionary(k => k.jointType);
            jointsDict[jointType].bone = objectTransform;
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
        }
    }
}