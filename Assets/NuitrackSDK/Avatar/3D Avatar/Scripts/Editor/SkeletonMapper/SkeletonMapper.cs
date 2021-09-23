using UnityEngine;

using nuitrack;


namespace NuitrackSDKEditor.Avatar
{
    public class SkeletonMapper<T> where T : Object
    {
        public delegate void DropHandler(T dropObject, JointType jointType);
        public delegate void SelectHandler(JointType jointType);

        public event DropHandler onDrop;
        public event SelectHandler onSelected;

        public virtual JointType SelectedJoint { get; set; } = JointType.None;

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

        /// <summary>
        /// Check whether the click was in the specified <see cref="Rect"/>
        /// </summary>
        /// <param name="clickRect">Target rect/></param>
        /// <returns>True if there was a click, otherwise false</returns>
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
}