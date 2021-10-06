using UnityEngine;
using UnityEditor;

using System;


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

    public static class NuitrackSDKGUI
    {
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

        public static void PropertyWithHelpButton(SerializedObject serializedObject, string propertyName, string url, string toolTip = "")
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            Rect propertyRect = WithHelpButton(url, toolTip);

            EditorGUI.PropertyField(propertyRect, property);
            serializedObject.ApplyModifiedProperties();
        }
    }
}