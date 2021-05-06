using UnityEngine;
using System.Collections.Generic;

enum MappingMode
{
    Indirect,
    Direct,
}

public class Avatar : MonoBehaviour
{
    [Header("Rigged model")]
    [SerializeField] ModelJoint[] modelJoints;
    [SerializeField] MappingMode mappingMode;
    [SerializeField] nuitrack.JointType rootJoint = nuitrack.JointType.Waist;
    [SerializeField] Transform rootModel;

    /// <summary> Model bones </summary> Dictionary with joints
    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();
    Dictionary<nuitrack.JointType, Transform> defaultParents = new Dictionary<nuitrack.JointType, Transform>();
    Dictionary<nuitrack.JointType, Vector3> defaultTransform = new Dictionary<nuitrack.JointType, Vector3>();

    MappingMode prevMappingMode;

    void Start()
    {
        prevMappingMode = mappingMode;
        //Adding model bones and JointType keys
        //Adding rotation offsets of model bones and JointType keys

        //Iterate joints from the modelJoints array
        //base rotation of the model bone is recorded 
        //then the model bones and their jointType are added to the jointsRigged dictionary
        for (int i = 0; i < modelJoints.Length; i++)
        {
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            jointsRigged.Add(modelJoints[i].jointType.TryGetMirrored(), modelJoints[i]);

            //Adding base distances between the child bone and the parent bone 
            if (modelJoints[i].parentJointType != nuitrack.JointType.None)
                AddBoneScale(modelJoints[i].jointType.TryGetMirrored(), modelJoints[i].parentJointType.TryGetMirrored());
        }
    }

    /// <summary>
    /// Adding distance between the target and parent model bones
    /// </summary>
    void AddBoneScale(nuitrack.JointType targetJoint, nuitrack.JointType parentJoint)
    {
        //take the position of the model bone
        Vector3 targetBonePos = jointsRigged[targetJoint].bone.position;
        //take the position of the model parent bone  
        Vector3 parentBonePos = jointsRigged[parentJoint].bone.position;
        jointsRigged[targetJoint].baseDistanceToParent = Vector3.Distance(parentBonePos, targetBonePos);
        //record the Transform of the model parent bone
        jointsRigged[targetJoint].parentBone = jointsRigged[parentJoint].bone;
    }

    void Update()
    {
        //If a skeleton is detected, process the model
        if (CurrentUserTracker.CurrentSkeleton != null) ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
    }

    /// <summary>
    /// Getting skeleton data from thr sensor and updating transforms of the model bones
    /// </summary>
    void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        if (mappingMode == MappingMode.Indirect)
            rootModel.position = 0.001f * skeleton.GetJoint(rootJoint).ToVector3();

        foreach (var riggedJoint in jointsRigged)
        {
            if(mappingMode == MappingMode.Indirect)
            {
                //Get joint from the Nuitrack
                nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);

                ModelJoint modelJoint = riggedJoint.Value;

                //Calculate the model bone rotation: take the mirrored joint orientation, add a basic rotation of the model bone, invert movement along the Z axis
                Quaternion jointOrient = joint.ToQuaternion() * modelJoint.baseRotOffset;

                modelJoint.bone.rotation = jointOrient;
            }
            else
            {
                //Get joint from the Nuitrack
                nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);

                //Get modelJoint
                ModelJoint modelJoint = riggedJoint.Value;

                if (joint.Confidence > 0.1f)
                {
                    //Bone position
                    Vector3 newPos = 0.001f * joint.ToVector3();
                    modelJoint.bone.position = newPos;

                    //Bone rotation
                    Quaternion jointOrient = joint.ToQuaternion() * modelJoint.baseRotOffset;
                    modelJoint.bone.rotation = jointOrient;

                    //Bone scale
                    if (modelJoint.parentBone != null)
                    {
                        //Take the Transform of a parent bone
                        Transform parentBone = modelJoint.parentBone;
                        //calculate how many times the distance between the child bone and its parent bone has changed compared to the base distance (which was recorded at the start)
                        float scaleDif = modelJoint.baseDistanceToParent / Vector3.Distance(newPos, parentBone.position);
                        //change the size of the bone to the resulting value (On default bone size (1,1,1))
                        parentBone.localScale = Vector3.one / scaleDif;
                        //compensation for size due to hierarchy
                        parentBone.localScale *= parentBone.localScale.x / parentBone.lossyScale.x;
                    }
                }
            }
        }
    }
}