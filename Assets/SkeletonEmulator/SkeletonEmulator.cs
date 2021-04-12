using UnityEngine;

using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class JointSimulation
{
	public Transform targetTransform;
	public nuitrack.JointType jointType;
}

public class SkeletonEmulator : MonoBehaviour
{
	[SerializeField] Transform spaceAxis;
	[SerializeField] List<JointSimulation> jointSimulations;

	Dictionary<nuitrack.JointType, Transform> jointsDictonary;

    void Awake()
    {
		jointsDictonary = jointSimulations.ToDictionary(p => p.jointType, v => v.targetTransform);
	}

    public Vector3 JointPoisition(nuitrack.JointType jointType)
	{
		if (jointsDictonary != null)
		{
			if (jointsDictonary.ContainsKey(jointType))
				return spaceAxis.InverseTransformPoint(jointsDictonary[jointType].position);
			else
			{
				Debug.LogError(string.Format("Unknown joint: {0}", jointType.ToString()));
				return Vector3.zero;
			}
		}
		else
        {
			foreach (JointSimulation js in jointSimulations)
				if (js.jointType == jointType)
					return spaceAxis.InverseTransformPoint(js.targetTransform.position);

			Debug.LogError(string.Format("Unknown joint: {0}", jointType.ToString()));
			return Vector3.zero;
		}
	}

    private void OnDrawGizmos()
    {
		Dictionary<nuitrack.JointType, Transform> skeleton = jointSimulations.ToDictionary(p => p.jointType, v => v.targetTransform);

		UnityEditor.Handles.color = new Color(0.5f, 0.5f, 1, 0.3f);
		foreach (Transform t in skeleton.Values)
			UnityEditor.Handles.DrawSphere(0, t.position, Quaternion.identity, 0.05f);

		UnityEditor.Handles.color = Color.white;
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.Waist].position, skeleton[nuitrack.JointType.Torso].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.Torso].position, skeleton[nuitrack.JointType.LeftCollar].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.Torso].position, skeleton[nuitrack.JointType.RightCollar].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftCollar].position, skeleton[nuitrack.JointType.LeftShoulder].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightCollar].position, skeleton[nuitrack.JointType.RightShoulder].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftCollar].position, skeleton[nuitrack.JointType.Head].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightCollar].position, skeleton[nuitrack.JointType.Head].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.Waist].position, skeleton[nuitrack.JointType.LeftHip].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.Waist].position, skeleton[nuitrack.JointType.RightHip].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftHip].position, skeleton[nuitrack.JointType.LeftKnee].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightHip].position, skeleton[nuitrack.JointType.RightKnee].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftKnee].position, skeleton[nuitrack.JointType.LeftAnkle].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightKnee].position, skeleton[nuitrack.JointType.RightAnkle].position);


		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftShoulder].position, skeleton[nuitrack.JointType.LeftElbow].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightShoulder].position, skeleton[nuitrack.JointType.RightElbow].position);

		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.LeftElbow].position, skeleton[nuitrack.JointType.LeftWrist].position);
		UnityEditor.Handles.DrawLine(skeleton[nuitrack.JointType.RightElbow].position, skeleton[nuitrack.JointType.RightWrist].position);
	}
}
