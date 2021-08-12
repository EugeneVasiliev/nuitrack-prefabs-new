using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Avatar
{
    public abstract class Avatar : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        protected List<nuitrack.JointType> jointTypes;

        [Header("Options")]
        [SerializeField, Range(1, 7)]
        protected int skeletonID = 1;

        [Tooltip("Aligns the size of the model's bones with the size of the bones of the user's skeleton, " +
            "ensuring that the model's size best matches the user's size.")]
        [SerializeField]
        protected bool alignmentBoneLength;

        [Tooltip("Aligns the positions of the model's joints with the joints of the user's skeleton.\n" +
           "It can cause model distortions and artifacts.")]
        [SerializeField] bool alignJointPosition = false;

        [SerializeField, Range(0, 1)] float jointConfidence = 0.1f;

        [Header("Space")]
        [Tooltip("(optional) Transform corresponding to the sensor coordinate system. If not specified, " +
           "the transformation is performed in the world coordinate system, where the sensor position = (0, 0, 0)")]
        [SerializeField] protected Transform space;

        protected ulong lastTimeStamp = 0;

        public List<nuitrack.JointType> JointTypes
        {
            get
            {
                return jointTypes;
            }
        }

        void Update()
        {
            if (NuitrackManager.SkeletonData == null || NuitrackManager.SkeletonData.Timestamp == lastTimeStamp)
                return;

            lastTimeStamp = NuitrackManager.SkeletonData.Timestamp;
            nuitrack.Skeleton skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(skeletonID);

            ProcessSkeleton(skeleton);
        }

        protected abstract void ProcessSkeleton(nuitrack.Skeleton skeleton);
    }
}