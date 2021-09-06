using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using UnityEditor;
using UnityEditor.UI;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
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
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            base.OnInspectorGUI();

            if (FindObjectOfType<NuitrackManager>(true) == null)
                Helper.NuitrackNotExistMessage();

            if (nuitrackAspectRationFitter.aspectMode != AspectRatioFitter.AspectMode.FitInParent)
            {
                UnityAction fixAcpectMode = delegate { FixAspectMode(m_AspectMode); };

                string message = string.Format("Aspect Mode is set to {0}." +
                    "The frame from the sensor may not be displayed correctly." +
                    "\nRecommended: Fit In Parent.",
                    nuitrackAspectRationFitter.aspectMode);

                Helper.DrawMessage(message, LogType.Warning, fixAcpectMode, "Fix");        
            }

            serializedObject.ApplyModifiedProperties();
        }

        void FixAspectMode(SerializedProperty aspectmodeProperty)
        {
            aspectmodeProperty.enumValueIndex = (int)AspectRatioFitter.AspectMode.FitInParent;
        }
    }
}
