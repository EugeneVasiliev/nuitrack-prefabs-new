using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(Avatar), true)]
    public class AvatarEditor : UnityEditor.Editor
    {
        protected static class Styles
        {
            public class JointItem
            {
                public nuitrack.JointType jointType;
                public Vector2 mapPosition;
                public bool optional = false;

                public JointItem(nuitrack.JointType jointType, Vector2 mapPosition, bool optional = false)
                {
                    this.jointType = jointType;
                    this.mapPosition = mapPosition;
                    this.optional = optional;
                }
            }

            public static int kIconSize = 19;

            //static Color kBoneValid = new Color(0, 0.75f, 0, 1.0f);
            //static Color kBoneInvalid = new Color(1.0f, 0.3f, 0.25f, 1.0f);
            //static Color kBoneInactive = Color.gray;
            //static Color kBoneSelected = new Color(0.4f, 0.7f, 1.0f, 1.0f);
            //static Color kBoneDrop = new Color(0.1f, 0.7f, 1.0f, 1.0f);

            public static GUIContent dotFill = EditorGUIUtility.IconContent("AvatarInspector/DotFill");
            public static GUIContent dotFrame = EditorGUIUtility.IconContent("AvatarInspector/DotFrame");
            public static GUIContent dotFrameDotted = EditorGUIUtility.IconContent("AvatarInspector/DotFrameDotted");
            public static GUIContent dotSelection = EditorGUIUtility.IconContent("AvatarInspector/DotSelection");

            //public static List<AvatarMaskBodyPart> BodyPartsOrder = new List<AvatarMaskBodyPart>()
            //{
            //    AvatarMaskBodyPart.Body,
            //    AvatarMaskBodyPart.Head,

            //    AvatarMaskBodyPart.LeftArm,
            //    AvatarMaskBodyPart.RightArm,

            //    AvatarMaskBodyPart.LeftLeg,
            //    AvatarMaskBodyPart.RightLeg
            //};

            public static Dictionary<AvatarMaskBodyPart, List<JointItem>> JointItems = new Dictionary<AvatarMaskBodyPart, List<JointItem>>()
            {
                {
                    AvatarMaskBodyPart.Body, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.Waist, new Vector2(0.00f, 0.08f)),
                        new JointItem(nuitrack.JointType.Torso, new Vector2(0.00f, 0.35f)),

                        new JointItem(nuitrack.JointType.LeftCollar, new Vector2(0.14f, 0.60f), true),
                        new JointItem(nuitrack.JointType.RightCollar, new Vector2(-0.14f, 0.60f), true),
                    }
                },

                {
                    AvatarMaskBodyPart.Head, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.Neck, new Vector2(0.00f, 0.66f), true),
                        new JointItem(nuitrack.JointType.Head, new Vector2(0.00f, 0.76f), true)
                    }
                },

                {
                    AvatarMaskBodyPart.LeftLeg, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.LeftHip, new Vector2(0.16f, 0.01f)),
                        new JointItem(nuitrack.JointType.LeftKnee, new Vector2(0.21f, -0.40f)),
                        new JointItem(nuitrack.JointType.LeftAnkle, new Vector2(0.23f, -0.80f)),
                    }

                },

                {
                    AvatarMaskBodyPart.RightLeg, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.RightHip, new Vector2(-0.16f, 0.01f)),
                        new JointItem(nuitrack.JointType.RightKnee,  new Vector2(-0.21f, -0.40f)),
                        new JointItem(nuitrack.JointType.RightAnkle, new Vector2(-0.23f, -0.80f))
                    }
                },

                {
                    AvatarMaskBodyPart.LeftArm, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.LeftShoulder, new Vector2(0.30f, 0.57f)),
                        new JointItem(nuitrack.JointType.LeftElbow, new Vector2(0.48f, 0.30f)),
                        new JointItem(nuitrack.JointType.LeftWrist, new Vector2(0.66f, 0.03f))
                    }
                },

                {
                    AvatarMaskBodyPart.RightArm, new List<JointItem>()
                    {
                        new JointItem(nuitrack.JointType.RightShoulder, new Vector2(-0.30f, 0.57f)),
                        new JointItem(nuitrack.JointType.RightElbow, new Vector2(-0.48f, 0.30f)),
                        new JointItem(nuitrack.JointType.RightWrist, new Vector2(-0.66f, 0.03f))
                    }
                }
            };


            //public static Dictionary<nuitrack.JointType, Vector2> BonesPosition = new Dictionary<nuitrack.JointType, Vector2>()
            //{ 
                //{ nuitrack.JointType.Waist, new Vector2(0.00f, 0.08f) },
                
                //{ nuitrack.JointType.LeftHip, new Vector2(0.16f, 0.01f) },
                //{ nuitrack.JointType.RightHip, new Vector2(-0.16f, 0.01f) },
                
                //{ nuitrack.JointType.LeftKnee,  new Vector2(0.21f, -0.40f) },
                //{ nuitrack.JointType.RightKnee,  new Vector2(-0.21f, -0.40f) },

                //{ nuitrack.JointType.LeftAnkle, new Vector2(0.23f, -0.80f) },
                //{ nuitrack.JointType.RightAnkle, new Vector2(-0.23f, -0.80f) },

                //{ HumanBodyBones.Spine, new Vector2(0.00f, 0.20f) },
                //{ nuitrack.JointType.Torso, new Vector2(0.00f, 0.35f) },
                //{ HumanBodyBones.UpperChest, new Vector2(0.00f, 0.50f) },
                //{ nuitrack.JointType.Neck, new Vector2(0.00f, 0.66f) },
                //{ nuitrack.JointType.Head, new Vector2(0.00f, 0.76f) },

                //{ nuitrack.JointType.LeftCollar, new Vector2(0.14f, 0.60f) },
                //{ nuitrack.JointType.RightCollar, new Vector2(-0.14f, 0.60f) },

                //{ nuitrack.JointType.LeftShoulder, new Vector2(0.30f, 0.57f) },
                //{ nuitrack.JointType.RightShoulder, new Vector2(-0.30f, 0.57f) },

                //{ nuitrack.JointType.LeftElbow, new Vector2(0.48f, 0.30f) },
                //{ nuitrack.JointType.RightElbow, new Vector2(-0.48f, 0.30f) },

                //{ nuitrack.JointType.LeftWrist, new Vector2(0.66f, 0.03f) },
                //{ nuitrack.JointType.RightWrist, new Vector2(-0.66f, 0.03f) },

                // 
                //{ nuitrack.JointType.LeftFoot, new Vector2(0.25f, -0.89f) },
                //{ nuitrack.JointType.RightFoot, new Vector2(-0.25f, -0.89f) }
            //};

            public static GUIContent UnityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");

            public static Dictionary<AvatarMaskBodyPart, List<GUIContent>> BodyParts = new Dictionary<AvatarMaskBodyPart, List<GUIContent>>()
            {
                { AvatarMaskBodyPart.Body, new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Torso") } },
                { AvatarMaskBodyPart.Head, new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/Head") } },

                { AvatarMaskBodyPart.LeftLeg, new List<GUIContent>() {  EditorGUIUtility.IconContent("AvatarInspector/LeftLeg") }  },
                { AvatarMaskBodyPart.RightLeg, new List<GUIContent>() { EditorGUIUtility.IconContent("AvatarInspector/RightLeg") }  },

                { AvatarMaskBodyPart.LeftArm, 
                    new List<GUIContent>() 
                    { 
                        EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
                        EditorGUIUtility.IconContent("AvatarInspector/LeftFingers")
                    } 
                },

                { AvatarMaskBodyPart.RightArm, 
                    new List<GUIContent>() 
                    { 
                        EditorGUIUtility.IconContent("AvatarInspector/RightArm"),
                        EditorGUIUtility.IconContent("AvatarInspector/RightFingers")
                    } 
                },
            };
        }

        protected Rect DudeRect
        {
            get
            {
                Rect rect = GUILayoutUtility.GetRect(Styles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.UnityDude.image.width));
                rect.x += (EditorGUIUtility.currentViewWidth - rect.width) / 2;

                return rect;
            }
        }

        protected void DrawDude(Rect rect, Color mainColor, Color disableColor, List<AvatarMaskBodyPart> filled)
        {
            Color oldColor = GUI.color;

            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            GUI.DrawTexture(rect, Styles.UnityDude.image);

            foreach(KeyValuePair<AvatarMaskBodyPart, List<GUIContent>> bodyPart in Styles.BodyParts)
            {
                GUI.color = filled != null && filled.Contains(bodyPart.Key) ? mainColor : disableColor;

                foreach (GUIContent guiContent in bodyPart.Value)
                    GUI.DrawTexture(rect, guiContent.image);
            }

            GUI.color = oldColor;
        }

        //protected Rect BoneIconRect(Rect baseRect, AvatarMaskBodyPart bodyPart, nuitrack.JointType jointType)
        //{
        //    Vector2 pos = Styles.JointItems[bodyPart][jointType];

        //    pos.y *= -1; // because higher values should be up
        //    pos.Scale(new Vector2(baseRect.width * 0.5f, baseRect.height * 0.5f));
        //    pos = baseRect.center + pos;
        //    int kIconSize = Styles.kIconSize;
        //    Rect boneRect = new Rect(pos.x - kIconSize * 0.5f, pos.y - kIconSize * 0.5f, kIconSize, kIconSize);

        //    return boneRect;
        //}

    }
}