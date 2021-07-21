#if FINAL_IK

using UnityEngine;

using RootMotion.Demos;

public class SpiderNuitrackController : MechSpiderController
{
    [Header("Body control")]
    [Range(0, 2)]
	[SerializeField] float sensitiveJoystic = 4f;

	[Range(0, 180)]
	[SerializeField] float deltaRotationDegrees = 10f;

    Quaternion defaultRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);

    void CameraControl()
    {
        cameraTransform.position = transform.position;
        cameraTransform.rotation = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);
    }

    Vector3 JointPoisition(nuitrack.Skeleton skeleton, nuitrack.JointType jointType)
    {
        return skeleton.GetJoint(jointType).ToVector3() * 0.001f;
    }

    void Update()
	{
		CameraControl();

		nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

        if (skeleton == null)
            return;

        Vector3 upBodyDirection = (JointPoisition(skeleton, nuitrack.JointType.Head) - JointPoisition(skeleton, nuitrack.JointType.Waist)).normalized;

        float verticalAxis = Mathf.Clamp(Vector3.Dot(upBodyDirection, Vector3.back) * sensitiveJoystic, -1, 1);
        float horizontalAxis = Mathf.Clamp(Vector3.Dot(upBodyDirection, Vector3.left) * sensitiveJoystic, -1, 1);

        Vector3 inputVector = new Vector3(horizontalAxis, 0, verticalAxis);
        Vector3 moveDirection = transform.TransformVector(inputVector).normalized * inputVector.magnitude;
        transform.Translate(moveDirection * speed * mechSpider.scale * Time.deltaTime, Space.World);


        Vector3 shoulderDirection = JointPoisition(skeleton, nuitrack.JointType.RightShoulder) - JointPoisition(skeleton, nuitrack.JointType.LeftShoulder);
        shoulderDirection.y = 0;

        Quaternion shoulderRotation = Quaternion.LookRotation(shoulderDirection.normalized, Vector3.up);

        if (Quaternion.Angle(shoulderRotation, defaultRotation) > deltaRotationDegrees)
        {
            Quaternion deltaShoulderRotation = Quaternion.Inverse(defaultRotation) * shoulderRotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, transform.rotation * deltaShoulderRotation, Time.deltaTime * turnSpeed);
        }
    }
}
#endif