using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using Reflection = System.Reflection;

using NuitrackSDK;


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

    protected string[] GetNuitrackSDKInspectorFieldNames()
    {
        Reflection.FieldInfo[] fields = GetFieldInfo(target.GetType());

        string[] excludeFieldsNames = fields.Where(f => f.IsDefined(typeof(NuitrackSDKInspector), true)).Select(f => f.Name).ToArray();
        excludeFieldsNames = excludeFieldsNames.Append("m_Script").ToArray();

        return excludeFieldsNames;
    }

    new protected void DrawDefaultInspector()
    {
        string[] excludeFieldsNames = GetNuitrackSDKInspectorFieldNames();
        DrawPropertiesExcluding(serializedObject, excludeFieldsNames);
        serializedObject.ApplyModifiedProperties();
    }

    [Obsolete("Use this method only if the GUI elements are displayed incorrectly (it may cause duplication of some elements).", false)]
    protected void DrawDefaultUnityInspector()
    {
        base.DrawDefaultInspector();
    }
}
