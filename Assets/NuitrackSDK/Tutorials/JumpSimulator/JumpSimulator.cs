using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class JumpSimulator : MonoBehaviour
{
    [SerializeField] float deltaAnkleFloor = 0.1f;
    Plane floorPlane;

    [SerializeField] RectTransform baseRect;

    [Header ("Best jump UI")] 
    [SerializeField] RectTransform bestJumpLine;
    [SerializeField] Text bestJumpLable;

    [Header("Current jump UI")]
    [SerializeField] RectTransform currentJumpLine;
    [SerializeField] Text currentJumpLable;

    [SerializeField] SkeletonEmulator skeletonEmulator;

    public Transform floorBasePointTransform;
    public Transform floorNormalVectorTransform;

    public float TotalMaxJumpHeigth
    {
        get;
        private set;
    }

    public float CurrentMaxJumpHeigth
    {
        get;
        private set;
    }


    List<nuitrack.JointType> leftLegJoints = new List<nuitrack.JointType>
    {
        nuitrack.JointType.LeftHip,
        nuitrack.JointType.LeftKnee,
        nuitrack.JointType.LeftAnkle
    };

    List<nuitrack.JointType> rightLegJoints = new List<nuitrack.JointType>
    {
        nuitrack.JointType.RightHip,
        nuitrack.JointType.RightKnee,
        nuitrack.JointType.RightAnkle
    };

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

    float LegLength(nuitrack.Skeleton skeleton, List<nuitrack.JointType> jointTypes)
    {
        float length = 0;

        for (int i = 0; i < jointTypes.Count - 1; i++)
            length += Vector3.Distance(JointPoisition(skeleton, jointTypes[i]), JointPoisition(skeleton, jointTypes[i + 1]));

        return length;
    }

    float ProjectHipY(nuitrack.Skeleton skeleton, float delta=0)
    {
        //return (skeleton.GetJoint(nuitrack.JointType.LeftHip).Proj.Y + skeleton.GetJoint(nuitrack.JointType.RightHip).Proj.Y) / 2;

        Vector3 hipPosition = JointPoisition(skeleton, nuitrack.JointType.LeftHip);
        hipPosition.y += delta;

        Vector3 hip = Camera.main.WorldToScreenPoint(hipPosition);
        return hip.y / Screen.height;
    }

    void Start()
    {
        bestJumpLine.gameObject.SetActive(false);
    }

    public Vector3 floorBasePoint;
    public Vector3 floorNormalVector;

    // Update is called once per frame
    void Update()
    {       
        if (NuitrackManager.UserFrame == null || CurrentUserTracker.CurrentSkeleton == null)
        {
            CurrentMaxJumpHeigth = 0;
            return;
        }

        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        floorBasePoint = NuitrackManager.UserFrame.Floor.ToVector3() * 0.001f;
        floorNormalVector = NuitrackManager.UserFrame.FloorNormal.ToVector3();

        //Vector3 floorBasePoint = floorBasePointTransform.position;
        //Vector3 floorNormalVector = (floorBasePointTransform.position - floorNormalVectorTransform.position).normalized;

        floorPlane = new Plane(floorNormalVector.normalized, floorBasePoint);

        //float bestJumpHipsY = ProjectHipY(skeleton, TotalMaxJumpHeigth);

        //bestJumpLine.anchoredPosition = new Vector2(0, bestJumpHipsY * baseRect.rect.height);
        //bestJumpLable.text = string.Format("Best: {0:F2}", TotalMaxJumpHeigth);

        float currentJumpHipsY = ProjectHipY(skeleton);

        currentJumpLine.anchoredPosition = new Vector2(0, currentJumpHipsY * baseRect.rect.height);
        currentJumpLable.text = string.Format("Current: {0:F2}", CurrentMaxJumpHeigth);


        float legLength = (LegLength(skeleton, leftLegJoints) + LegLength(skeleton, rightLegJoints)) / 2;

        if (!LegOnFloor(skeleton))
        {
            Vector3 leftHipPosition = JointPoisition(skeleton, nuitrack.JointType.LeftHip);
            Vector3 rightHipPosition = JointPoisition(skeleton, nuitrack.JointType.RightHip);

            Vector3 floorLeftHipPoint = floorPlane.ClosestPointOnPlane(leftHipPosition);
            Vector3 floorRightHipPoint = floorPlane.ClosestPointOnPlane(rightHipPosition);

            float hipFloorDistance = (Vector3.Distance(leftHipPosition, floorLeftHipPoint) + Vector3.Distance(rightHipPosition, floorRightHipPoint)) / 2;
            float deltaJump = hipFloorDistance - legLength;

            //Debug.Log(deltaJump);

            if (deltaJump > CurrentMaxJumpHeigth)
                CurrentMaxJumpHeigth = deltaJump;

            if (CurrentMaxJumpHeigth > TotalMaxJumpHeigth)
            {
                TotalMaxJumpHeigth = CurrentMaxJumpHeigth;
                //bestJumpLine.gameObject.SetActive(true);
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
