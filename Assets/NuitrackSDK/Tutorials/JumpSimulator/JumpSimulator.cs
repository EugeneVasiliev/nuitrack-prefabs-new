using System;

using UnityEngine;
using UnityEngine.UI;


public class JumpSimulator : MonoBehaviour
{
    [SerializeField] float deltaAnkleFloor = 0.1f;
    Plane floorPlane;

    [SerializeField] RectTransform baseRect;
    [SerializeField] AspectRatioFitter aspectRatioFitter;

    [Header ("Floor UI")]
    [SerializeField] RectTransform floorLine;

    [Header ("Best jump UI")]
    [SerializeField] RectTransform bestJumpLine;
    [SerializeField] Text bestJumpLable;

    [Header("Current jump UI")]
    [SerializeField] RectTransform currentJumpLine;
    [SerializeField] Text currentJumpLable;

    [Header ("Test options")]
    [SerializeField] SkeletonEmulator skeletonEmulator;
    [SerializeField] Transform floorBasePointTransform;
    [SerializeField] Transform floorNormalVectorTransform;

    public float TotalMaxJumpHeight
    {
        get;
        private set;
    }

    public float CurrentMaxJumpHeight
    {
        get;
        private set;
    }

    Vector3 JointPoisition(nuitrack.Skeleton skeleton, nuitrack.JointType jointType)
    {
        return skeleton.GetJoint(jointType).ToVector3() * 0.001f;

        //return skeletonEmulator.JointPoisition(jointType);
    }

    bool LegOnFloor(nuitrack.Skeleton skeleton)
    {
        Vector3 leftAnkle = JointPoisition(skeleton, nuitrack.JointType.LeftAnkle);
        Vector3 rightAnkle = JointPoisition(skeleton, nuitrack.JointType.RightAnkle);

        Vector3 floorLeftAnklePoint = floorPlane.ClosestPointOnPlane(leftAnkle);
        Vector3 floorRightAnklePoint = floorPlane.ClosestPointOnPlane(rightAnkle);

        return Vector3.Distance(leftAnkle, floorLeftAnklePoint) <= deltaAnkleFloor || Vector3.Distance(rightAnkle, floorRightAnklePoint) <= deltaAnkleFloor;
    }

    float GetLowerJoint(nuitrack.Skeleton skeleton, out nuitrack.JointType lowerJointType)
    {
        lowerJointType = nuitrack.JointType.Head;
        float jumpHeight = float.MaxValue;

        foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
        {
            Vector3 jointPoint = JointPoisition(skeleton, jointType);
            Vector3 floorPoint = floorPlane.ClosestPointOnPlane(jointPoint);

            float currentDistance = Vector3.Distance(jointPoint, floorPoint);

            if (currentDistance < jumpHeight)
            {
                lowerJointType = jointType;
                jumpHeight = currentDistance;
            }
        }

        return jumpHeight;
    }

    Vector3 ScreenPoint(Vector3 realPoint)
    {
        Vector3 cameraSpacePoint = Camera.main.transform.TransformPoint(realPoint);
        return Camera.main.WorldToScreenPoint(cameraSpacePoint);
    }

    void Start()
    {
        DisplayLines(false);
    }


    void DisplayLines(bool visible)
    {
        currentJumpLine.gameObject.SetActive(visible);
        floorLine.gameObject.SetActive(visible);
        bestJumpLine.gameObject.SetActive(visible);
    }
    
    void Update()
    {
        if(NuitrackManager.ColorFrame != null)
            aspectRatioFitter.aspectRatio = (float)(NuitrackManager.ColorFrame.Cols) / NuitrackManager.ColorFrame.Rows;

        if (NuitrackManager.UserFrame == null || CurrentUserTracker.CurrentSkeleton == null)
        {
            DisplayLines(false);
            return;
        }

        DisplayLines(true);

        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        Vector3 floorBasePoint = NuitrackManager.UserFrame.Floor.ToVector3() * 0.001f;
        Vector3 floorNormalVector = NuitrackManager.UserFrame.FloorNormal.ToVector3().normalized;

        //Vector3 floorBasePoint = floorBasePointTransform.position;
        //Vector3 floorNormalVector = (floorBasePointTransform.position - floorNormalVectorTransform.position).normalized;

        floorPlane = new Plane(floorNormalVector, floorBasePoint);

        Vector3 userWaist = JointPoisition(skeleton, nuitrack.JointType.Waist);
        Vector3 userFloor = floorPlane.ClosestPointOnPlane(userWaist);
        Vector3 screenFloorPoint = ScreenPoint(userFloor);

        floorLine.anchoredPosition = new Vector2(0, baseRect.rect.height - screenFloorPoint.y);


        Vector3 bestJump = userFloor + floorNormalVector * TotalMaxJumpHeight;
        Vector3 screenBestUserJump = ScreenPoint(bestJump);

        bestJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height - screenBestUserJump.y);


        nuitrack.JointType lowerJoint;
        float jumpHeight = GetLowerJoint(skeleton, out lowerJoint);

        Vector3 jointPosition = JointPoisition(skeleton, lowerJoint);
        Vector3 currentJumpScreenPoint = ScreenPoint(jointPosition);

        currentJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height - currentJumpScreenPoint.y);

        currentJumpLable.text = string.Format("Current: {0:F2}", CurrentMaxJumpHeight);

        if (!LegOnFloor(skeleton))
        {
            if (jumpHeight > CurrentMaxJumpHeight)
                CurrentMaxJumpHeight = jumpHeight;

            if (CurrentMaxJumpHeight > TotalMaxJumpHeight)
            {
                TotalMaxJumpHeight = CurrentMaxJumpHeight;
                bestJumpLable.text = string.Format("Best: {0:F2}", TotalMaxJumpHeight);
            }
        }
    }
   

    //void OnDrawGizmos()
    //{
    //    Vector3 floorBasePoint = floorBasePointTransform.position;
    //    Vector3 floorNormalVector = (floorBasePointTransform.position - floorNormalVectorTransform.position).normalized;

    //    floorPlane = new Plane(floorNormalVector, floorBasePoint);

    //    UnityEditor.Handles.color = new Color(0.5f, 1, 0.5f, 1f);
    //    UnityEditor.Handles.DrawWireDisc(floorBasePoint, floorNormalVector, 2);

    //    UnityEditor.Handles.DrawLine(floorBasePointTransform.position, floorNormalVectorTransform.position);

    //    Vector3 leftAnkle = JointPoisition(null, nuitrack.JointType.LeftAnkle);
    //    Vector3 rightAnkle = JointPoisition(null, nuitrack.JointType.RightAnkle);

    //    Vector3 floorLeftAnklePoint = floorPlane.ClosestPointOnPlane(leftAnkle);
    //    Vector3 floorRightAnklePoint = floorPlane.ClosestPointOnPlane(rightAnkle);

    //    Gizmos.color = LegOnFloor(null) ? Color.green : Color.red;
    //    Gizmos.DrawSphere(floorLeftAnklePoint, 0.1f);
    //    Gizmos.DrawSphere(floorRightAnklePoint, 0.1f);
    //}
}
