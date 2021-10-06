using UnityEngine;
using UnityEditor;

using System.IO;


namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(NuitrackManager), true)]
    public class NuitrackManagerEditor : NuitrackSDKEditor
    {
        readonly Color warningColor = Color.yellow;
        readonly Color errorColor = Color.red;
        readonly Color okColor = Color.green;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawConfigState();

            DrawSensorOptions();

            DrawRecordFileGUI();
        }

        void DrawConfigState()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "wifiConnect",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case",
                "Only skeleton. PC, Unity Editor, MacOS and IOS");


            NuitrackSDKGUI.PropertyWithHelpButton(
                serializedObject,
                "useNuitrackAi",
                "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md",
                "ONLY PC! Nuitrack AI is the new version of Nuitrack skeleton tracking middleware");

            NuitrackSDKGUI.PropertyWithHelpButton(
                 serializedObject,
                 "useFaceTracking",
                 "https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Unity_Face_Tracking.md",
                 "Track and get information about faces with Nuitrack (position, angle of rotation, box, emotions, age, gender)");
        }

        void DrawSensorOptions()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sensor options", EditorStyles.boldLabel);

            SerializedProperty depth2ColorRegistration = serializedObject.FindProperty("depth2ColorRegistration");
            EditorGUILayout.PropertyField(depth2ColorRegistration);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty mirrorProp = serializedObject.FindProperty("mirror");
            mirrorProp.boolValue = EditorGUILayout.Toggle("Mirror mode", mirrorProp.boolValue);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty sensorRotation = serializedObject.FindProperty("sensorRotation");

            if (mirrorProp.boolValue)
                sensorRotation.enumValueIndex = 0;

            EditorGUI.BeginDisabledGroup(mirrorProp.boolValue);

            EditorGUILayout.PropertyField(sensorRotation);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }


        #region Record file

        readonly string[] fileFilter = new string[] { "Bag or oni file", "bag,oni" };

        GUIContent WarningMessage
        {
            get
            {
                GUIContent message = EditorGUIUtility.IconContent("console.warnicon.sml");
                message.text = "Path is not specified";

                return message;
            }
        }

        GUIContent ErrorMessage
        {
            get
            {
                GUIContent message = EditorGUIUtility.IconContent("console.erroricon.sml");
                message.text = "Specified file was not found, check the correctness of the path";

                return message;
            }
        }

        GUIContent ClearFile
        {
            get
            {
                GUIContent message = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
                message.text = "Clear";

                return message;
            }
        }

        void DrawRecordFileGUI()
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);

            bool useFile = serializedObject.FindProperty("useFileRecord").boolValue;
            string path = serializedObject.FindProperty("pathToFileRecord").stringValue;
            bool pathIsCorrect = File.Exists(path);

            if (useFile)
            {
                Color color;

                if (path == string.Empty)
                    color = warningColor;
                else if (!pathIsCorrect)
                    color = errorColor;
                else
                    color = okColor;

                using (new GUIColor(color))
                    GUILayout.BeginVertical(EditorStyles.helpBox);
            }

            SerializedProperty useFileRecordProp = serializedObject.FindProperty("useFileRecord");
            useFileRecordProp.boolValue = EditorGUILayout.Toggle("Use record file", useFileRecordProp.boolValue);
            serializedObject.ApplyModifiedProperties();

            if(useFileRecordProp.boolValue)
            {
                SerializedProperty pathtoFileProp = serializedObject.FindProperty("pathToFileRecord");

                if (!pathIsCorrect || path == string.Empty)
                {
                    GUIContent message = path == string.Empty ? WarningMessage : ErrorMessage;
                    GUILayout.Label(message, EditorStyles.wordWrappedLabel);
                }

                pathtoFileProp.stringValue = EditorGUILayout.TextField("Path to file record", pathtoFileProp.stringValue);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Browse"))
                {
                    string newFilePath = EditorUtility.OpenFilePanelWithFilters("Open *.oni or *.bag file", Application.dataPath, fileFilter);

                    if(newFilePath != null && newFilePath != string.Empty)
                        pathtoFileProp.stringValue = newFilePath;
                }
                EditorGUI.BeginDisabledGroup(pathtoFileProp.stringValue == string.Empty);

                if (GUILayout.Button(ClearFile))
                    pathtoFileProp.stringValue = string.Empty;

                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                GUILayout.EndHorizontal();
            }

            if (useFile)
                GUILayout.EndVertical();
        }

        #endregion
    }
}