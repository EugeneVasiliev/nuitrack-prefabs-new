using UnityEngine;
using UnityEditor;

using UnityEditor.SceneManagement;

// Menu Item Template
// +-------------+-----------------------------+
// |             | Label                       |
// |    Image    |                             |
// |             | Description                 |
// |             |                             |
// +-------------+--------------+--------------+
// | Text button | Video button | Scene button |
// +-------------+--------------+--------------+


namespace NuitrackSDKEditor.Readme
{
    [CustomEditor(typeof(NuitrackTutorials), true)]
    public class NuitrackTutorialsEditor : NuitrackSDKEditor
    {
        const float tutorailItemHeight = 82;
        const int maxDescriptionCharacresCount = 200;

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

        bool DrawToSceneButton(SceneAsset scene, string label, string icon, GUIStyle style)
        {
            GUIContent videoButtonContent = EditorGUIUtility.IconContent(icon);
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(scene == null);

            if (GUILayout.Button(videoButtonContent, style))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                string scenePath = AssetDatabase.GetAssetPath(scene);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Object obj = AssetDatabase.LoadAssetAtPath(scenePath, typeof(Object));
                Selection.activeObject = obj;

                return true;
            }

            EditorGUI.EndDisabledGroup();

            return false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Nuitrack tutorials", HeaderLabelStyle);
            EditorGUILayout.Space();

            SerializedProperty tutorialsItems = serializedObject.FindProperty("tutorialItems");

            foreach (SerializedProperty tutorialItem in tutorialsItems)
            {
                // Content item
                using (new VerticalGroup(EditorStyles.helpBox))
                {
                    // Content & button plane
                    using (new VerticalGroup())
                    {
                        // Content
                        using (new HorizontalGroup())
                        {
                            Texture previewImage = tutorialItem.FindPropertyRelative("previewImage").objectReferenceValue as Texture;
                            previewImage = previewImage ?? EditorGUIUtility.IconContent("d_SceneViewVisibility@2x").image;

                            float maxWidth = (tutorailItemHeight / previewImage.height) * previewImage.width;
                            GUILayout.Box(previewImage, GUILayout.Height(tutorailItemHeight), GUILayout.Width(maxWidth));

                            // Label & description
                            using (new VerticalGroup())
                            {
                                string label = tutorialItem.FindPropertyRelative("label").stringValue;

                                EditorGUILayout.LabelField(label, LabelStyle);

                                string description = tutorialItem.FindPropertyRelative("description").stringValue;

                                if(description.Length > maxDescriptionCharacresCount)
                                    description = description.Substring(0, maxDescriptionCharacresCount) + "...";

                                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
                            }
                        }

                        GUILayout.Space(10);

                        // Button plane
                        using (new HorizontalGroup())
                        {
                            string textURL = tutorialItem.FindPropertyRelative("textURL").stringValue;
                            string videoURL = tutorialItem.FindPropertyRelative("videoURL").stringValue;
                            SceneAsset scene = tutorialItem.FindPropertyRelative("scene").objectReferenceValue as SceneAsset;

                            DrawButton(textURL, "Text", "d_UnityEditor.ConsoleWindow@2x", EditorStyles.miniButtonLeft);
                            DrawButton(videoURL, "Video", "d_UnityEditor.Timeline.TimelineWindow", EditorStyles.miniButtonMid);

                            if (DrawToSceneButton(scene, "Scene", "d_animationvisibilitytoggleon", EditorStyles.miniButtonRight))
                                break;
                        }

                        GUILayout.Space(5);
                    }
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("See more on GitHub page"))
                Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc");
        }
    }
}
