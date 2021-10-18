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


namespace NuitrackSDKEditor.Documentation
{
    public class NuitrackTutorialsEditorWindow : EditorWindow
    {
        const float tutorailItemHeight = 82;
        const int maxDescriptionCharacresCount = 200;

        static Vector2 scrollPos = Vector2.zero;

        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            NuitrackTutorialsEditorWindow window = (NuitrackTutorialsEditorWindow)EditorWindow.GetWindow(typeof(NuitrackTutorialsEditorWindow));
            window.Show();
        }

        void OnGUI()
        {
            DrawTutorials();
        }

        static GUIStyle HeaderLabelStyle
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

        static GUIStyle LabelStyle
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

        static void DrawButton(string url, string label, string icon, GUIStyle style)
        {
            GUIContent videoButtonContent = EditorGUIUtility.IconContent(icon);
            videoButtonContent.text = label;

            EditorGUI.BeginDisabledGroup(url == null || url == string.Empty);

            if (GUILayout.Button(videoButtonContent, style))
                Application.OpenURL(url);

            EditorGUI.EndDisabledGroup();
        }

        static bool DrawToSceneButton(SceneAsset scene, string label, string icon, GUIStyle style)
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
   
        public static void DrawTutorials()
        {
            EditorGUILayout.LabelField("Nuitrack tutorials", HeaderLabelStyle);
            EditorGUILayout.Space();

            NuitrackTutorials tutorials = (NuitrackTutorials)AssetDatabase.LoadAssetAtPath("Assets/NuitrackSDK/TUTORIALS.asset", typeof(NuitrackTutorials));

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            foreach (NuitrackTutorials.TutorialItem tutorialItem in tutorials.TutorialItems)
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
                            Texture previewImage = tutorialItem.PreviewImage;
                            previewImage = previewImage ?? EditorGUIUtility.IconContent("d_SceneViewVisibility@2x").image;

                            float maxWidth = (tutorailItemHeight / previewImage.height) * previewImage.width;
                            GUILayout.Box(previewImage, GUILayout.Height(tutorailItemHeight), GUILayout.Width(maxWidth));

                            // Label & description
                            using (new VerticalGroup())
                            {
                                EditorGUILayout.LabelField(tutorialItem.Label, LabelStyle);

                                string description = tutorialItem.Description;

                                if (description.Length > maxDescriptionCharacresCount)
                                    description = description.Substring(0, maxDescriptionCharacresCount) + "...";

                                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
                            }
                        }

                        GUILayout.Space(10);

                        // Button plane
                        using (new HorizontalGroup())
                        {
                            DrawButton(tutorialItem.TextURL, "Text", "d_UnityEditor.ConsoleWindow@2x", EditorStyles.miniButtonLeft);
                            DrawButton(tutorialItem.VideoURL, "Video", "d_UnityEditor.Timeline.TimelineWindow", EditorStyles.miniButtonMid);

                            if (DrawToSceneButton(tutorialItem.Scene, "Scene", "d_animationvisibilitytoggleon", EditorStyles.miniButtonRight))
                                break;
                        }

                        GUILayout.Space(5);
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button("See more on GitHub page"))
                Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc");
        }
    }
}
