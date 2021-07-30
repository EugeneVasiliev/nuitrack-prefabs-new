using System.Collections.Generic;
using UnityEngine;

namespace NuitrackSDK.SafetyGrid
{
    public class SensorPointReplacer : MonoBehaviour
    {
        [SerializeField] Transform CameraPosition;

        [SerializeField] GameObject leftGrid;
        [SerializeField] GameObject rightGrid;
        [SerializeField] GameObject forwardGrid;

        [SerializeField] Material gridMaterial;
        [SerializeField] float XYTrigger = 0.2f;
        [SerializeField] float ZTrigger = 1700;

        Color gridColor;
        public void ChangePlace(Vector3 pos)
        {
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        }

        void Start()
        {
            NuitrackManager.onSkeletonTrackerUpdate += CheckSkeletonPositions;

            gridColor = gridMaterial.color;
            gridColor.a = 0;
            gridMaterial.color = gridColor;
        }

        void OnDestroy()
        {
            NuitrackManager.onSkeletonTrackerUpdate -= CheckSkeletonPositions;
        }

        bool leftVis = false;
        bool rightVis = false;
        bool forwardVis = false;

        void CheckSkeletonPositions(nuitrack.SkeletonData skeletonData)
        {
            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
            if (skeleton == null)
                return;
            List<nuitrack.Joint> joints = new List<nuitrack.Joint>(10);
            joints.Add(skeleton.GetJoint(nuitrack.JointType.Head));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.Torso));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftElbow));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftWrist));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightElbow));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightWrist));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftKnee));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightKnee));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftAnkle));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightAnkle));

            float min = 1;
            float max = 0;
            float minZ = 4000;
            foreach (nuitrack.Joint i in joints)
            {
                float xplus = 0;
                float zplus = 0;
                if (i.Type == nuitrack.JointType.Head || i.Type == nuitrack.JointType.Torso)
                {
                    xplus = 0.15f;
                    zplus = 250f;
                }

                if (i.Proj.X < min)
                {
                    min = i.Proj.X - xplus;
                }
                if (i.Proj.X > max)
                {
                    max = i.Proj.X + xplus;
                }
                if (i.Proj.Z < minZ)
                    minZ = i.Proj.Z - zplus;
            }

            float distance = Mathf.Min(min, 1.0f - max);
            float alpha = 0;
            if (distance < XYTrigger)
                alpha = 1 - distance / XYTrigger;
            if (minZ < 1500)
                alpha = 1;
            else if (1 - (minZ - 1500) / (ZTrigger - 1500) > alpha)
                alpha = 1 - (minZ - 1500) / (ZTrigger - 1500);
            gridColor.a = alpha;
            gridMaterial.color = gridColor;
        }
    }
}
