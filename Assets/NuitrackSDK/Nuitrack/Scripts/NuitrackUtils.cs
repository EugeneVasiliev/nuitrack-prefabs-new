using UnityEngine;

using System.Collections.Generic;

using JointType = nuitrack.JointType;

public static class NuitrackUtils
{
    #region Transform
    public static Vector3 ToVector3(this nuitrack.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector3 ToVector3(this nuitrack.Joint joint)
    {
        return new Vector3(joint.Real.X, joint.Real.Y, joint.Real.Z);
    }

    public static Quaternion ToQuaternion(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    public static Quaternion ToQuaternionMirrored(this nuitrack.Joint joint)
    {
        Vector3 jointUp = new Vector3(-joint.Orient.Matrix[1], joint.Orient.Matrix[4], -joint.Orient.Matrix[7]);   //Y(Up)
        Vector3 jointForward = new Vector3(joint.Orient.Matrix[2], -joint.Orient.Matrix[5], joint.Orient.Matrix[8]);   //Z(Forward)

        if (jointForward.magnitude < 0.01f)
            return Quaternion.identity; //should not happen

        return Quaternion.LookRotation(jointForward, jointUp);
    }

    #endregion

    #region SkeletonUltils

    static readonly Dictionary<JointType, HumanBodyBones> nuitrackToUnity = new Dictionary<JointType, HumanBodyBones>()
    {
        {JointType.Head,                HumanBodyBones.Head},
        {JointType.Neck,                HumanBodyBones.Neck},
        {JointType.LeftCollar,          HumanBodyBones.LeftShoulder},
        {JointType.RightCollar,         HumanBodyBones.RightShoulder},
        {JointType.Torso,               HumanBodyBones.Hips},
        {JointType.Waist,               HumanBodyBones.Hips},   // temporarily


        {JointType.LeftFingertip,       HumanBodyBones.LeftMiddleDistal},
        {JointType.LeftHand,            HumanBodyBones.LeftMiddleProximal},
        {JointType.LeftWrist,           HumanBodyBones.LeftHand},
        {JointType.LeftElbow,           HumanBodyBones.LeftLowerArm},
        {JointType.LeftShoulder,        HumanBodyBones.LeftUpperArm},

        {JointType.RightFingertip,      HumanBodyBones.RightMiddleDistal},
        {JointType.RightHand,           HumanBodyBones.RightMiddleProximal},
        {JointType.RightWrist,          HumanBodyBones.RightHand},
        {JointType.RightElbow,          HumanBodyBones.RightLowerArm},
        {JointType.RightShoulder,       HumanBodyBones.RightUpperArm},


        {JointType.LeftFoot,            HumanBodyBones.LeftToes},
        {JointType.LeftAnkle,           HumanBodyBones.LeftFoot},
        {JointType.LeftKnee,            HumanBodyBones.LeftLowerLeg},
        {JointType.LeftHip,             HumanBodyBones.LeftUpperLeg},

        {JointType.RightFoot,           HumanBodyBones.RightToes},
        {JointType.RightAnkle,          HumanBodyBones.RightFoot},
        {JointType.RightKnee,           HumanBodyBones.RightLowerLeg},
        {JointType.RightHip,            HumanBodyBones.RightUpperLeg},
    };

    /// <summary>
    /// Returns the appropriate HumanBodyBones  for nuitrack.JointType
    /// </summary>
    /// <param name="nuitrackJoint">nuitrack.JointType</param>
    /// <returns>HumanBodyBones</returns>
    public static HumanBodyBones ToUnityBones(this JointType nuitrackJoint)
    {
        return nuitrackToUnity[nuitrackJoint];
    }

    static readonly Dictionary<JointType, JointType> mirroredJoints = new Dictionary<JointType, JointType>() {
        {JointType.LeftShoulder, JointType.RightShoulder},
        {JointType.RightShoulder, JointType.LeftShoulder},
        {JointType.LeftElbow, JointType.RightElbow},
        {JointType.RightElbow, JointType.LeftElbow},
        {JointType.LeftWrist, JointType.RightWrist},
        {JointType.RightWrist, JointType.LeftWrist},
        {JointType.LeftFingertip, JointType.RightFingertip},
        {JointType.RightFingertip, JointType.LeftFingertip},

        {JointType.LeftHip, JointType.RightHip},
        {JointType.RightHip, JointType.LeftHip},
        {JointType.LeftKnee, JointType.RightKnee},
        {JointType.RightKnee, JointType.LeftKnee},
        {JointType.LeftAnkle, JointType.RightAnkle},
        {JointType.RightAnkle, JointType.LeftAnkle},
    };

    public static JointType TryGetMirrored(this JointType joint)
    {
        JointType mirroredJoint = joint;
        if (NuitrackManager.DepthSensor.IsMirror() && mirroredJoints.ContainsKey(joint))
        {
            mirroredJoints.TryGetValue(joint, out mirroredJoint);
        }

        return mirroredJoint;
    }

    #endregion
}