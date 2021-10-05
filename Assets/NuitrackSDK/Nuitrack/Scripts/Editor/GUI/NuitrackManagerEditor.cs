using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(NuitrackManager), true)]
    public class NuitrackManagerEditor : NuitrackSDKEditorGUI
    {
        readonly Color warningColor = Color.yellow;
        readonly Color errorColor = Color.red;
        readonly Color okColor = Color.green;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawMirrorAndAngle();

            DrawRecordFileGUI();
        }

        #region Sensor mirror and angle

        void DrawMirrorAndAngle()
        {
            EditorGUILayout.LabelField("Sensor settings", EditorStyles.boldLabel);

            SerializedProperty mirrorProp = serializedObject.FindProperty("mirror");
            mirrorProp.boolValue = EditorGUILayout.Toggle("Mirror mode", mirrorProp.boolValue);
            serializedObject.ApplyModifiedProperties();

            SerializedProperty rotationProp = serializedObject.FindProperty("rotationDegree");

            if (mirrorProp.boolValue)
                rotationProp.enumValueIndex = 0;

            EditorGUI.BeginDisabledGroup(mirrorProp.boolValue);

            EditorGUILayout.PropertyField(rotationProp);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();

            //GUIContent helpButton = EditorGUIUtility.IconContent("_Help");
            //if(GUILayout.Button(helpButton, EditorStyles.miniButton))
            //{

            //}
        }

        #endregion

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