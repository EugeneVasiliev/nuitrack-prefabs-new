using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

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

        static string selectTags = string.Empty;

        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            NuitrackTutorialsEditorWindow window = (NuitrackTutorialsEditorWindow)EditorWindow.GetWindow(typeof(NuitrackTutorialsEditorWindow));
            window.Show();
        }

        void OnGUI()
        {
            DrawTutorials(true);
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
   
        static string TagField()
        {
            GUIContent clearButton = EditorGUIUtility.IconContent("d_winbtn_win_close");
            clearButton.tooltip = "Clear";

            Rect main = EditorGUILayout.GetControlRect();
            main.xMax -= clearButton.image.width;

            Rect clearButtonRect = new Rect(main.x + main.width, main.y, clearButton.image.width, main.height);

            string inputTags = EditorGUI.TextField(main, "Filter by tags", selectTags);

            if (GUI.Button(clearButtonRect, clearButton, GUIStyle.none))
            {
                inputTags = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            return inputTags;
        }

        public static void DrawTutorials(bool inspectorMode)
        {
            EditorGUILayout.LabelField("Nuitrack tutorials", HeaderLabelStyle);
            EditorGUILayout.Space();

            NuitrackTutorials tutorials = (NuitrackTutorials)AssetDatabase.LoadAssetAtPath("Assets/NuitrackSDK/TUTORIALS.asset", typeof(NuitrackTutorials));

            selectTags = TagField();

            List<string> tags = new List<string>(selectTags.Split(','));
            for(int i = 0; i < tags.Count; i++)
                tags[i] = tags[i].Trim(' ');

            if (inspectorMode)
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

                        using (new HorizontalGroup())
                        {
                            foreach (string tag in tutorialItem.Tags)
                            {
                                bool tagClicked = GUILayout.Button(string.Format("#{0}", tag), EditorStyles.linkLabel);
                                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                                if (tagClicked)
                                {
                                    selectTags += selectTags == string.Empty ? tag : string.Format(" ,{0}", tag);
                                    GUIUtility.keyboardControl = 0;
                                }
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

            if (inspectorMode)
                GUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button("See more on GitHub page"))
                Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc");
        }
    }
}
