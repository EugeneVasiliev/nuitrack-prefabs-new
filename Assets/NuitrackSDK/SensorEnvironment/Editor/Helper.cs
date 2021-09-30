using UnityEngine;

using UnityEditor;
using UnityEngine.Events;

using System.Collections.Generic;


namespace NuitrackSDKEditor
{
    public static class Helper
    {
        static Dictionary<LogType, Color> messageColors = new Dictionary<LogType, Color>()
        {
            { LogType.Warning, Color.yellow },
            { LogType.Log, Color.white },
            { LogType.Error, Color.red },
            { LogType.Assert, Color.red },
            { LogType.Exception, Color.red }
        };

        public static void NuitrackNotExistMessage()
        {
            DrawMessage("Make sure that when the script is running, the NuitrackScripts prefab will be on the scene.", LogType.Warning);
        }

        public static void DrawMessage(string message, LogType messageType, UnityAction fixAction = null, string fixButtonLabel = null)
        {
            EditorGUILayout.Space();

            using (new GUIColor(messageColors[messageType]))
                GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);

            if (fixAction != null)
                if (GUILayout.Button(fixButtonLabel))
                    fixAction.Invoke();

            GUILayout.EndVertical();
        }
    }
}