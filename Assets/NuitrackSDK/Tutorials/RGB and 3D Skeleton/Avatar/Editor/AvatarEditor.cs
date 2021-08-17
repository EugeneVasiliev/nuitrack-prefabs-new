using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using nuitrack;

namespace NuitrackSDK.Avatar.Editor
{
    [CustomEditor(typeof(Avatar), true)]
    public class AvatarEditor : BaseAvatarEditor
    {
        protected override void OnEnable()
        {
            Avatar avatar = serializedObject.targetObject as Avatar;

            if (avatar.ModelJoints == null || avatar.ModelJoints.Count == 0)
            {
                avatar.ModelJoints = new List<ModelJoint>();

                foreach (Styles.GUIBodyPart guiBodyPart in Styles.BodyParts.Values)
                    foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                    {
                        ModelJoint modelJoint = new ModelJoint() { jointType = guiJoint.jointType };
                        avatar.ModelJoints.Add(modelJoint);
                    }
            }

            base.OnEnable();
        }

        protected override void AddJoint(JointType jointType, Transform objectTransform, ref Dictionary<JointType, ModelJoint> jointsDict)
        {
            Avatar avatar = serializedObject.targetObject as Avatar;

            Undo.RecordObject(avatar, "Avatar mapping modified");
            jointsDict[jointType].bone = objectTransform;
        }
    }
}