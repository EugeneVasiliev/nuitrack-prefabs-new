using UnityEngine;
using UnityEditor;

using System.Linq;
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

            if (avatar.ModelJoints == null)
                avatar.ModelJoints = new List<ModelJoint>();

            List<JointType> avatarJoints = avatar.ModelJoints.Select(k => k.jointType).ToList();
  
            int index = 0;

            foreach (Styles.GUIBodyPart guiBodyPart in Styles.BodyParts.Values)
                foreach (Styles.GUIJoint guiJoint in guiBodyPart.guiJoint)
                {
                    if (!avatarJoints.Contains(guiJoint.jointType))
                    {
                        ModelJoint modelJoint = new ModelJoint() { jointType = guiJoint.jointType };
                        avatar.ModelJoints.Insert(index, modelJoint);
                    }

                    index++;
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