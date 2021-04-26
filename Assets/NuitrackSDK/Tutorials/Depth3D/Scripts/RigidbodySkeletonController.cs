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

    [SerializeField] Transform sensorCenter;
    [SerializeField] GameObject jointObj;

    [SerializeField] float smoothVal = 4f;

    Dictionary<nuitrack.JointType, Transform> jointsObj;

    void Start()
    {
        jointsObj = new Dictionary<nuitrack.JointType, Transform>();

        foreach (nuitrack.JointType jointType in System.Enum.GetValues(typeof(nuitrack.JointType)))
        {
            GameObject joint = Instantiate(jointObj, sensorCenter);
            joint.name = jointType.ToString();
            jointsObj.Add(jointType, joint.transform);
        }
    }

    void Update()
    {
        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        if (skeleton == null)
            return;

        foreach (KeyValuePair<nuitrack.JointType, Transform> joints in jointsObj)
        {
            Vector3 position = skeleton.GetJoint(joints.Key).Real.ToVector3() * 0.001f;
            joints.Value.localPosition = position;
        }
    }

    void FixedUpdate()
    {
        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        if (skeleton == null)
            return;

        foreach(RigidbodyJoint rigidbodyJoint in rigidbodyJoints)
        {
            Vector3 newPosition = skeleton.GetJoint(rigidbodyJoint.jointType).Real.ToVector3() * 0.001f;
            Vector3 spacePostion = space.TransformPoint(newPosition);

            Vector3 lerpPosition = Vector3.Lerp(rigidbodyJoint.rigidbody.position, spacePostion, Time.deltaTime * smoothVal);

            rigidbodyJoint.rigidbody.MovePosition(lerpPosition);
        }
    }
}
