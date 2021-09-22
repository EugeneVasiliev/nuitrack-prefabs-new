using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonJointListUI<T> : SkeletonMapper<T> where T : Object
    {
        Dictionary<AvatarMaskBodyPart, bool> foldOpenned = Styles.BodyParts.Keys.ToDictionary(k => k, v => true);

        Dictionary<JointType, int> controlsID = new Dictionary<JointType, int>();

        public override JointType SelectJoint 
        { 
            get => base.SelectJoint; 
            set
            {
                base.SelectJoint = value;

                if(controlsID.ContainsKey(value))
                    GUIUtility.keyboardControl = controlsID[value];
            }
        }

        Rect DrawJointDot(Vector2 position, Styles.GUIJoint guiJoint, bool filled, bool selected)
        {
            Texture dotGUI = (guiJoint.Optional ? Styles.Dot.frameDotted : Styles.Dot.frame).image;

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

        int GetLastControllID()
        {
            System.Type type = typeof(EditorGUIUtility);
            System.Reflection.FieldInfo field = type.GetField("s_LastControlID", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            return (int)field.GetValue(null);
        }

        public void Draw(Dictionary<JointType, T> jointsDict)
        {
            controlsID.Clear();

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
                        JointType jointType = guiJoint.JointType;

                        T jointItem = jointsDict.ContainsKey(jointType) ? jointsDict[jointType] : null;

                        Rect controlRect = EditorGUILayout.GetControlRect();
                        Vector2 position = new Vector2(controlRect.x, controlRect.y);

                        Rect jointRect = DrawJointDot(position, guiJoint, jointItem != null, jointType == SelectJoint);
                        controlRect.xMin += jointRect.width;

                        string displayName = GetUnityDisplayName(jointType, bodyPart);

                        T newJointObject = EditorGUI.ObjectField(controlRect, displayName, jointItem, typeof(T), true) as T;
                        controlsID.Add(jointType, GetLastControllID());

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