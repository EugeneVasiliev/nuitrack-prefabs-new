using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using Reflection = System.Reflection;

using NuitrackSDK;


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

    public abstract class NuitrackSDKEditorGUI : Editor
    {
        Reflection.FieldInfo[] GetFieldInfo(Type typeObject)
        {
            Reflection.FieldInfo[] fields = typeObject.GetFields(
                Reflection.BindingFlags.Instance |
                Reflection.BindingFlags.NonPublic |
                Reflection.BindingFlags.Public);

            if (typeObject.BaseType != typeof(MonoBehaviour))
            {
                Reflection.FieldInfo[] baseTypeFields = GetFieldInfo(typeObject.BaseType);
                fields = fields.Concat(baseTypeFields).ToArray();
            }

            return fields;
        }

        /// <summary>
        /// Get the names of fields marked with <see cref="NuitrackSDKInspector"/> attribute
        /// </summary>
        /// <returns>Array of strings</returns>
        protected string[] GetNuitrackSDKInspectorFieldNames()
        {
            Reflection.FieldInfo[] fields = GetFieldInfo(target.GetType());

            string[] excludeFieldsNames = fields.Where(f => f.IsDefined(typeof(NuitrackSDKInspector), true)).Select(f => f.Name).ToArray();
            excludeFieldsNames = excludeFieldsNames.Append("m_Script").ToArray();

            return excludeFieldsNames;
        }

        /// <summary>
        /// Draw the inspector. 
        /// Fields marked with the attribute <see cref="NuitrackSDKInspector"/> 
        /// will not be drawn in the inheritors of <see cref="NuitrackSDKEditorGUI"/> . 
        /// </summary>
        new protected void DrawDefaultInspector()
        {
            string[] excludeFieldsNames = GetNuitrackSDKInspectorFieldNames();
            DrawPropertiesExcluding(serializedObject, excludeFieldsNames);
            serializedObject.ApplyModifiedProperties();
        }

        [Obsolete("Use this method only if the GUI elements are displayed incorrectly (it may cause duplication of some elements). Use DrawDefaultInspector()", false)]
        protected void DrawDefaultUnityInspector()
        {
            base.DrawDefaultInspector();
        }
    }
}