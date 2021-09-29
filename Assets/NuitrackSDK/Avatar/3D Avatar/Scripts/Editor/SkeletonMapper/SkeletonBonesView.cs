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

        public delegate void BoneHandler(nuitrack.JointType jointType, Transform boneTransform);

        public event BoneHandler OnRemoveBone;
        public event BoneHandler OnBoneSelected;

        readonly Type boneHandleType = typeof(Editor).Assembly.GetType("UnityEditor.Handles").GetNestedType("BoneRenderer", BindingFlags.NonPublic);
        readonly MethodInfo boneVerticesMethod = null;

        readonly Color select = Color.white;
        readonly Color hoverColor = Color.black;

        readonly Color mainColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        readonly Color unusedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        const float jointSphereMult = 0.3f;
        const float minSizeAssignBoneMark = 0.1f;
        const float minSizeModelBoneMark = 0.05f;

        readonly Dictionary<nuitrack.JointType, List<nuitrack.JointType>> childsList = null;

        readonly Transform root = null;
        readonly Dictionary<Transform, bool> validBones = null;

        public virtual nuitrack.JointType SelectedJoint { get; set; } = nuitrack.JointType.None;

        public ViewMode CurrentViewMode
        {
            get;
            set;
        } = ViewMode.ModelBones;

        public SkeletonBonesView(Transform root, ViewMode viewMode)
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
        /// <param name="excludeBoneTransforms">List of bones to hide</param>
        public void DrawSkeleton(Dictionary<nuitrack.JointType, Transform> excludeBoneTransforms)
        {
            switch (CurrentViewMode)
            {
                case ViewMode.ModelBones:
                    DrawBonesRecursive(root, excludeBoneTransforms);
                    break;

                case ViewMode.AssignedBones:
                    DrawCurrentSkeleton(excludeBoneTransforms);
                    break;
            }
        }

        void DrawBonesRecursive(Transform transform, Dictionary<nuitrack.JointType, Transform> excludeBoneTransforms)
        {
            if (validBones.ContainsKey(transform) && validBones[transform])
            {
                float dist = validBones.ContainsKey(transform.parent) ? Vector3.Distance(transform.position, transform.parent.position) : 0;
                int countJoints = validBones.ContainsKey(transform.parent) ? transform.childCount + 2 : transform.childCount + 1;

                Color oldColor = Handles.color;
                Handles.color = excludeBoneTransforms.ContainsValue(transform) ? Color.green : unusedColor;

                foreach (Transform child in transform)
                    if (validBones.ContainsKey(child))
                    {
                        dist += Vector3.Distance(transform.position, child.position);
                        DrawBone(transform.position, child.position);
                    }

                dist = Math.Max(minSizeModelBoneMark, dist / countJoints);

                int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);
                DrawBoneController(controlID, transform, nuitrack.JointType.None, dist * jointSphereMult);

                Handles.color = oldColor;
            }

            foreach (Transform child in transform)
                DrawBonesRecursive(child, excludeBoneTransforms);
        }

        void DrawCurrentSkeleton(Dictionary<nuitrack.JointType, Transform> excludeBoneTransforms)
        {
            foreach (KeyValuePair<nuitrack.JointType, Transform> jointTransform in excludeBoneTransforms)
            {
                nuitrack.JointType joint = jointTransform.Key;
                nuitrack.JointType parent = joint.GetParent();
                Transform transform = jointTransform.Value;

                float dist = excludeBoneTransforms.ContainsKey(parent) ? Vector3.Distance(transform.position, excludeBoneTransforms[parent].position) : 0;
                int countJoints = excludeBoneTransforms.ContainsKey(parent) ? 2 : 1;

                Handles.color = SelectedJoint == joint ? Color.Lerp(mainColor, select, 0.5f) : mainColor;

                if (childsList.ContainsKey(joint))
                {
                    foreach (nuitrack.JointType childJoint in childsList[joint])
                    {
                        if (excludeBoneTransforms.ContainsKey(childJoint))
                        {
                            Transform childTransform = excludeBoneTransforms[childJoint];

                            dist += Vector3.Distance(transform.position, childTransform.position);
                            countJoints++;

                            DrawBone(transform.position, childTransform.position);
                        }
                    }
                }

                dist = Math.Max(minSizeAssignBoneMark, dist / countJoints);
                int controlID = GUIUtility.GetControlID(root.name.GetHashCode(), FocusType.Passive);

                DrawBoneController(controlID, transform, joint, dist * jointSphereMult);
            }
        }

        void DrawBoneController(int controllerID, Transform boneTransform, nuitrack.JointType jointType, float size)
        {
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

                        OnBoneSelected?.Invoke(jointType, boneTransform);
                    }
                    break;

                case EventType.Repaint:

                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controllerID)
                        Handles.color = Color.Lerp(Handles.color, hoverColor, 0.5f);
                    break;

                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == controllerID)
                    {
                        if (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete)
                        {
                            GUIUtility.keyboardControl = 0;
                            e.Use();

                            OnRemoveBone?.Invoke(jointType, boneTransform);
                        }
                    }
                    break;
            }


            Handles.SphereHandleCap(controllerID, boneTransform.position, boneTransform.rotation, size, EventType.Repaint);
            Handles.color = oldColor;
        }
    }
}
