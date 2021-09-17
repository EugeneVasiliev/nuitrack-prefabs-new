using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;

namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(Avatar), true)]
    public class AvatarEditor : BaseAvatarEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            Avatar avatar = serializedObject.targetObject as Avatar;

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

        protected override void AddJoint(JointType jointType, Transform objectTransform, ref Dictionary<JointType, ModelJoint> jointsDict)
        {
            Avatar avatar = serializedObject.targetObject as Avatar;

            Undo.RecordObject(avatar, "Avatar mapping modified");
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