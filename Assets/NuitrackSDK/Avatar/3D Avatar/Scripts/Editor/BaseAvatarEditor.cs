using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;

namespace NuitrackSDK.Avatar.Editor
{
    public static class Styles
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
            public JointType jointType;
            public Vector2 mapPosition;
            public bool optional = false;

            public GUIJoint(JointType jointType, Vector2 mapPosition, bool optional = false)
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
                            new GUIJoint(JointType.Waist, new Vector2(0.00f, 0.08f)),
                            new GUIJoint(JointType.Torso, new Vector2(0.00f, 0.35f)),
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
                            new GUIJoint(JointType.Neck, new Vector2(0.00f, 0.7f), true),
                            new GUIJoint(JointType.Head, new Vector2(0.00f, 0.80f), true)
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
                            new GUIJoint(JointType.LeftHip, new Vector2(-0.16f, 0.01f)),
                            new GUIJoint(JointType.LeftKnee, new Vector2(-0.21f, -0.40f)),
                            new GUIJoint(JointType.LeftAnkle, new Vector2(-0.23f, -0.80f)),
                            //new GUIJoint(JointType.LeftFoot, new Vector2(0.25f, -0.89f), true),
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
                            new GUIJoint(JointType.RightHip, new Vector2(0.16f, 0.01f)),
                            new GUIJoint(JointType.RightKnee,  new Vector2(0.21f, -0.40f)),
                            new GUIJoint(JointType.RightAnkle, new Vector2(0.23f, -0.80f)),
                            //new GUIJoint(JointType.RightFoot, new Vector2(-0.25f, -0.89f), true)
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
                            new GUIJoint(JointType.LeftCollar, new Vector2(-0.12f, 0.60f), true),
                            new GUIJoint(JointType.LeftShoulder, new Vector2(-0.30f, 0.57f)),
                            new GUIJoint(JointType.LeftElbow, new Vector2(-0.48f, 0.30f)),
                            new GUIJoint(JointType.LeftWrist, new Vector2(-0.66f, 0.03f))
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
                            new GUIJoint(JointType.RightCollar, new Vector2(0.12f, 0.60f), true),
                            new GUIJoint(JointType.RightShoulder, new Vector2(0.30f, 0.57f)),
                            new GUIJoint(JointType.RightElbow, new Vector2(0.48f, 0.30f)),
                            new GUIJoint(JointType.RightWrist, new Vector2(0.66f, 0.03f))
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

    [CustomEditor(typeof(BaseAvatar), true)]
    public class BaseAvatarEditor : UnityEditor.Editor
    {
        //JointType selectJoint = JointType.None;

        SkeletonMapperUI<Transform> skeletonMapper = null;
        SkeletonJointListUI<Transform> skeletonJointListUI = null;

        protected virtual void AddJoint(JointType jointType, Transform objectTransform)
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            ref List<ModelJoint> modelJoints = ref myScript.ModelJoints;

            Dictionary<JointType, ModelJoint> jointsDict = modelJoints.ToDictionary(k => k.jointType);

            if (objectTransform != null)
            {
                Undo.RecordObject(myScript, "Avatar mapping modified");

                if (jointsDict.ContainsKey(jointType))
                    jointsDict[jointType].bone = objectTransform;
                else
                {
                    ModelJoint modelJoint = new ModelJoint()
                    {
                        bone = objectTransform,
                        jointType = jointType,
                    };

                    modelJoints.Add(modelJoint);
                }
            }
            else if (jointsDict.ContainsKey(jointType))
            {
                Undo.RecordObject(myScript, "Remove avatar joint object");          
                modelJoints.Remove(jointsDict[jointType]);
            }
        }

