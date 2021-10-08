using UnityEngine;
using UnityEditor;

using System;
using System.IO;


namespace NuitrackSDKEditor
{
    /// <summary>
    /// Put the <see cref="GUI"/> block code in the using statement to color the <see cref="GUI"/> elements in the specified color
    /// After the using block, the <see cref="GUI"/> color will return to the previous one
    ///
    /// <example>
    /// This shows how to change the GUI color
    /// <code>
    /// using (new GUIColor(Color.green))
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class GUIColor : IDisposable
    {
        Color oldColor;

        public GUIColor(Color newColor)
        {
            oldColor = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = oldColor;
        }
    }

    /// <summary>
    /// Put the <see cref="Handles"/> block code in the using statement to color the <see cref="Handles"/> elements in the specified color
    /// After the using block, the <see cref="Handles"/> color will return to the previous one
    ///
    /// <example>
    /// This shows how to change the Handles color
    /// <code>
    /// using (new HandlesColor(Color.green))
    /// {
    ///     // Your Handles code ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class HandlesColor : IDisposable
    {
        Color oldColor;

        public HandlesColor(Color newColor)
        {
            oldColor = Handles.color;
            Handles.color = newColor;
        }

        public void Dispose()
        {
            Handles.color = oldColor;
        }
    }

    /// <summary>
    /// GUI draw helper class
    /// </summary>
    public static class NuitrackSDKGUI
    {
        /// <summary>
        /// Draw the "Help" button. Return a rectangle to draw your GUI element with an indent for the button.
        /// </summary>
        /// <param name="url">Click-through link</param>
        /// <param name="tooltip">(optional) ToolTip displayed when hovering over the button</param>
        /// <returns>Rectangle to draw your GUI element with an indent for the button.</returns>
        public static Rect WithHelpButton(string url, string tooltip = "")
        {
            GUIContent helpButton = EditorGUIUtility.IconContent("_Help");
            helpButton.tooltip = tooltip;

            Rect main = EditorGUILayout.GetControlRect();
            main.xMax -= helpButton.image.width;

            Rect helpButtonRect = new Rect(main.x + main.width, main.y, helpButton.image.width, main.height);

            if (GUI.Button(helpButtonRect, helpButton, GUIStyle.none))
                Application.OpenURL(url);

            return main;
        }

        /// <summary>
        /// Draw property with "Help" button.
        /// </summary>
        /// <param name="serializedObject">Target serialized object</param>
        /// <param name="propertyName">Name of object property</param>
        /// <param name="url">Click-through link</param>
        /// <param name="toolTip">(optional) ToolTip displayed when hovering over the button</param>
        public static void PropertyWithHelpButton(SerializedObject serializedObject, string propertyName, string url, string toolTip = "")
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            Rect propertyRect = WithHelpButton(url, toolTip);

            EditorGUI.PropertyField(propertyRect, property);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw a GUI block of the path to the file supplemented with the "Browse" and "Clear" buttons. 
        /// This element also provides a file selection dialog box.
        /// </summary>
        /// <param name="path">Current path to file</param>
        /// <param name="filterLabel">Filter label</param>
        /// <param name="extension">Filterable file extensions</param>
        /// <returns>Path to file</returns>
        public static string OpenFileField(string path, string filterLabel, params string[] extension)
        {
            GUIContent browseButtonContent = EditorGUIUtility.IconContent("Project");
            browseButtonContent.text = "Browse";

            GUIContent clearButtonContent = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
            clearButtonContent.text = "Clear";

            GUIContent errorMessage = EditorGUIUtility.IconContent("console.erroricon.sml");
            errorMessage.text = "Specified file was not found, check the correctness of the path";

            GUIContent warningMessage = EditorGUIUtility.IconContent("console.warnicon.sml");
            warningMessage.text = "Path is not specified";

            bool pathIsCorrect = File.Exists(path);

            Color color;

            if (path == string.Empty)
                color = Color.yellow;
            else if (!pathIsCorrect)
                color = Color.red;
            else
                color = Color.green;

            using (new GUIColor(color))
                GUILayout.BeginVertical(EditorStyles.helpBox);

            if (!pathIsCorrect || path == string.Empty)
            {
                GUIContent message = path == string.Empty ? warningMessage : errorMessage;
                GUILayout.Label(message, EditorStyles.wordWrappedLabel);
            }

            path = EditorGUILayout.TextField("Path to file", path);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(browseButtonContent))
            {
                string windowLabel = string.Format("Open {0} file", string.Join(", ", extension));
                string[] fileFilter = new string[]
                {
                    filterLabel,
                    string.Join(",", extension)
                };

                string newFilePath = EditorUtility.OpenFilePanelWithFilters(windowLabel, Application.dataPath, fileFilter);

                if (newFilePath != null && newFilePath != string.Empty)
                    path = newFilePath;
            }

            EditorGUI.BeginDisabledGroup(path == string.Empty);

            if (GUILayout.Button(clearButtonContent))
                path = string.Empty;

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            return path;
        }
    }
}