using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


using NuitrackAvatar;

namespace NuitrackAvatarEditor
{
    [CustomEditor(typeof(GenericAvatar), true)]
    public class GenericAvatarEditor : AvatarEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GenericAvatar myScript = (GenericAvatar)target;

            Rect rect = DudeRect;

            DrawDude(rect);

            List<GenericAvatar.JointItem> jointItems = myScript.JointItems;
            Dictionary<nuitrack.JointType, GenericAvatar.JointItem> jointItemsDict = jointItems.ToDictionary(k => k.jointType);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (KeyValuePair<nuitrack.JointType, Vector2> bone in Styles.BonesPosition)
            {
                nuitrack.JointType jointType = bone.Key;

                Rect r = BoneIconRect(rect, jointType);
                GUI.DrawTexture(r, Styles.dotFrame.image);

                Transform boneGameObject = null;

                if (jointItemsDict.ContainsKey(jointType))
                    boneGameObject = jointItemsDict[jointType].boneTransform;

                Transform newObj = (Transform) EditorGUILayout.ObjectField(jointType.ToString(), boneGameObject, typeof(Transform), true);

                if (newObj != null)
                {
                    Color oldColor = GUI.color;
                    GUI.color = Color.green;
                    GUI.DrawTexture(r, Styles.dotFill.image);
                    GUI.color = oldColor;
                }

                if (newObj == boneGameObject)
                    continue;

                if (boneGameObject == null && newObj != null)
                {
                    myScript.JointTypes.Add(jointType);

                    GenericAvatar.JointItem jointItem = new GenericAvatar.JointItem()
                    {
                        jointType = jointType,
                        boneTransform = newObj
                    };
                    jointItems.Add(jointItem);
                }
                else if (newObj == null && boneGameObject != null)
                {
                    myScript.JointTypes.Remove(jointType);
                    jointItems.Remove(jointItemsDict[jointType]);
                }
                else
                    jointItemsDict[jointType].boneTransform = newObj;
            }

            EditorGUILayout.EndVertical();
        }
    }
}