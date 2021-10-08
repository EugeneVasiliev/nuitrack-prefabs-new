using UnityEngine;
using UnityEditor;

using UnityEditor.SceneManagement;


namespace NuitrackSDKEditor.Readme
{
    [CustomEditor(typeof(NuitrackReadme), true)]
    public class NuitrackReadmeEditor : NuitrackSDKEditor
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

        void DrawButton(string url, string label, string icon, GUIStyle style)
        {
            GUIContent videoButtonContent = EditorGUIUtility.IconContent(icon);
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(url == null || url == string.Empty);

            if (GUILayout.Button(videoButtonContent, style))
                Application.OpenURL(url);

            EditorGUI.EndDisabledGroup();
        }

        void DrawButton(SceneAsset scene, string label, GUIStyle style)
        {
            GUIContent videoButtonContent = EditorGUIUtility.IconContent("d_animationvisibilitytoggleon");
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(scene == null);

            if (GUILayout.Button(videoButtonContent, style))
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
                Texture previewImage = tutorialItem.FindPropertyRelative("previewImage").objectReferenceValue as Texture;
                string label = tutorialItem.FindPropertyRelative("label").stringValue;

                using (new VecrticalGroup(EditorStyles.helpBox))
                {
                    using (new VecrticalGroup())
                    {
                        using (new HorizontalGroup())
                        {
                            float maxWidth = (tutorailItemHeight / previewImage.height) * previewImage.width;
                            GUILayout.Box(previewImage, GUILayout.Height(tutorailItemHeight), GUILayout.Width(maxWidth));


                            EditorGUILayout.LabelField(label, LabelStyle);
                        }

                        EditorGUILayout.Space();

                        using (new HorizontalGroup())
                        {
                            string textURL = tutorialItem.FindPropertyRelative("textURL").stringValue;
                            string videoURL = tutorialItem.FindPropertyRelative("videoURL").stringValue;
                            SceneAsset scene = tutorialItem.FindPropertyRelative("scene").objectReferenceValue as SceneAsset;

                            DrawButton(textURL, "Text", "PreTexA@2x", EditorStyles.miniButtonLeft);
                            DrawButton(videoURL, "Video", "Profiler.Video@2x", EditorStyles.miniButtonMid);
                            DrawButton(scene, "Scene", EditorStyles.miniButtonRight);
                        }

                    }
                }
            }
        }
    }
}
