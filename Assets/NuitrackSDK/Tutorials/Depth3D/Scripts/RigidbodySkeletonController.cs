using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RigidbodySkeletonController : MonoBehaviour
{
    [Header ("Rigidbody")]
    [SerializeField] List<nuitrack.JointType> targetJoints;
    [SerializeField] GameObject rigidBodyJoint;

    [SerializeField] float smoothVal = 4f;

    [Header ("Space")]
    [SerializeField] Transform space;
    [SerializeField] Transform sensorCenter;

    Dictionary<nuitrack.JointType, Rigidbody> rigidbodyObj;

    void Start()
    {
        rigidbodyObj = new Dictionary<nuitrack.JointType, Rigidbody>();

        foreach (nuitrack.JointType jointType in targetJoints)
        {
            GameObject jointObj = Instantiate(rigidBodyJoint, sensorCenter);
            jointObj.name = string.Format("{0}_rigidbody", jointType.ToString());

            Rigidbody rigidbody = jointObj.GetComponent<Rigidbody>();
            rigidbodyObj.Add(jointType, rigidbody);
        }
    }

    void FixedUpdate()
    {
        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        if (skeleton == null)
            return;

        foreach(KeyValuePair<nuitrack.JointType, Rigidbody> rigidbodyJoint in rigidbodyObj)
        {
            Vector3 newPosition = skeleton.GetJoint(rigidbodyJoint.Key).Real.ToVector3() * 0.001f;

            Vector3 spacePostion = space == null ? newPosition : space.TransformPoint(newPosition);
            Vector3 lerpPosition = Vector3.Lerp(rigidbodyJoint.Value.position, spacePostion, Time.deltaTime * smoothVal);

            rigidbodyJoint.Value.MovePosition(lerpPosition);
        }
    }

    void UpdateSkeletons()
    {

    }
}
