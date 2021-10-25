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

        [Range(0, 1)]
        [SerializeField] float sensitivity = 0.15f;

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
            leftGrid.transform.localPosition = new Vector3(0, 0, leftGrid.size.x * leftGrid.transform.localScale.x / 2 + sideDistance);
            rightGrid.transform.localPosition = new Vector3(0, 0, rightGrid.size.x * rightGrid.transform.localScale.x / 2 + sideDistance);
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
            float proximityLeft = 0, proximityRight = 0;
            foreach (nuitrack.Joint joint in joints)
            {
                float jointPosX = joint.ToVector3().x / 1000;
                float posZ = joint.ToVector3().z / 1000; //mm into unityunits

                float angle = fov / 2;
                float sideDistance = posZ / Mathf.Cos(angle * Mathf.Deg2Rad);
                float frontWidth = Mathf.Sqrt(-(posZ * posZ) + sideDistance * sideDistance);
                float disttoside = jointPosX / frontWidth;

                if (proximityLeft < disttoside)
                    proximityLeft = disttoside;

                if (proximityRight > disttoside)
                    proximityRight = disttoside;

                if (minZ > posZ)
                    minZ = posZ;
            }

            ChangeAlpha(frontGrid, (warningDistance - minZ) / (warningDistance * sensitivity));
            ChangeAlpha(leftGrid, 1.0f - (1.0f - proximityLeft) / sensitivity);
            ChangeAlpha(rightGrid, 1.0f - (1.0f - (-proximityRight)) / sensitivity);
        }

        void ChangeAlpha(SpriteRenderer spriteRenderer, float alpha)
        {
            Color gridColor = spriteRenderer.color;
            gridColor.a = alpha;
            spriteRenderer.color = gridColor;
        }
    }
}
