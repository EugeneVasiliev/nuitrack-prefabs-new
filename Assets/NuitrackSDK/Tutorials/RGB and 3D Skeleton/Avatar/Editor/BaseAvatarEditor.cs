using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(BaseAvatar), true)]
    public class BaseAvatarEditor : UnityEditor.Editor
    {
        protected static class Styles
        {
            public class GUIBodyPart
            {
                public string labelKey;

                public List<GUIJoint> guiJoint;
                public List<GUIContent> guiContents;

                public string Lable
                {
                    get
                    {
                        return EditorGUIUtility.TrTextContent(labelKey).text;
                    }
                }
            }

            public class GUIJoint
            {
                public nuitrack.JointType jointType;
                public Vector2 mapPosition;
                public bool optional = false;

                public GUIJoint(nuitrack.JointType jointType, Vector2 mapPosition, bool optional = false)
                {
                    this.jointType = jointType;
                    this.mapPosition = mapPosition;
                    this.optional = optional;
                }
            }

            public static Color mainColor = new Color(0.2f, 0.6f, 1f, 1f); // Color.blue;
            public static Color disableColor = new Color(0.5f, 0.5f, 0.6f, 1f);

            public static class Dot
            {
                public static Color color = new Color(0.7f, 0.7f, 0.7f, 1);

                public static GUIContent fill = EditorGUIUtility.IconContent("AvatarInspector/DotFill");
                public static GUIContent frame = EditorGUIUtility.IconContent("AvatarInspector/DotFrame");
                public static GUIContent frameDotted = EditorGUIUtility.IconContent("AvatarInspector/DotFrameDotted");
                public static GUIContent selection = EditorGUIUtility.IconContent("AvatarInspector/DotSelection");
            }

            public static GUIContent UnityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");

            public static Dictionary<AvatarMaskBodyPart, GUIBodyPart> BodyParts = new Dictionary<AvatarMaskBodyPart, GUIBodyPart>()
            {
                {
                    AvatarMaskBodyPart.Body, new GUIBodyPart()
                    {
                        labelKey = "Body",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.Waist, new Vector2(0.00f, 0.08f)),
                            new GUIJoint(nuitrack.JointType.Torso, new Vector2(0.00f, 0.35f)),
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Torso") }
                    }
                    
                    //{ HumanBodyBones.Spine, new Vector2(0.00f, 0.20f) },
                    //{ HumanBodyBones.UpperChest, new Vector2(0.00f, 0.50f) },
                },

                {
                    AvatarMaskBodyPart.Head, new GUIBodyPart()
                    {
                        labelKey = "Head",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.Neck, new Vector2(0.00f, 0.66f), true),
                            new GUIJoint(nuitrack.JointType.Head, new Vector2(0.00f, 0.76f), true)
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Head") }
                    }
                },

                {
                    AvatarMaskBodyPart.LeftLeg, new GUIBodyPart()
                    {
                        labelKey = "Left Leg",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.LeftHip, new Vector2(-0.16f, 0.01f)),
                            new GUIJoint(nuitrack.JointType.LeftKnee, new Vector2(-0.21f, -0.40f)),
                            new GUIJoint(nuitrack.JointType.LeftAnkle, new Vector2(-0.23f, -0.80f)),
                            //new JointItem(nuitrack.JointType.LeftFoot, new Vector2(0.25f, -0.89f), true),
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/RightLeg") }
                    }

                },

                {
                    AvatarMaskBodyPart.RightLeg, new GUIBodyPart()
                    {
                        labelKey = "Right Leg",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.RightHip, new Vector2(0.16f, 0.01f)),
                            new GUIJoint(nuitrack.JointType.RightKnee,  new Vector2(0.21f, -0.40f)),
                            new GUIJoint(nuitrack.JointType.RightAnkle, new Vector2(0.23f, -0.80f)),
                            //new JointItem(nuitrack.JointType.RightFoot, new Vector2(-0.25f, -0.89f), true)
                        },

                        guiContents = new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/LeftLeg") }
                    }
                },

                {
                    AvatarMaskBodyPart.LeftArm, new GUIBodyPart()
                    {
                        labelKey = "Left Arm",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.LeftCollar, new Vector2(-0.14f, 0.60f), true),
                            new GUIJoint(nuitrack.JointType.LeftShoulder, new Vector2(-0.30f, 0.57f)),
                            new GUIJoint(nuitrack.JointType.LeftElbow, new Vector2(-0.48f, 0.30f)),
                            new GUIJoint(nuitrack.JointType.LeftWrist, new Vector2(-0.66f, 0.03f))
                        },

                        guiContents = new List<GUIContent>()
                        {
                            EditorGUIUtility.IconContent("AvatarInspector/RightArm"),
                            EditorGUIUtility.IconContent("AvatarInspector/RightFingers")
                        }
                    }
                },

                {
                    AvatarMaskBodyPart.RightArm, new GUIBodyPart()
                    {
                        labelKey = "Right Arm",

                        guiJoint = new List<GUIJoint>()
                        {
                            new GUIJoint(nuitrack.JointType.RightCollar, new Vector2(0.14f, 0.60f), true),
                            new GUIJoint(nuitrack.JointType.RightShoulder, new Vector2(0.30f, 0.57f)),
                            new GUIJoint(nuitrack.JointType.RightElbow, new Vector2(0.48f, 0.30f)),
                            new GUIJoint(nuitrack.JointType.RightWrist, new Vector2(0.66f, 0.03f))
                        },

                        guiContents = new List<GUIContent>()
                        {
                            EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
                            EditorGUIUtility.IconContent("AvatarInspector/LeftFingers")
                        }
                    }
                }
            };
        }

        protected Rect GetAvatarViewRect()
        {
            Rect rect = GUILayoutUtility.GetRect(Styles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.UnityDude.image.width));
            rect.x += (EditorGUIUtility.currentViewWidth - rect.width) / 2;

            return rect;
        }

        protected void DrawDude(Rect rect, List<AvatarMaskBodyPart> filled)
        {
            Color oldColor = GUI.color;

            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            GUI.DrawTexture(rect, Styles.UnityDude.image);

            foreach(KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPart in Styles.BodyParts)
            {
                GUI.color = filled != null && filled.Contains(bodyPart.Key) ? Styles.mainColor : Styles.disableColor;

                foreach (GUIContent guiContent in bodyPart.Value.guiContents)
                    GUI.DrawTexture(rect, guiContent.image);
            }

            GUI.color = oldColor;
        }

        protected Rect DrawAvatarJointIcon(Rect rect, Styles.GUIJoint jointItem, bool filled, bool selected)
        {
            Vector2 pos = jointItem.mapPosition;
            pos.y *= -1; // because higher values should be up
            pos.Scale(new Vector2(rect.width * 0.5f, rect.height * 0.5f));
            pos = rect.center + pos;

            Texture dotGUI = (jointItem.optional ? Styles.Dot.frameDotted : Styles.Dot.frame).image;
            Vector2 jointRect = new Vector2(pos.x - dotGUI.width * 0.5f, pos.y - dotGUI.height * 0.5f);

            return DrawJointDot(jointRect, jointItem, filled, selected);
        }

        protected Rect DrawJointDot(Vector2 position, Styles.GUIJoint jointItem, bool filled, bool selected)
        {
            Texture dotGUI = (jointItem.optional ? Styles.Dot.frameDotted : Styles.Dot.frame).image;

            Rect rect = new Rect(position.x, position.y, dotGUI.width, dotGUI.height);

            Color oldColor = GUI.color;
            GUI.color = Styles.Dot.color;
            GUI.DrawTexture(rect, dotGUI);
            GUI.color = oldColor;

            if (filled)
                GUI.DrawTexture(rect, Styles.Dot.fill.image);

            if(selected)
                GUI.DrawTexture(rect, Styles.Dot.selection.image);

            return rect;
        }

        protected Transform HandleDragDrop(Rect dropRect)
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

        protected List<AvatarMaskBodyPart> GetActiveBodyParts(ICollection<nuitrack.JointType> jointsList)
        {
            List<AvatarMaskBodyPart> bodyParts = new List<AvatarMaskBodyPart>(Styles.BodyParts.Keys);

            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPart in Styles.BodyParts)
                foreach (Styles.GUIJoint jointItem in bodyPart.Value.guiJoint)
                    if (!jointItem.optional && !jointsList.Contains(jointItem.jointType))
                    {
                        bodyParts.Remove(bodyPart.Key);
                        break;
                    }

            return bodyParts;
        }

        protected bool PingObject(Rect rect, Transform targetTransform)
        {
            Event evt = Event.current;

            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                if (targetTransform != null)
                    EditorGUIUtility.PingObject(targetTransform);

                evt.Use();

                return true;
            }

            return false;
        }
    }
}