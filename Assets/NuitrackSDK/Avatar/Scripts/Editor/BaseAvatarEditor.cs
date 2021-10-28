﻿using UnityEditor;

using nuitrack;
using NuitrackSDK.Avatar;


namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(BaseAvatar), true)]
    public class BaseAvatarEditor : NuitrackSDKEditor
    {
        protected virtual JointType SelectJoint { get; set; } = JointType.None;

        public override void OnInspectorGUI()
        {
            DrawSkeletonSettings();
            DrawDefaultInspector();

            DrawAvatarGUI();
        }

        /// <summary>
        /// Draw basic avatar settings
        /// </summary>
        protected void DrawSkeletonSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skeleton settings", EditorStyles.boldLabel);

            SerializedProperty useCurrentUserTracker = serializedObject.FindProperty("useCurrentUserTracker");
            useCurrentUserTracker.boolValue = EditorGUILayout.Toggle("Use current user tracker", useCurrentUserTracker.boolValue);
            serializedObject.ApplyModifiedProperties();

            if (!useCurrentUserTracker.boolValue)
            {
                SerializedProperty userID = serializedObject.FindProperty("userID");
                userID.intValue = EditorGUILayout.IntSlider("User ID", userID.intValue, Users.MinID, Users.MaxID);
                serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty jointConfidence = serializedObject.FindProperty("jointConfidence");
            jointConfidence.floatValue = EditorGUILayout.Slider("Joint confidence", jointConfidence.floatValue, 0, 1);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Override this method to add your own settings and parameters in the Inspector.
        /// </summary>
        protected virtual void DrawAvatarGUI() { }      
    }
}