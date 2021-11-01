using UnityEditor;
using UnityEngine;

using NuitrackSDK;


namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(UserTracker), true)]
    public class UserTrackerEditor : NuitrackSDKEditor
    {
        public override void OnInspectorGUI()
        {
            DrawUserTrackintSettings();
            DrawDefaultInspector();
        }

        public virtual new void DrawDefaultInspector()
        {
            base.DrawDefaultInspector();
        }

        /// <summary>
        /// Draw basic avatar settings
        /// </summary>
        protected void DrawUserTrackintSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("User tracking options", EditorStyles.boldLabel);

            SerializedProperty useCurrentUserTracker = serializedObject.FindProperty("useCurrentUserTracker");
            EditorGUILayout.PropertyField(useCurrentUserTracker, new GUIContent("Use current user tracker"));
            serializedObject.ApplyModifiedProperties();

            if (!useCurrentUserTracker.boolValue)
            {
                SerializedProperty userID = serializedObject.FindProperty("userID");
                userID.intValue = EditorGUILayout.IntSlider("User ID", userID.intValue, Users.MinID, Users.MaxID);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}