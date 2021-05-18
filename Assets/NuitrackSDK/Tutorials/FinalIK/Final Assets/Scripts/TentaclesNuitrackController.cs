#if FINAL_IK
using UnityEngine;
using System.Collections.Generic;

using RootMotion.FinalIK;

public class TentaclesNuitrackController : MonoBehaviour
{
    [SerializeField] FABRIK fABRIK;
	[SerializeField] List<nuitrack.JointType> handJoints;
	[SerializeField] float interpolationRatio = 4f;

    Vector3 JointPoisition(nuitrack.Skeleton skeleton, nuitrack.JointType jointType)
    {
        return skeleton.GetJoint(jointType).ToVector3() * 0.001f;
    }

    float HandLength(nuitrack.Skeleton skeleton)
    {
        float handLength = 0;

        for (int i = 0; i < handJoints.Count - 1; i++)
            handLength += (JointPoisition(skeleton, handJoints[i]) - JointPoisition(skeleton, handJoints[i + 1])).magnitude;

        return handLength;
    }

    float TentaclesLength
	{
		get
		{
			IKSolver.Bone[] bones = fABRIK.solver.bones;

			float length = 0;

			for (int i = 0; i < bones.Length - 1; i++)
				length += (bones[i].transform.position - bones[i + 1].transform.position).magnitude;

			return length;
		}
	}

    void Update()
    {
        nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        if (skeleton == null)
            return;

        Vector3 localHandPosition = (JointPoisition(skeleton, handJoints[handJoints.Count - 1]) - JointPoisition(skeleton, handJoints[0])) / HandLength(skeleton);
        fABRIK.solver.target.localPosition = Vector3.Lerp(fABRIK.solver.target.localPosition, localHandPosition * TentaclesLength, Time.deltaTime * interpolationRatio);
    }
}
#endif