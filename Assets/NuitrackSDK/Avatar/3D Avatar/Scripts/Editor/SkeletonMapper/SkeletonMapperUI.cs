using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonMapperUI<T> : SkeletonMapper<T> where T : Object
    {
        List<AvatarMaskBodyPart> GetActiveBodyParts(List<JointType> jointsList)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>(Styles.BodyParts.Keys);

            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPart in Styles.BodyParts)
                foreach (Styles.GUIJoint guiJoint in bodyPart.Value.guiJoint)
                    if (!guiJoint.optional && !jointsList.Contains(guiJoint.jointType))
                    {
                        bodyParts.Remove(bodyPart.Key);
                        break;
                    }

            return bodyParts;
        }

        Rect DrawAvatarJointIcon(Rect rect, Styles.GUIJoint guiJoint, bool filled, bool selected)
        {
            Vector2 pos = guiJoint.mapPosition;
            pos.Scale(new Vector2(rect.width * 0.5f, -rect.height * 0.5f));
            pos += rect.center;

            Texture dotGUI = (guiJoint.optional ? Styles.Dot.frameDotted : Styles.Dot.frame).image;
            Vector2 position = new Vector2(pos.x - dotGUI.width * 0.5f, pos.y - dotGUI.height * 0.5f);

            //------------------------

            Rect jointRect = new Rect(position.x, position.y, dotGUI.width, dotGUI.height);

            Color oldColor = GUI.color;
            GUI.color = Styles.Dot.color;
            GUI.DrawTexture(jointRect, dotGUI);
            GUI.color = oldColor;

            if (filled)
                GUI.DrawTexture(jointRect, Styles.Dot.fill.image);

            if (selected)
                GUI.DrawTexture(jointRect, Styles.Dot.selection.image);

            return jointRect;
        }

        public void Draw(List<JointType> activeJoints)
        {
            Rect rect = GUILayoutUtility.GetRect(Styles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.UnityDude.image.width));
            rect.x += (EditorGUIUtility.currentViewWidth - rect.width) / 2;

            Color oldColor = GUI.color;

            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            GUI.DrawTexture(rect, Styles.UnityDude.image);

            List<AvatarMaskBodyPart> filled = GetActiveBodyParts(activeJoints);

            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPart in Styles.BodyParts)
            {
                GUI.color = filled.Contains(bodyPart.Key) ? Styles.mainColor : Styles.disableColor;

                foreach (GUIContent guiContent in bodyPart.Value.guiContents)
                    GUI.DrawTexture(rect, guiContent.image);
            }

            GUI.color = oldColor;

            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPartItem in Styles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                Styles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    JointType jointType = guiJoint.jointType;
                    Rect jointPointRect = DrawAvatarJointIcon(rect, guiJoint, activeJoints.Contains(jointType), jointType == SelectJoint);

                    T newJoint = HandleDragDrop(jointPointRect);

                    if (newJoint != null)
                        OnDropAction(newJoint, jointType);

                    if (HandleClick(jointPointRect))
                        OnSelectedAction(jointType);
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
