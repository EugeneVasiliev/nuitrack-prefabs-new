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
        protected override void OnEnable()
        {
            base.OnEnable();

            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            List<JointType> avatarJoints = avatar.ModelJoints.Select(k => k.jointType).ToList();
  
            int index = 0;

            foreach (Styles.GUIBodyPart guiBodyPart in Styles.BodyParts.Values)
                foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    if (!avatarJoints.Contains(guiJoint.JointType))
                    {
                        ModelJoint modelJoint = new ModelJoint() { jointType = guiJoint.JointType };
                        avatar.ModelJoints.Insert(index, modelJoint);
                    }

                    index++;
                }
        }

        protected override void AddJoint(JointType jointType, Transform objectTransform)
        {
            NuitrackSDK.Avatar.Avatar avatar = serializedObject.targetObject as NuitrackSDK.Avatar.Avatar;

            Undo.RecordObject(avatar, "Avatar mapping modified");

            Dictionary<JointType, ModelJoint> jointsDict = avatar.ModelJoints.ToDictionary(k => k.jointType);
            jointsDict[jointType].bone = objectTransform;
        }

        protected override void RemoveJoint(JointType jointType)
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
        }
    }
}