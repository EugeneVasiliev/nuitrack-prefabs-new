using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonMapperListGUI<T> : SkeletonMapper<T> where T : Object
    {
        Dictionary<AvatarMaskBodyPart, bool> foldOpenned = SkeletonMapperStyles.BodyParts.Keys.ToDictionary(k => k, v => true);

        Dictionary<JointType, int> controlsID = new Dictionary<JointType, int>();

        public override JointType SelectedJoint
        {
            get => base.SelectedJoint;
            set
            {
                base.SelectedJoint = value;

                if (controlsID.ContainsKey(value))
                    GUIUtility.keyboardControl = controlsID[value];
            }
        }

        List<JointType> jointMask = null;

        /// <summary>
        /// View of the list of joints.
        /// </summary>
        /// <param name="jointMask">The mask of the displayed joints. If null, all available joints will be drawn.</param>
        public SkeletonMapperListGUI(List<JointType> jointMask)
        {
            this.jointMask = jointMask;
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

        /// <summary>
        /// Draw a list of joints
        /// </summary>
        /// <param name="jointsDict">Dictionary of joints and joint targets</param>
        public void Draw(Dictionary<JointType, T> jointsDict)
        {
            controlsID.Clear();

            foreach (KeyValuePair<AvatarMaskBodyPart, SkeletonMapperStyles.GUIBodyPart> bodyPartItem in SkeletonMapperStyles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                SkeletonMapperStyles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foldOpenned[bodyPart] = EditorGUILayout.BeginFoldoutHeaderGroup(foldOpenned[bodyPart], guiBodyPart.Lable);

                if (foldOpenned[bodyPart])
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    foreach (SkeletonMapperStyles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                    {
                        JointType jointType = guiJoint.JointType;

                        if (jointMask.Contains(jointType))
                        {
                            T jointItem = jointsDict.ContainsKey(jointType) ? jointsDict[jointType] : null;

                            Rect controlRect = EditorGUILayout.GetControlRect();
                            Vector2 position = new Vector2(controlRect.x, controlRect.y);

                            Rect jointRect = SkeletonMapperStyles.Dot.Draw(position, guiJoint.Optional, jointItem != null, jointType == SelectedJoint);
                            controlRect.xMin += jointRect.width;

                            string displayName = GetUnityDisplayName(jointType, bodyPart);

                            T newJointObject = EditorGUI.ObjectField(controlRect, displayName, jointItem, typeof(T), true) as T;
                            controlsID.Add(jointType, GetLastControllID());

                            if (newJointObject != jointItem)
                                OnDropAction(newJointObject, jointType);

                            if (HandleClick(controlRect))
                                OnSelectedAction(jointType);
                        }
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}