        protected virtual void OnEnable()
        {
            //selectJoint = JointType.None;

            BaseAvatar avatar = serializedObject.targetObject as BaseAvatar;

            if (avatar.ModelJoints == null)
                avatar.ModelJoints = new List<ModelJoint>();

            skeletonMapper = new SkeletonMapperUI<Transform>();
            skeletonMapper.onDrop += SkeletonMapper_onDrop;
            skeletonMapper.onSelected += SkeletonMapper_onSelected;

            skeletonJointListUI = new SkeletonJointListUI<Transform>();
            skeletonJointListUI.onDrop += SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected += SkeletonMapper_onSelected;
        }

        protected virtual void OnDisable()
        {
            skeletonMapper.onDrop -= SkeletonMapper_onDrop;
            skeletonMapper.onSelected -= SkeletonMapper_onSelected;
            skeletonMapper = null;

            skeletonJointListUI.onDrop -= SkeletonMapper_onDrop;
            skeletonJointListUI.onSelected -= SkeletonMapper_onSelected;
            skeletonJointListUI = null;
        }

        void SkeletonMapper_onDrop(Transform newJoint, JointType jointType)
        {
            AddJoint(jointType, newJoint);

            if (newJoint != null)
            {
                EditorGUIUtility.PingObject(newJoint);
                //selectJoint = jointType;

                skeletonMapper.SelectJoint = jointType;
                skeletonJointListUI.SelectJoint = jointType;
            }
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            //selectJoint = jointType;
            skeletonMapper.SelectJoint = jointType;
            skeletonJointListUI.SelectJoint = jointType;

            Dictionary<JointType, ModelJoint> jointsDict = myScript.ModelJoints.ToDictionary(k => k.jointType);

            if (jointsDict[jointType].bone != null)
            {
                //EditorGUIUtility.PingObject(jointsDict[jointType].bone);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawSkeletonSettings();
            DrawSubAvatarGUI();

            DrawCustomDefaultInspector();
            DrawSkeletonMap();
        }

        protected void DrawSkeletonSettings()
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skeleton settings", EditorStyles.boldLabel);

            SerializedProperty useCurrentUserTracker = serializedObject.FindProperty("useCurrentUserTracker");
            useCurrentUserTracker.boolValue = EditorGUILayout.Toggle("Use current user tracker", useCurrentUserTracker.boolValue);
            serializedObject.ApplyModifiedProperties();

            if (!useCurrentUserTracker.boolValue)
            {
                SerializedProperty skeletonID = serializedObject.FindProperty("skeletonID");
                skeletonID.intValue = EditorGUILayout.IntSlider("Skeleton ID", skeletonID.intValue, myScript.MinSkeletonID, myScript.MaxSkeletonID);
                serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty jointConfidence = serializedObject.FindProperty("jointConfidence");
            jointConfidence.floatValue = EditorGUILayout.Slider("Joint confidence", jointConfidence.floatValue, 0, 1);
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawSubAvatarGUI()
        {

        }

        protected virtual void DrawCustomDefaultInspector()
        {
            SerializedProperty property = serializedObject.GetIterator();

            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                if ("m_Script" != property.propertyPath)
                    EditorGUILayout.PropertyField(property, true);

                //using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
                //{
                //    EditorGUILayout.PropertyField(property, true);
                //}
                expanded = false;
            }
            serializedObject.ApplyModifiedProperties();
        }   

        protected void DrawSkeletonMap()
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Avatar map", EditorStyles.boldLabel);

            if (skeletonMapper != null)
            {
                List<JointType> activeJoints = (from j in myScript.ModelJoints where j.bone != null select j.jointType).ToList();
                skeletonMapper.Draw(activeJoints);
            }

            if (skeletonJointListUI != null)
            {
                Dictionary<JointType, Transform> jointDict = myScript.ModelJoints.Where(v => v.bone != null).ToDictionary(k => k.jointType, v => v.bone);
                skeletonJointListUI.Draw(jointDict);
            }
        }
    }

    public class SkeletonMapper<T> where T : Object
    {
        public delegate void DropHandler(T dropObject, JointType jointType);
        public delegate void SelectHandler(JointType jointType);

        public event DropHandler onDrop;
        public event SelectHandler onSelected;

        public JointType SelectJoint { get; set; } = JointType.None;

        protected void OnDropAction(T dropObject, JointType jointType)
        {
            if (onDrop != null)
                onDrop(dropObject, jointType);
        }

        protected void OnSelectedAction(JointType jointType)
        {
            if (onSelected != null)
                onSelected(jointType);
        } 

        protected bool HandleClick(Rect clickRect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.MouseDown && clickRect.Contains(evt.mousePosition))
            {
                evt.Use();
                return true;
            }
            return false;
        }
    }

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
                    bool isActive = activeJoints.Contains(jointType);

                    Rect jointPointRect = DrawAvatarJointIcon(rect, guiJoint, isActive, jointType == SelectJoint);

                    T newJoint = HandleDragDrop(jointPointRect);

                    if(newJoint != null)
                        OnDropAction(newJoint, jointType);

                    if(HandleClick(jointPointRect))
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

    public class SkeletonJointListUI<T> : SkeletonMapper<T> where T : Object
    {
        Dictionary<AvatarMaskBodyPart, bool> foldOpenned = Styles.BodyParts.Keys.ToDictionary(k => k, v => true);

        protected Rect DrawJointDot(Vector2 position, Styles.GUIJoint guiJoint, bool filled, bool selected)
        {
            Texture dotGUI = (guiJoint.optional ? Styles.Dot.frameDotted : Styles.Dot.frame).image;

            Rect rect = new Rect(position.x, position.y, dotGUI.width, dotGUI.height);

            Color oldColor = GUI.color;
            GUI.color = Styles.Dot.color;
            GUI.DrawTexture(rect, dotGUI);
            GUI.color = oldColor;

            if (filled)
                GUI.DrawTexture(rect, Styles.Dot.fill.image);

            if (selected)
                GUI.DrawTexture(rect, Styles.Dot.selection.image);

            return rect;
        }

        string GetUnityDisplayName(JointType jointType, AvatarMaskBodyPart bodyPart = AvatarMaskBodyPart.Root)
        {
            string displayName = jointType.ToUnityBones().ToString();

            if (bodyPart == AvatarMaskBodyPart.LeftArm || bodyPart == AvatarMaskBodyPart.LeftLeg)
                displayName = displayName.Replace("Left", "");
            else if (bodyPart == AvatarMaskBodyPart.RightArm || bodyPart == AvatarMaskBodyPart.RightLeg)
                displayName = displayName.Replace("Right", "");

            return ObjectNames.NicifyVariableName(displayName);
        }

        public void Draw(Dictionary<JointType, T> jointsDict)
        {
            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPartItem in Styles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                Styles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foldOpenned[bodyPart] = EditorGUILayout.BeginFoldoutHeaderGroup(foldOpenned[bodyPart], guiBodyPart.Lable);

                if (foldOpenned[bodyPart])
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                    {
                        JointType jointType = guiJoint.jointType;

                        T jointItem = jointsDict.ContainsKey(jointType) ? jointsDict[jointType] : null;

                        Rect controlRect = EditorGUILayout.GetControlRect();
                        Vector2 position = new Vector2(controlRect.x, controlRect.y);

                        Rect jointRect = DrawJointDot(position, guiJoint, jointItem != null, jointType == SelectJoint);
                        controlRect.xMin += jointRect.width;

                        string displayName = GetUnityDisplayName(jointType, bodyPart);

                        T newJointObject = EditorGUI.ObjectField(controlRect, displayName, jointItem, typeof(T), true) as T;

                        if (jointType == SelectJoint)
                        {
                            System.Type type = typeof(EditorGUIUtility);
                            System.Reflection.FieldInfo field = type.GetField("s_LastControlID", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                            int id = (int)field.GetValue(null);

                            GUIUtility.keyboardControl = id;
                        }

                        if (newJointObject != jointItem)
                            OnDropAction(newJointObject, jointType);

                        if (HandleClick(controlRect))
                            OnSelectedAction(jointType);
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}