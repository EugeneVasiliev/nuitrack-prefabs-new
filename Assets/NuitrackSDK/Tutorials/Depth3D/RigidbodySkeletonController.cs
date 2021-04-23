using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RigidbodySkeletonController : MonoBehaviour
{
    [System.Serializable]
    public class RigidbodyJoint
    {
        public Rigidbody rigidbody;
        public nuitrack.JointType jointType;
    }

    [SerializeField] Transform space;
    [SerializeField] List<RigidbodyJoint> rigidbodyJoints;

    [Header ("Test options")]
    [SerializeField] DataProvider dataProvider;

    void FixedUpdate()
    {
        //nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
        DataProvider.DPSkeleton skeleton = dataProvider.CurrentSkeleton;

        if (skeleton == null)
            return;

        foreach(RigidbodyJoint rigidbodyJoint in rigidbodyJoints)
        {
            Vector3 newPosition = skeleton.GetJoint(rigidbodyJoint.jointType).Real.ToVector3() * 0.001f;
            Vector3 spacePostion = space.TransformPoint(newPosition);

            rigidbodyJoint.rigidbody.MovePosition(spacePostion);
        }
    }
}
