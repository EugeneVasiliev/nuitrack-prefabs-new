using UnityEngine;

using System.Linq;
using System.Collections.Generic;

// This is a test script. In the near future, nuitrack.unitypackage will have a tool 
// for convenient work with skeleton data (positioning the skeleton in local coordinates, 
// aligning the skeleton in RGB, etc.)
public class LocalSkeleton : MonoBehaviour
{
    [Header("Main")]
    [SerializeField, Range(1, 6)] int skeletonID = 1;
    [SerializeField] Transform space;

    [Header("Joints")]
    [SerializeField] List<nuitrack.JointType> targetJoints;
    [SerializeField] GameObject jointPrefab;

    [Header("Options")]
    [SerializeField, Range(0f, 1f)] float jointConfidence = 0.2f;

    Dictionary<nuitrack.JointType, GameObject> gameObjectJoints;

    ulong lastTimeStamp = 0;

    void Awake()
    {
        gameObjectJoints = targetJoints.ToDictionary(k => k, v => Instantiate(jointPrefab, space));
    }

    void Update()
    {
        if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
            return;

        lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;

        nuitrack.Skeleton skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID);

        if (skeleton == null)
            return;

        foreach (KeyValuePair<nuitrack.JointType, GameObject> goJoints in gameObjectJoints)
        {
            nuitrack.Joint joint = skeleton.GetJoint(goJoints.Key);

            if (joint.Confidence > jointConfidence)
            {
                goJoints.Value.SetActive(true);

                Vector3 newPosition = joint.ToVector3() * 0.001f;
                Quaternion newRotation = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.ToQuaternion();

                goJoints.Value.transform.localPosition = newPosition;  // or space.TransformVector(newPosition); if goJoints.Value.parent == null
                goJoints.Value.transform.localRotation = newRotation;
            }
            else
                goJoints.Value.SetActive(false);
        }
    }
}
