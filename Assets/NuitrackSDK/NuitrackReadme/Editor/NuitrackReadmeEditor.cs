using UnityEngine;
using UnityEditor;

using UnityEditor.SceneManagement;
using NuitrackSDKEditor.Readme;

namespace NuitrackSDKEditor.Readme
{ 
    [CustomEditor(typeof(NuitrackReadme), true)]
    public class NuitrackReadmeEditor : NuitrackSDKEditorGUI
    {
        float tutorailItemHeight = 96;

        GUIStyle HeaderLabelStyle
        {
            get
            {
                GUIStyle headerLabelStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    fontSize = 16
                };

                return headerLabelStyle;
            }
        }

        GUIStyle LabelStyle
        {
            get
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
                {
                    alignment = TextAnchor.UpperLeft,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    wordWrap = true
                };

                return labelStyle;
            }
        }

        void DrawButton(Rect rect, string url, string label, string icon)
        {
            GUIContent videoButtonContent = EditorGUIUtility.IconContent(icon);
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(url == null || url == string.Empty);

            if (GUI.Button(rect, videoButtonContent))
                Application.OpenURL(url);

            EditorGUI.EndDisabledGroup();
        }

        void DrawButton(Rect rect, SceneAsset scene, string label)
        {
            GUIContent videoButtonContent = EditorGUIUtility.ObjectContent(null, typeof(SceneAsset));
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(scene == null);

            if (GUI.Button(rect, videoButtonContent))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                string scenePath = AssetDatabase.GetAssetPath(scene);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            EditorGUI.EndDisabledGroup();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Nuitrack tutorials", HeaderLabelStyle);
            EditorGUILayout.Space();

            SerializedProperty tutorialsItems = serializedObject.FindProperty("tutorialItems");

            foreach (SerializedProperty tutorialItem in tutorialsItems)
            {
                Rect controlRect = EditorGUILayout.GetControlRect(false, tutorailItemHeight);

                Texture previewImage = tutorialItem.FindPropertyRelative("previewImage").objectReferenceValue as Texture;
                string label = tutorialItem.FindPropertyRelative("label").stringValue;

                GUI.Box(controlRect, previewImage, EditorStyles.helpBox);

                Rect upSideRect = new Rect(controlRect);
                upSideRect.xMin += (tutorailItemHeight / previewImage.height) * previewImage.width;
                upSideRect.yMax -= controlRect.height / 2;

                EditorGUI.LabelField(upSideRect, label, LabelStyle);


                Rect downSideRect = new Rect(controlRect);
                downSideRect.xMin += (tutorailItemHeight / previewImage.height) * previewImage.width;
                downSideRect.yMin += (controlRect.height / 3) * 2;

                string textURL = tutorialItem.FindPropertyRelative("textURL").stringValue;

                Rect textUrlRect = new Rect(downSideRect);
                textUrlRect.xMax -= (downSideRect.width / 3) * 2;

                DrawButton(textUrlRect, textURL, "Text", "PreTexA@2x");

                string videoURL = tutorialItem.FindPropertyRelative("videoURL").stringValue;

                Rect videoUrlRect = new Rect(downSideRect);
                videoUrlRect.xMin += (downSideRect.width / 3);
                videoUrlRect.xMax -= (downSideRect.width / 3);

                DrawButton(videoUrlRect, videoURL, "Video", "Profiler.Video@2x");

                SceneAsset scene = tutorialItem.FindPropertyRelative("scene").objectReferenceValue as SceneAsset;

                Rect openSceneRect = new Rect(downSideRect);
                openSceneRect.xMin += (downSideRect.width / 3) * 2;

                DrawButton(openSceneRect, scene, "Scene");
            }
        }
    }
}
