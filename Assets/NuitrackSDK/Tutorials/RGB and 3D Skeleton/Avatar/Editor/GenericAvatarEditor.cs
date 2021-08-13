using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(GenericAvatar), true)]
    public class GenericAvatarEditor : BaseAvatarEditor
    {
        Dictionary<AvatarMaskBodyPart, bool> foldOpenned;

        nuitrack.JointType selectJoint = nuitrack.JointType.None;

        void OnEnable()
        {
            foldOpenned = Styles.BodyParts.Keys.ToDictionary(k => k, v => true);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GenericAvatar myScript = (GenericAvatar)target;

            Rect rect = GetAvatarViewRect();

            ref List<ModelJoint> jointItems = ref myScript.JointItems;

            Dictionary<nuitrack.JointType, ModelJoint> jointsDict = jointItems.ToDictionary(k => k.jointType);

            List<AvatarMaskBodyPart> bodyParts = GetActiveBodyParts(jointsDict.Keys);
            DrawDude(rect, bodyParts);

            foreach (KeyValuePair<AvatarMaskBodyPart, Styles.GUIBodyPart> bodyPartItem in Styles.BodyParts)
            {
                AvatarMaskBodyPart bodyPart = bodyPartItem.Key;
                Styles.GUIBodyPart guiBodyPart = bodyPartItem.Value;

                foldOpenned[bodyPart] = EditorGUILayout.BeginFoldoutHeaderGroup(foldOpenned[bodyPart], guiBodyPart.Lable);

                bool drawItems = foldOpenned[bodyPart];

                if (drawItems)
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    nuitrack.JointType jointType = guiJoint.jointType;
                    Transform jointTransform = jointsDict.ContainsKey(jointType) ? jointsDict[jointType].bone : null;
                   
                    Rect jointPointRect = DrawAvatarJointIcon(rect, guiJoint, jointTransform != null, jointType == selectJoint);
                    Transform newJoint = HandleDragDrop(jointPointRect);
                    
                    if (newJoint != null)
                    {
                        AddJoint(jointType, newJoint, ref jointsDict);
                        EditorGUIUtility.PingObject(newJoint);
                        selectJoint = jointType;

                        jointTransform = newJoint;
                    }

                    if (drawItems)
                    {
                        Rect controlRect = EditorGUILayout.GetControlRect();
                        Vector2 position = new Vector2(controlRect.x, controlRect.y);

                        Rect jointRect = DrawJointDot(position, guiJoint, jointTransform != null, jointType == selectJoint);
                        controlRect.xMin += jointRect.width;

                        string displayName = jointType.ToUnityBones().ToString();

                        if (bodyPart == AvatarMaskBodyPart.LeftArm || bodyPart == AvatarMaskBodyPart.LeftLeg)
                            displayName = displayName.Replace("Left", "");
                        else if (bodyPart == AvatarMaskBodyPart.RightArm || bodyPart == AvatarMaskBodyPart.RightLeg)
                            displayName = displayName.Replace("Right", "");

                        displayName = ObjectNames.NicifyVariableName(displayName);

                        jointTransform = EditorGUI.ObjectField(controlRect, displayName, jointTransform, typeof(Transform), true) as Transform;
                        AddJoint(jointType, jointTransform, ref jointsDict);

                        if (PingObject(controlRect, jointTransform))
                            selectJoint = jointType;
                    }

                    if (PingObject(jointPointRect, jointTransform))
                        selectJoint = jointType;
                }

                if (drawItems)
                    EditorGUILayout.EndVertical();

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        void AddJoint(nuitrack.JointType jointType, Transform objectTransform, ref Dictionary<nuitrack.JointType, ModelJoint> jointsDict)
        {
            GenericAvatar myScript = (GenericAvatar)target;

            ref List<ModelJoint> jointItems = ref myScript.JointItems;

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

                    jointItems.Add(modelJoint);
                    jointsDict.Add(jointType, modelJoint);
                }
            }
            else if (jointsDict.ContainsKey(jointType))
            {
                Undo.RecordObject(myScript, "Remove avatar joint object");
                jointItems.Remove(jointsDict[jointType]);

                jointsDict.Remove(jointType);
            }
        }
    }
}