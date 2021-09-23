﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JointType = nuitrack.JointType;

namespace NuitrackSDK.Avatar
{
    public class Avatar : BaseAvatar
    {
        [Header ("Options")]
        [Tooltip ("(optional) Specify the transform, which represents the sensor " +
            "coordinate system, to display the Avatar in front of the sensor." +
            "\nCalibration is not supported." +
            "\n\nIf not specified, the object transformation is used.")]
        [SerializeField] Transform sensorSpace;

        [Tooltip("Aligns the size of the model's bones with the size of the bones of the user's skeleton, " +
           "ensuring that the model's size best matches the user's size.")]
        [SerializeField] bool alignmentBoneLength = true;

        [SerializeField] JointType rootJoint = JointType.Waist;

        [Header("VR settings")]
        [SerializeField, NuitrackSDKInspector] bool vrMode = false;
        [SerializeField, NuitrackSDKInspector] GameObject vrHead;
        [SerializeField, NuitrackSDKInspector] Transform headTransform;
        Transform spawnedHead;

        [Header("Calibration")]
        [SerializeField] bool recenterOnSuccess;
        TPoseCalibration tPoseCalibration;
        Vector3 basePivotOffset = Vector3.zero;
        Vector3 startPoint; //Root joint model bone position on start


        [Header("Joints")]
        [SerializeField, NuitrackSDKInspector]
        protected List<ModelJoint> modelJoints;
        /// <summary> Model bones </summary> Dictionary with joints
        Dictionary<JointType, ModelJoint> jointsRigged = new Dictionary<JointType, ModelJoint>();

        void OnEnable()
        {
            tPoseCalibration = FindObjectOfType<TPoseCalibration>();
            tPoseCalibration.onSuccess += OnSuccessCalib;
        }

        bool IsTransformSpace
        {
            get
            {
                return sensorSpace == null || sensorSpace == transform;
            }
        }

        Transform SpaceTransform
        {
            get
            {
                return IsTransformSpace ? transform : sensorSpace;
            }
        }

        public ref List<ModelJoint> ModelJoints
        {
            get
            {
                return ref modelJoints;
            }
        }

        void Start()
        {
            //Adding model bones and JointType keys
            //Adding rotation offsets of model bones and JointType keys

            //Iterate joints from the modelJoints array
            //base rotation of the model bone is recorded 
            //then the model bones and their jointType are added to the jointsRigged dictionary
            foreach (ModelJoint modelJoint in modelJoints)
            {
                if (transform == modelJoint.bone)
                    Debug.LogError("Base transform can't be bone!");

                if (modelJoint.bone)
                {
                    modelJoint.baseRotOffset = Quaternion.Inverse(SpaceTransform.rotation) * modelJoint.bone.rotation;
                    jointsRigged.Add(modelJoint.jointType.TryGetMirrored(), modelJoint);
                }
            }

            foreach (ModelJoint modelJoint in modelJoints)
            {
                //Adding base distances between the child bone and the parent bone 
                if (modelJoint.bone != null && modelJoint.jointType.GetParent() != JointType.None)
                    AddBoneScale(modelJoint.jointType.TryGetMirrored(), modelJoint.jointType.GetParent().TryGetMirrored());
            }

            if (vrMode)
            {
                spawnedHead = Instantiate(vrHead).transform;
                spawnedHead.position = headTransform.position;
            }

            if (jointsRigged.ContainsKey(rootJoint))
            {
                Vector3 rootPosition = jointsRigged[rootJoint].bone.position;
                startPoint = SpaceTransform.InverseTransformPoint(rootPosition);
            }
        }

        /// <summary>
        /// Adding distance between the target and parent model bones
        /// </summary>
        void AddBoneScale(JointType targetJoint, JointType parentJoint)
        {
            //take the position of the model bone
            Vector3 targetBonePos = jointsRigged[targetJoint].bone.position;
            //take the position of the model parent bone  
            Vector3 parentBonePos = jointsRigged[parentJoint].bone.position;
            jointsRigged[targetJoint].baseDistanceToParent = Vector3.Distance(parentBonePos, targetBonePos);
            //record the Transform of the model parent bone
            jointsRigged[targetJoint].parentBone = jointsRigged[parentJoint].bone;
        }

        protected override void Update()
        {
            base.Update();

            if (vrMode)
                spawnedHead.position = headTransform.position;
        }

        /// <summary>
        /// Getting skeleton data from thr sensor and updating transforms of the model bones
        /// </summary>
        protected override void ProcessSkeleton(nuitrack.Skeleton skeleton)
        {
            if (!alignmentBoneLength)
            {
                Vector3 rootPosition = SpaceTransform.rotation * GetJointTransform(rootJoint).Position + basePivotOffset;
                jointsRigged[rootJoint].bone.position = SpaceTransform.TransformPoint(rootPosition);
            }

            foreach (var riggedJoint in jointsRigged)
            {
                //Get joint from the Nuitrack
                //nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);
                JointTransform jointTransform = GetJointTransform(riggedJoint.Key);
                if (jointTransform.IsActive)
                {
                    //Get modelJoint
                    ModelJoint modelJoint = riggedJoint.Value;

                    //Bone rotation
                    Quaternion jointRotation = IsTransformSpace ? jointTransform.RotationMirrored : jointTransform.Rotation;

                    modelJoint.bone.rotation = SpaceTransform.rotation * (jointRotation * modelJoint.baseRotOffset);

                    if (alignmentBoneLength)
                    {
                        Vector3 jointPos = jointTransform.Position + basePivotOffset;
                        Vector3 localPos = IsTransformSpace ? Quaternion.Euler(0, 180, 0) * jointPos : jointPos;

                        Vector3 newPos = SpaceTransform.TransformPoint(localPos);

                        modelJoint.bone.position = newPos;

                        //Bone scale
                        if (modelJoint.parentBone != null && modelJoint.jointType.GetParent() != rootJoint)
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

        private void OnSuccessCalib(Quaternion rotation)
        {
            StartCoroutine(CalculateOffset());
        }

        public IEnumerator CalculateOffset()
        {
            if (!recenterOnSuccess || !IsTransformSpace) 
                yield break;
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (jointsRigged.ContainsKey(rootJoint))
            {
                Vector3 rootPosition = jointsRigged[rootJoint].bone.position;

                Vector3 rootSpacePosition = SpaceTransform.InverseTransformPoint(rootPosition);
                Vector3 newPivotOffset = startPoint - rootSpacePosition + basePivotOffset;
                newPivotOffset.x = 0;

                basePivotOffset = newPivotOffset;
            }
        }

        void OnDisable()
        {
            tPoseCalibration.onSuccess -= OnSuccessCalib;
        }
    }
}