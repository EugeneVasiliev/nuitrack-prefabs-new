using System.Collections.Generic;
using UnityEngine;

namespace NuitrackSDK.SafetyGrid
{
    public class SafetyGrid : MonoBehaviour
    {
        [SerializeField] SpriteRenderer frontGrid, leftGrid, rightGrid;

        [SerializeField] float warningDistance = 1.5f;
        [SerializeField] float fov = 60;
        [SerializeField] bool autoAdjustingFOV = true;
        [SerializeField] Transform leftHinge, rightHinge;
        [SerializeField] Transform leftPos, rightPos;

        Color gridColor;

        float bufferPercent = 10;

        void Start()
        {
            NuitrackManager.onSkeletonTrackerUpdate += CheckSkeletonPositions;

            float angle = fov / 2;

            if (autoAdjustingFOV)
                angle = NuitrackManager.DepthSensor.GetOutputMode().HFOV * Mathf.Rad2Deg / 2;

            leftHinge.localEulerAngles = new Vector3(0, angle, 0);
            rightHinge.localEulerAngles = new Vector3(0, -angle, 0);
            frontGrid.transform.localPosition = new Vector3(frontGrid.transform.localPosition.x, frontGrid.transform.localPosition.y, warningDistance);
            float sideDistance = warningDistance / Mathf.Cos(angle * Mathf.Deg2Rad);
            leftPos.transform.localPosition = new Vector3(0, 0, sideDistance);
            rightPos.transform.localPosition = new Vector3(0, 0, sideDistance);
            float frontWidth = Mathf.Sqrt(-(warningDistance * warningDistance) + sideDistance * sideDistance);
            frontGrid.size = new Vector2(frontWidth * 2 / frontGrid.transform.localScale.x, frontGrid.size.y);
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

            float minZ = float.MaxValue;
            foreach (nuitrack.Joint i in joints)
            {
                float posZ = i.ToVector3().z / 1000; //mm into unityunits
                if (minZ > posZ)
                    minZ = posZ;
            }

            float alpha = (warningDistance - minZ) / (warningDistance / 100 * bufferPercent);

            gridColor = frontGrid.color;
            gridColor.a = alpha;

            frontGrid.color = gridColor;
            leftGrid.color = gridColor;
            rightGrid.color = gridColor;
        }
    }
}
