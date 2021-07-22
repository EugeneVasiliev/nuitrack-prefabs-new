using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace NuitrackAvatarEditor
{
    [CustomEditor(typeof(NuitrackAvatar.HumanoidAvatar), true)]
    public class HumanoidAvatarEditor : AvatarEditor
    {
        Color mainColor = Color.green;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            NuitrackAvatar.HumanoidAvatar myScript = (NuitrackAvatar.HumanoidAvatar)target;

            Rect rect = DudeRect;
            DrawDude(rect, mainColor);

            //SerializedProperty acSP = serializedObject.FindProperty("jointTypes");

            List<nuitrack.JointType> jointTypes = myScript.JointTypes;

            //foreach (KeyValuePair<nuitrack.JointType, Vector2> bone in Styles.BonesPosition)
            //{
            //    nuitrack.JointType jointType = bone.Key;
            //    Rect r = BoneIconRect(rect, jointType);

            //    bool haveBone = jointTypes.Contains(bone.Key);

            //    Color oldColor = GUI.color;
            //    GUI.color = haveBone ? Color.green : Color.white;

            //    GUI.DrawTexture(r, Styles.dotFrame.image);

            //    if (haveBone)
            //        GUI.DrawTexture(r, Styles.dotFill.image);

            //    GUI.color = oldColor;

            //    Event evt = Event.current;
                
            //    if (evt.type == EventType.MouseDown && r.Contains(evt.mousePosition))
            //    {
            //        Undo.RecordObject(myScript, "Change " + bone.Key.ToString() + " used");

            //        if (haveBone)
            //            jointTypes.Remove(bone.Key);
            //        else
            //            jointTypes.Add(bone.Key);
            //    }
            //}
        }
    }
}