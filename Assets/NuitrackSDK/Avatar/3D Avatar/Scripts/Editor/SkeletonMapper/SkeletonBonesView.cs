using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Reflection;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonBonesView
    {
        public enum ViewMode
        {
            ModelBones = 0,
            AssignedBones = 1,
            None
        }

        public delegate void BoneHandler(ViewMode viewMode, nuitrack.JointType jointType, Transform boneTransform);

        public event BoneHandler OnRemoveBone;
        public event BoneHandler OnBoneSelected;

        readonly Type boneHandleType = typeof(Editor).Assembly.GetType("UnityEditor.Handles").GetNestedType("BoneRenderer", BindingFlags.NonPublic);
        readonly MethodInfo boneVerticesMethod = null;

        readonly Color select = Color.white;
        readonly Color hoverColor = Color.black;

        readonly Color mainColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        readonly Color unusedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        readonly Color waitModelBoneSelect = Color.yellow;

        const float jointSphereMult = 0.3f;
        const float minSizeAssignBoneMark = 0.1f;
        const float minSizeModelBoneMark = 0.05f;

        readonly Dictionary<nuitrack.JointType, List<nuitrack.JointType>> childsList = null;

        readonly Transform root = null;
        readonly Dictionary<Transform, bool> validBones = null;

        readonly GUIContent[] skeletonModeGuiContent = null;

        ViewMode currentViewMode = ViewMode.ModelBones;

        readonly GUIContent infoIconGUIContent = EditorGUIUtility.IconContent("console.infoicon.sml");

        public virtual nuitrack.JointType SelectedJoint { get; set; } = nuitrack.JointType.None;

        public ViewMode CurrentViewMode
        {
            get
            {
                return currentViewMode;
            }
            set
            {
                currentViewMode = value;
                SceneView.RepaintAll();
            }
        }

        bool ModelBonesSelectionMode
        {
            get
            {
                return CurrentViewMode == ViewMode.ModelBones && SelectedJoint != nuitrack.JointType.None;
            }
        }

        public SkeletonBonesView(Transform root, ViewMode viewMode = ViewMode.AssignedBones)
        {
            CurrentViewMode = viewMode;
            this.root = root;

            validBones = SkeletonUtils.GetValidBones(root);

            boneVerticesMethod = boneHandleType.GetMethod("GetBoneWireVertices", BindingFlags.Public | BindingFlags.Static);

            childsList = new Dictionary<nuitrack.JointType, List<nuitrack.JointType>>();

            foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
            {
                nuitrack.JointType parent = jointType.GetParent();

                if (parent != nuitrack.JointType.None)
                {
                    if (!childsList.ContainsKey(parent))
                        childsList.Add(parent, new List<nuitrack.JointType>());

                    childsList[parent].Add(jointType);
                }
            }

            // UI toolbar elements

            GUIContent modelBonesContent = EditorGUIUtility.IconContent("scenepicking_pickable-mixed_hover");
            modelBonesContent.text = "Model bones";

            GUIContent assignBonesContent = EditorGUIUtility.IconContent("AvatarSelector");
            assignBonesContent.text = "Assigned bones";

            GUIContent noneContent = EditorGUIUtility.IconContent("d_animationvisibilitytoggleoff");

            skeletonModeGuiContent = new GUIContent[] { modelBonesContent, assignBonesContent, noneContent };
        }

        void DrawBone(Vector3 start, Vector3 end)
        {
            if (boneVerticesMethod.Invoke(null, new object[] { start, end }) is Vector3[] vertices)
                Handles.DrawPolyLine(vertices);
        }

        /// <summary>
        /// Draw the skeleton of the avatar in the Scene View.
        /// Use this in method OnSceneGUI of your custom editors.
        /// </summary>
        /// <param name="root">Root transform of the skeleton object</param>
        /// <param name="includeBones">List of bones to hide</param>
        public void DrawSceneGUI(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            switch (CurrentViewMode)
            {
                case ViewMode.ModelBones:
                    DrawModelBones(includeBones);
                    break;

                case ViewMode.AssignedBones:
                    DrawAssignedBones(includeBones);
                    break;
            }
        }

        /// <summary>
        /// Draw a GUI in the inspector.
        /// Use this in method OnInspectorGUI of your custom editors.
        /// </summary>
        public void DrawInspectorGUI()
        {
            EditorGUILayout.LabelField("Skeleton display mode", EditorStyles.boldLabel);

            if (CurrentViewMode != ViewMode.None)
            {
                Color oldColor = GUI.color;

                if (ModelBonesSelectionMode)
                    GUI.color = waitModelBoneSelect;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUI.color = oldColor;
            }

            switch (CurrentViewMode)
            {
                case ViewMode.ModelBones:
                    infoIconGUIContent.text = SelectedJoint == nuitrack.JointType.None ?
                        "Select the joint on the avatar, and then specify the joint on the model in order to set the match." :
                        "Specify the joint on the model in order to set the match. \nClick \"Deselect\" for cancels selection.";

                    EditorGUILayout.LabelField(infoIconGUIContent, EditorStyles.wordWrappedLabel);
                    break;

                case ViewMode.AssignedBones:
                    infoIconGUIContent.text = "The mode displays the specified joints of the skeleton. You can blow out the joints on the model.";
                    EditorGUILayout.LabelField(infoIconGUIContent, EditorStyles.wordWrappedLabel);
                    break;
            }

            CurrentViewMode = (ViewMode)GUILayout.Toolbar((int)CurrentViewMode, skeletonModeGuiContent);

            if (CurrentViewMode != ViewMode.None)
                EditorGUILayout.EndVertical();
        }

        void DrawModelBones(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            foreach (KeyValuePair<Transform, bool> validBone in validBones)
                if (validBone.Value)
                {
                    Transform transform = validBone.Key;

                    bool hasParent = transform.parent != null && validBones.ContainsKey(transform.parent) && validBones[transform.parent];

                    float dist = hasParent ? Vector3.Distance(transform.position, transform.parent.position) : 0;
                    int countJoints = hasParent ? transform.childCount + 2 : transform.childCount + 1;

                    Color oldColor = Handles.color;
                    Handles.color = includeBones.ContainsValue(transform) ? Color.green : unusedColor;

                    List<Transform> childs = new List<Transform>();

                    foreach (Transform child in transform)
                        if (validBones.ContainsKey(child))
                        {
                            dist += Vector3.Distance(transform.position, child.position);
                            childs.Add(child);
                        }

                    dist = Math.Max(minSizeModelBoneMark, dist / countJoints);

                    int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);
                    DrawBoneController(controlID, transform, childs, nuitrack.JointType.None, dist * jointSphereMult);

                    Handles.color = oldColor;
                }
        }

        void DrawAssignedBones(Dictionary<nuitrack.JointType, Transform> includeBones)
        {
            foreach (KeyValuePair<nuitrack.JointType, Transform> jointTransform in includeBones)
            {
                nuitrack.JointType joint = jointTransform.Key;
                nuitrack.JointType parent = joint.GetParent();
                Transform transform = jointTransform.Value;

                float dist = includeBones.ContainsKey(parent) ? Vector3.Distance(transform.position, includeBones[parent].position) : 0;
                List<Transform> childs = new List<Transform>();

                Handles.color = SelectedJoint == joint ? Color.Lerp(mainColor, select, 0.5f) : mainColor;

                if (childsList.ContainsKey(joint))
                {
                    foreach (nuitrack.JointType childJoint in childsList[joint])
                        if (includeBones.ContainsKey(childJoint))
                        {
                            Transform childTransform = includeBones[childJoint];
                            dist += Vector3.Distance(transform.position, childTransform.position);

                            childs.Add(childTransform);
                        }
                }

                int countJoints = childs.Count + (includeBones.ContainsKey(parent) ? 2 : 1);
                dist = Math.Max(minSizeAssignBoneMark, dist / countJoints);

                int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);

                DrawBoneController(controlID, transform, childs, joint, dist * jointSphereMult);
            }
        }

        void DrawBoneController(int controllerID, Transform boneTransform, List<Transform> childs, nuitrack.JointType jointType, float size)
        {
            childs ??= new List<Transform>();

            Event e = Event.current;

            //We divide the size by 2, since strange behavior is detected when an element falls into the selection.
            //The size of the visual element is set by the diameter, and the selection area by the radius.
            Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size / 2, EventType.Layout);

            Color oldColor = Handles.color;

            switch (e.GetTypeForControl(controllerID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controllerID && e.button == 0)
                    {
                        // Respond to a press on this handle. Drag starts automatically.
                        GUIUtility.hotControl = controllerID;
                        GUIUtility.keyboardControl = controllerID;

                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controllerID && e.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();

                        OnBoneSelected?.Invoke(CurrentViewMode, jointType, boneTransform);
                    }
                    break;

                case EventType.Repaint:
                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controllerID)
                        Handles.color = Color.Lerp(Handles.color, hoverColor, 0.5f);
                    break;

                case EventType.KeyDown:
                    if ((e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete) && GUIUtility.keyboardControl == controllerID)
                    {
                        GUIUtility.keyboardControl = 0;
                        e.Use();

                        OnRemoveBone?.Invoke(CurrentViewMode, jointType, boneTransform);
                    }
                    break;
            }

            foreach (Transform child in childs)
                DrawBone(boneTransform.position, child.position);

            Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size, EventType.Repaint);
            Handles.color = oldColor;
        }
    }
}
