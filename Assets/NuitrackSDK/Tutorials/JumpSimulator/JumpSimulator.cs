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

    bool lefOnFloorLastFrame = true;

    public float BestJumpHeight
    {
        get;
        private set;
    }

    public float CurrentJumpHeight
    {
        get;
        private set;
    }

    bool LegOnFloor(nuitrack.Skeleton skeleton)
    {
        Vector3 leftAnkle = skeleton.GetJoint(nuitrack.JointType.LeftAnkle).ToVector3();
        Vector3 rightAnkle = skeleton.GetJoint(nuitrack.JointType.RightAnkle).ToVector3();

        Vector3 floorLeftAnklePoint = floorPlane.ClosestPointOnPlane(leftAnkle);
        Vector3 floorRightAnklePoint = floorPlane.ClosestPointOnPlane(rightAnkle);

        return Vector3.Distance(leftAnkle, floorLeftAnklePoint) <= deltaAnkleFloor * 1000 || 
            Vector3.Distance(rightAnkle, floorRightAnklePoint) <= deltaAnkleFloor * 1000;
    }

    float GetLowerJoint(nuitrack.Skeleton skeleton, out nuitrack.JointType lowerJointType)
    {
        lowerJointType = nuitrack.JointType.Head;
        float jumpHeight = float.MaxValue;

        foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
        {
            Vector3 jointPoint = skeleton.GetJoint(jointType).ToVector3();
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

    Vector3 ScreenPoint(nuitrack.Vector3 realPoint, nuitrack.DepthFrame frame)
    {
        nuitrack.Vector3 point = NuitrackManager.DepthSensor.ConvertRealToProjCoords(realPoint);

        point.X /= frame.Cols;
        point.Y /= frame.Rows;

        return point.ToVector3();
    }

    nuitrack.Vector3 NuitrackVector(Vector3 vector)
    {
        return new nuitrack.Vector3(vector.x, vector.y, vector.z);
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

        Vector3 floorBasePoint = NuitrackManager.UserFrame.Floor.ToVector3();
        Vector3 floorNormalVector = NuitrackManager.UserFrame.FloorNormal.ToVector3().normalized;

        //Vector3 floorBasePoint = floorBasePointTransform.position;
        //Vector3 floorNormalVector = (floorBasePointTransform.position - floorNormalVectorTransform.position).normalized;

        floorPlane = new Plane(floorNormalVector, floorBasePoint);

        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        nuitrack.JointType lowerJoint = nuitrack.JointType.None;
        float jumpHeight = GetLowerJoint(skeleton, out lowerJoint);

        if (!LegOnFloor(skeleton))
        {
            if (lefOnFloorLastFrame)
            {
                CurrentJumpHeight = 0;
                lefOnFloorLastFrame = false;
            }

            if (jumpHeight > CurrentJumpHeight)
                CurrentJumpHeight = jumpHeight;

            if (CurrentJumpHeight > BestJumpHeight)
            {
                BestJumpHeight = CurrentJumpHeight;
                bestJumpLable.text = string.Format("Best: {0:F2}", BestJumpHeight * 0.001f);
            }
        }
        else
            lefOnFloorLastFrame = true;

        nuitrack.Vector3 jointPosition = skeleton.GetJoint(lowerJoint).Real;
        Vector3 currentJumpScreenPoint = ScreenPoint(jointPosition, NuitrackManager.DepthFrame);

        currentJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height * currentJumpScreenPoint.y);
        currentJumpLable.text = string.Format("Current: {0:F2}", CurrentJumpHeight * 0.001f);

        Vector3 userWaistPoint = skeleton.GetJoint(nuitrack.JointType.Waist).ToVector3();
        Vector3 userFloorPoint = floorPlane.ClosestPointOnPlane(userWaistPoint);
        Vector3 screenFloorPoint = ScreenPoint(NuitrackVector(userFloorPoint), NuitrackManager.DepthFrame);

        floorLine.anchoredPosition = new Vector2(0, baseRect.rect.height * screenFloorPoint.y);

        Vector3 bestJumpPoint = userFloorPoint + floorNormalVector * BestJumpHeight;
        Vector3 screenBestJumpPoint = ScreenPoint(NuitrackVector(bestJumpPoint), NuitrackManager.DepthFrame);

        bestJumpLine.anchoredPosition = new Vector2(0, baseRect.rect.height * screenBestJumpPoint.y);
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
