using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonMapperGUI<T> : SkeletonMapper<T> where T : Object
    {
        public class ColorTheme
        {
            public Color mainColor;
            public Color disableColor;
        }

        List<JointType> jointMask = null;

        ColorTheme colorTheme = new ColorTheme()
        {
            mainColor = new Color(0.2f, 0.6f, 1f, 1f), // Color.blue;
            disableColor = new Color(0.5f, 0.5f, 0.6f, 1f)
        };

        /// <summary>
        /// View of a person with a map of joints.
        /// </summary>
        /// <param name="jointMask">The mask of the displayed joints. If null, all available joints will be drawn.</param>
        /// <param name="colorTheme">Color theme. If null is set, the default theme will be used.</param>
        public SkeletonMapperGUI(List<JointType> jointMask, ColorTheme colorTheme = null)
        {
            this.jointMask = jointMask;
            
            if(colorTheme != null)
                this.colorTheme = colorTheme;
        }

        List<AvatarMaskBodyPart> GetActiveBodyParts(List<JointType> jointsList)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>(SkeletonMapperStyles.BodyParts.Keys);

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonMapperStyles.GUIBodyPart> bodyPart in SkeletonMapperStyles.BodyParts)
                foreach (SkeletonMapperStyles.GUIJoint guiJoint in bodyPart.Value.guiJoint)
                    if (!guiJoint.Optional && !jointsList.Contains(guiJoint.JointType) && (jointMask == null || jointMask.Contains(guiJoint.JointType)))
                    {
                        bodyParts.Remove(bodyPart.Key);
                        break;
                    }

            return bodyParts;
        }

        Rect DrawAvatarJointIcon(Rect rect, SkeletonMapperStyles.GUIJoint guiJoint, bool filled, bool selected)
        {
            Vector2 pos = guiJoint.MapPosition;
            pos.Scale(new Vector2(rect.width * 0.5f, -rect.height * 0.5f));
            pos += rect.center;

            Rect jointRect = SkeletonMapperStyles.Dot.DrawСentered(pos, guiJoint.Optional, filled, selected);
            return jointRect;
        }

        /// <summary>
        /// Draw a map of joints
        /// </summary>
        /// <param name="activeJoints">Active joints (will be displayed as filled dots)</param>
        public void Draw(List<JointType> activeJoints)
        {
            Rect rect = GUILayoutUtility.GetRect(SkeletonMapperStyles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(SkeletonMapperStyles.UnityDude.image.width));
            rect.x += (EditorGUIUtility.currentViewWidth - rect.width) / 2;

            Color oldColor = GUI.color;

            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            GUI.DrawTexture(rect, SkeletonMapperStyles.UnityDude.image);

            List<AvatarMaskBodyPart> filled = GetActiveBodyParts(activeJoints);

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonMapperStyles.GUIBodyPart> bodyPart in SkeletonMapperStyles.BodyParts)
            {
                GUI.color = filled.Contains(bodyPart.Key) ? colorTheme.mainColor : colorTheme.disableColor;

                foreach (GUIContent guiContent in bodyPart.Value.guiContents)
                    GUI.DrawTexture(rect, guiContent.image);
            }

            GUI.color = oldColor;

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonMapperStyles.GUIBodyPart> bodyPartItem in SkeletonMapperStyles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                SkeletonMapperStyles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foreach (SkeletonMapperStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    JointType jointType = guiJoint.JointType;

                    if (jointMask == null || jointMask.Contains(jointType))
                    {
                        Rect jointPointRect = DrawAvatarJointIcon(rect, guiJoint, activeJoints.Contains(jointType), jointType == SelectedJoint);

                        T newJoint = HandleDragDrop(jointPointRect);

                        if (newJoint != null)
                            OnDropAction(newJoint, jointType);

                        if (HandleClick(jointPointRect))
                            OnSelectedAction(jointType);
                    }
                }
            }
        }

        T HandleDragDrop(Rect dropRect)
        {
            EventType eventType = Event.current.type;

            T dropObject = default(T);

            if ((eventType == EventType.DragPerform || eventType == EventType.DragUpdated) &&
                dropRect.Contains(Event.current.mousePosition) && GUI.enabled)
            {
                Object[] references = DragAndDrop.objectReferences;
                Object validatedObject = references.Length == 1 ? references[0] : null;

                T validObj = GetValidType(validatedObject);

                if (validObj != null && !validObj.Equals(default(T)))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                    if (eventType == EventType.DragPerform)
                        dropObject = validObj;

                    GUI.changed = true;
                    DragAndDrop.AcceptDrag();
                    DragAndDrop.activeControlID = 0;
                }
            }

            return dropObject;
        }

        T GetValidType(Object validatedObject)
        {
            if (EditorUtility.IsPersistent(validatedObject))
                return default(T);

            if (validatedObject is T)
                return (T)validatedObject;
            else if (validatedObject is GameObject)
                return (validatedObject as GameObject).GetComponent<T>();

            return default(T);
        }
    }
}
