using System.Collections.Generic;
using UnityEngine;

namespace NuitrackSDK.SafetyGrid
{
    public class SafetyGrid : MonoBehaviour
    {
        [SerializeField] SpriteRenderer frontGrid, leftGrid, rightGrid;

        [SerializeField] float warningDistance = 1.5f;
        [SerializeField] Transform leftHinge, rightHinge;
        [SerializeField] float XYTrigger = 0.2f;
        [SerializeField] float ZTrigger = 1700;

        [SerializeField] bool autoAdjustingFOV = true;
        Color gridColor;

        float bufferPercent = 10;

        void Start()
        {
            NuitrackManager.onSkeletonTrackerUpdate += CheckSkeletonPositions;

            if (autoAdjustingFOV)
            {
                nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

                leftHinge.localEulerAngles = new Vector3(0, mode.HFOV * Mathf.Rad2Deg / 2, 0);
                rightHinge.localEulerAngles = new Vector3(0, -mode.HFOV * Mathf.Rad2Deg / 2, 0);

                //leftGrid.transform.localPosition = new Vector3(0, 0, warningDistance);
                //rightGrid.transform.localPosition = new Vector3(0, 0, warningDistance);

                frontGrid.transform.localPosition = new Vector3(frontGrid.transform.localPosition.x, frontGrid.transform.localPosition.y, warningDistance);
            }
        }

        void OnDestroy()
        {
            NuitrackManager.onSkeletonTrackerUpdate -= CheckSkeletonPositions;
        }

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
            float minZ = float.MaxValue;
            foreach (nuitrack.Joint i in joints)
            {
                //float xplus = 0;
                //float zplus = 0;
                //if (i.Type == nuitrack.JointType.Head || i.Type == nuitrack.JointType.Torso)
                //{
                //    xplus = 0.15f;
                //    zplus = 250f;
                //}

                //if (i.Proj.X < min)
                //{
                //    min = i.Proj.X - xplus;
                //}
                //if (i.Proj.X > max)
                //{
                //    max = i.Proj.X + xplus;
                //}
                //if (i.Proj.Z < minZ)
                //    minZ = i.Proj.Z - zplus;

                float posZ = i.ToVector3().z / 100 / 10;
                if (minZ > posZ)
                    minZ = posZ;
            }

            float distance = Mathf.Min(min, 1.0f - max);
            float alpha = 0;
            if (distance < XYTrigger)
                alpha = 1 - distance / XYTrigger;

            if (minZ < warningDistance)
            {
                alpha = (warningDistance - minZ) / (warningDistance / 100 * bufferPercent);
            }
            else
                alpha = 0;

            gridColor = frontGrid.color;
            gridColor.a = alpha;

            frontGrid.color = gridColor;
            leftGrid.color = gridColor;
            rightGrid.color = gridColor;
        }
    }
}
