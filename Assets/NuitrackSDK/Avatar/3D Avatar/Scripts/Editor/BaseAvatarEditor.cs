using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;

using NuitrackSDK;
using NuitrackSDK.Avatar;

using Reflection = System.Reflection;

namespace NuitrackSDKEditor.Avatar
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
            public JointType JointType { get; private set; }
            public Vector2 MapPosition { get; private set; }
            public bool Optional { get; private set; } = false;

            public GUIJoint(JointType jointType, Vector2 mapPosition, bool optional = false)
            {
                JointType = jointType;
                MapPosition = mapPosition;
                Optional = optional;
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
    public class BaseAvatarEditor : Editor
    {
        SkeletonMapperUI<Transform> skeletonMapper = null;
        SkeletonJointListUI<Transform> skeletonJointListUI = null;

        protected JointType SelectJoint { get; set; } = JointType.None;

        Reflection.FieldInfo[] GetFieldInfo(System.Type typeObject)
        {
            Reflection.FieldInfo[] fields = typeObject.GetFields(
                Reflection.BindingFlags.Instance | 
                Reflection.BindingFlags.NonPublic | 
                Reflection.BindingFlags.Public);

            if (typeObject.BaseType != typeof(MonoBehaviour))
            {
                Reflection.FieldInfo[] baseTypeFields = GetFieldInfo(typeObject.BaseType);
                fields = fields.Concat(baseTypeFields).ToArray();
            }

            return fields;
        }

        protected string[] GetHideInNuitrackSDKInspectorFieldNames()
        {
            Reflection.FieldInfo[] fields = GetFieldInfo(target.GetType());

            string[] excludeFieldsNames = fields.Where(f => f.IsDefined(typeof(HideInNuitrackSDKInspector), true)).Select(f => f.Name).ToArray();
            excludeFieldsNames = excludeFieldsNames.Append("m_Script").ToArray();

            return excludeFieldsNames;
        }

        protected virtual void AddJoint(JointType jointType, Transform objectTransform)
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            ref List<ModelJoint> modelJoints = ref myScript.ModelJoints;

            Dictionary<JointType, ModelJoint> jointsDict = modelJoints.ToDictionary(k => k.jointType);

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

        protected virtual void RemoveJoint(JointType jointType)
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            ref List<ModelJoint> modelJoints = ref myScript.ModelJoints;
            Dictionary<JointType, ModelJoint> jointsDict = modelJoints.ToDictionary(k => k.jointType);

            if (jointsDict.ContainsKey(jointType))
            {
                Undo.RecordObject(myScript, "Remove avatar joint object");
                modelJoints.Remove(jointsDict[jointType]);
            }
        }

        protected virtual void OnEnable()
        {
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
            if (newJoint != null)
            {
                AddJoint(jointType, newJoint);
                
                EditorGUIUtility.PingObject(newJoint);
                SelectJoint = jointType;

                skeletonMapper.SelectJoint = SelectJoint;
                skeletonJointListUI.SelectJoint = SelectJoint;
            }
            else
                RemoveJoint(jointType);
        }

        void SkeletonMapper_onSelected(JointType jointType)
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            SelectJoint = jointType;
            skeletonMapper.SelectJoint = SelectJoint;
            skeletonJointListUI.SelectJoint = SelectJoint;

            Dictionary<JointType, ModelJoint> jointsDict = myScript.ModelJoints.ToDictionary(k => k.jointType);

            if (jointsDict[jointType].bone != null)
                EditorGUIUtility.PingObject(jointsDict[jointType].bone);
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
            string[] excludeFieldsNames = GetHideInNuitrackSDKInspectorFieldNames();        
            DrawPropertiesExcluding(serializedObject, excludeFieldsNames);
            serializedObject.ApplyModifiedProperties();
        }   

        protected void DrawSkeletonMap()
        {
            BaseAvatar myScript = serializedObject.targetObject as BaseAvatar;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Avatar map", EditorStyles.boldLabel);

            if (skeletonMapper != null)
            {
                List<JointType> activeJoints = myScript.ModelJoints.Where(j => j.bone != null).Select(j => j.jointType).ToList();
                skeletonMapper.Draw(activeJoints);
            }

            if (skeletonJointListUI != null)
            {
                Dictionary<JointType, Transform> jointDict = myScript.ModelJoints.Where(v => v.bone != null).ToDictionary(k => k.jointType, v => v.bone);
                skeletonJointListUI.Draw(jointDict);
            }
        }
    }
}