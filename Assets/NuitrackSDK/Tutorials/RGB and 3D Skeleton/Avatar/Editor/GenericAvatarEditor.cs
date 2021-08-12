using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(GenericAvatar), true)]
    public class GenericAvatarEditor : AvatarEditor
    {
        Color mainColor = new Color(0.2f, 0.6f, 1f, 1f);// Color.blue;
        Color disableColor = new Color(0.5f, 0.5f, 0.6f, 1f);

        Dictionary<AvatarMaskBodyPart, bool> foldOpenned;

        void OnEnable()
        {
            foldOpenned = Styles.BodyPartsOrder.ToDictionary(k => k, v => true);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GenericAvatar myScript = (GenericAvatar)target;

            Rect rect = DudeRect;

            ref List<GenericAvatar.JointItem> jointItems = ref myScript.JointItems;
            Dictionary<nuitrack.JointType, GenericAvatar.JointItem> jointsDict = jointItems.ToDictionary(k => k.jointType);

            List<AvatarMaskBodyPart> bodyParts = GetActiveBodyParts(jointsDict);
            DrawDude(rect, mainColor, disableColor, bodyParts);            

            foreach (AvatarMaskBodyPart bodyPart in Styles.BodyPartsOrder)
            {
                foldOpenned[bodyPart] = EditorGUILayout.BeginFoldoutHeaderGroup(foldOpenned[bodyPart], bodyPart.ToString());
                bool drawItems = foldOpenned[bodyPart];

                if (drawItems)
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (Styles.JointItem jointItem in Styles.JointItems[bodyPart])
                {
                    nuitrack.JointType jointType = jointItem.jointType;
                    Transform jointTransform = jointsDict.ContainsKey(jointType) ? jointsDict[jointType].boneTransform : null;

                    Rect jointPointRect = BoneIconRect(rect, jointItem);
                    if (jointTransform != null)
                        GUI.DrawTexture(jointPointRect, Styles.dotFill.image);


                    Transform newJoint = HandleDragDrop(jointPointRect);
                    if (newJoint != null)
                    {
                        AddJoint(myScript, jointType, newJoint, ref jointsDict, ref jointItems);
                        EditorGUIUtility.PingObject(newJoint);
                    }

                    Event evt = Event.current;

                    if (evt.type == EventType.MouseDown && jointPointRect.Contains(evt.mousePosition))
                    {
                        if (jointTransform != null)
                            EditorGUIUtility.PingObject(jointTransform);

                        evt.Use();
                    }

                    if (drawItems)
                    {
                        jointTransform = (Transform)EditorGUILayout.ObjectField(jointType.ToString(), jointTransform, typeof(Transform), true);
                        AddJoint(myScript, jointType, jointTransform, ref jointsDict, ref jointItems);
                    }
                }

                if (drawItems)
                    EditorGUILayout.EndVertical();

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        void AddJoint(GenericAvatar myScript, nuitrack.JointType jointType, Transform objectTransform, 
            ref Dictionary<nuitrack.JointType, GenericAvatar.JointItem> jointsDict,  
            ref List<GenericAvatar.JointItem> jointItems)
        {
            if (objectTransform != null)
            {
                Undo.RecordObject(myScript, "Avatar mapping modified");

                if (jointsDict.ContainsKey(jointType))
                    jointsDict[jointType].boneTransform = objectTransform;
                else
                    jointItems.Add(
                        new GenericAvatar.JointItem()
                        {
                            boneTransform = objectTransform,
                            jointType = jointType,
                        });
            }
            else if (jointsDict.ContainsKey(jointType))
            {
                Undo.RecordObject(myScript, "Remove avatar joint object");
                jointItems.Remove(jointsDict[jointType]);
            }
        }

        List<AvatarMaskBodyPart> GetActiveBodyParts(Dictionary<nuitrack.JointType, GenericAvatar.JointItem> jointsDict)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>(Styles.BodyPartsOrder);

            foreach (KeyValuePair<AvatarMaskBodyPart, List<Styles.JointItem>> bodyPart in Styles.JointItems)
                foreach (Styles.JointItem jointItem in bodyPart.Value)
                    if (!jointItem.optional && !jointsDict.ContainsKey(jointItem.jointType))
                    {
                        bodyParts.Remove(bodyPart.Key);
                        break;
                    }

            return bodyParts;
        }

        Rect BoneIconRect(Rect baseRect, Styles.JointItem jointItem)
        {
            Vector2 pos = jointItem.mapPosition;
            pos.y *= -1; // because higher values should be up
            pos.Scale(new Vector2(baseRect.width * 0.5f, baseRect.height * 0.5f));
            pos = baseRect.center + pos;
            int kIconSize = Styles.kIconSize;
            Rect boneRect = new Rect(pos.x - kIconSize * 0.5f, pos.y - kIconSize * 0.5f, kIconSize, kIconSize);

            Color oldColor = GUI.color;
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1);

            GUIContent dotGUI = jointItem.optional ? Styles.dotFrameDotted : Styles.dotFrame;
            GUI.DrawTexture(boneRect, dotGUI.image);
            GUI.color = oldColor;

            return boneRect;
        }

        Transform HandleDragDrop(Rect dropRect)
        {
            EventType eventType = Event.current.type;

            Transform dropTransform = null;

            switch (eventType)
            {
                case EventType.DragExited:
                    if (GUI.enabled)
                        HandleUtility.Repaint();
                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dropRect.Contains(Event.current.mousePosition) && GUI.enabled)
                    {
                        Object[] references = DragAndDrop.objectReferences;
                        Object validatedObject = references.Length == 1 ? references[0] : null;

                        if (validatedObject != null)
                        {
                            if (!(validatedObject is Transform || validatedObject is GameObject) || EditorUtility.IsPersistent(validatedObject))
                                validatedObject = null;
                        }

                        if (validatedObject != null)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                            if (eventType == EventType.DragPerform)
                            {
                                if (validatedObject is GameObject)
                                    dropTransform = (validatedObject as GameObject).transform;
                                else
                                    dropTransform = validatedObject as Transform;

                                GUI.changed = true;
                                DragAndDrop.AcceptDrag();
                                DragAndDrop.activeControlID = 0;
                            }
                            else
                            {
                                //DragAndDrop.activeControlID = id;
                            }
                            Event.current.Use();
                        }
                    }
                    break;
            }

            return dropTransform;
        }

        //void DrawSkeleton()
        //{

        //}
    }
}