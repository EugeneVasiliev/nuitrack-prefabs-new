using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
using UnityEditor.UI;

namespace NuitrackSDK.Frame.Editor
{
    [CustomEditor(typeof(NuitrackAspectRationFitter), true)]
    public class NuitrackAspectRationFitter_Editor : AspectRatioFitterEditor
    {
        SerializedProperty m_FrameMode;
        SerializedProperty m_AspectMode;

        NuitrackAspectRationFitter nuitrackAspectRationFitter;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_FrameMode = serializedObject.FindProperty("frameMode");
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");

            nuitrackAspectRationFitter = target as NuitrackAspectRationFitter;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_FrameMode);  
            EditorGUILayout.Space();

            base.OnInspectorGUI();

            if (FindObjectOfType<NuitrackManager>(true) == null)
            {
                EditorGUILayout.Space();

                Color oldColor = GUI.color;

                GUI.color = Color.red;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = oldColor;

                EditorGUILayout.LabelField("NuitrackManager (NuitrackScripts object) is not found on the scene. " +
                    "Add prefab NuitrackScripts for the component to work.", EditorStyles.wordWrappedLabel);

                EditorGUILayout.EndVertical();
            }

            if(nuitrackAspectRationFitter.aspectMode != UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent)
            {
                EditorGUILayout.Space();

                Color oldColor = GUI.color;

                GUI.color = Color.yellow;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = oldColor;

                string message = "Aspect Mode is set to {0}." +
                    "The frame from the sensor may not be displayed correctly." +
                    "\nRecommended parameter: Fit In Parent.";

                EditorGUILayout.LabelField(string.Format(message, nuitrackAspectRationFitter.aspectMode), EditorStyles.wordWrappedLabel);
                
                if(GUILayout.Button("Fix"))
                    m_AspectMode.enumValueIndex = (int)AspectRatioFitter.AspectMode.FitInParent;

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
