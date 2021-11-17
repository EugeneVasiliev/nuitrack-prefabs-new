using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(SensorSpace), true)]
    public class SensorSpace_Editor : NuitrackSDKEditor
    {
        SerializedProperty viewCanvasProperty;

        void OnEnable()
        {
            viewCanvasProperty = serializedObject.FindProperty("viewCanvas");
        }

        SerializedProperty DrawProperty(string nameProperty)
        {
            SerializedProperty property = serializedObject.FindProperty(nameProperty);
            EditorGUILayout.PropertyField(property);
            serializedObject.ApplyModifiedProperties();

            return property;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            if (FindObjectOfType<NuitrackManager>(true) == null)
                NuitrackSDKGUI.NuitrackNotExistMessage();

            SerializedProperty cameraFovAlign = DrawProperty("cameraFovAlign");

            if (cameraFovAlign.boolValue)
            {
                SensorSpace sensorSpace = serializedObject.targetObject as SensorSpace;
                Canvas canvas = viewCanvasProperty.objectReferenceValue as Canvas;

                if (canvas != null)
                {
                    if (canvas.renderMode == RenderMode.WorldSpace || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        string message = string.Format("The canvas rendering mode is specified: {0}.\nIt is recommended to use: {1}",
                           canvas.renderMode, RenderMode.ScreenSpaceCamera);

                        string buttonLabel = string.Format("Switch to {0}", RenderMode.ScreenSpaceCamera);

                        UnityAction fixAction = delegate { FixCanvasRenderMode(canvas, sensorSpace); };

                        NuitrackSDKGUI.DrawMessage(message, LogType.Warning, fixAction, buttonLabel);
                    }
                }
                else
                {
                    string message = "View Canvas is not set. The screen size will be used to align the camera's fov.";
                    NuitrackSDKGUI.DrawMessage(message, LogType.Log);
                }

                DrawProperty("viewCanvas");
            }

            SerializedProperty floorTracking = DrawProperty("floorTracking");

            if(floorTracking.boolValue)
            {
                DrawProperty("sensorSpace");

                DrawProperty("deltaHeight");
                DrawProperty("deltaAngle");
                DrawProperty("floorCorrectionSpeed");
            }
        }

        void FixCanvasRenderMode(Canvas canvas, SensorSpace sensorSpace)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = sensorSpace.Camera;
            canvas.planeDistance = 15;
        }
    }
}
