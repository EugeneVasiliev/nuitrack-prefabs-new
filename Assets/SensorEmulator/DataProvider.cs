using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;


public class DataProvider : MonoBehaviour
{
    [System.Serializable]
    public class DPFrame
    {
        public int Cols;
        public int Rows;

        [HideInInspector]
        public byte[] data; 

        public int DataSize
        {
            get
            {
                return data.Length;
            }
        }

        public IntPtr Data
        {
            get
            {
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, unmanagedPointer, data.Length);
                // Call unmanaged code
                Marshal.FreeHGlobal(unmanagedPointer);
                return unmanagedPointer;
            }
        }
    }

    [System.Serializable]
    public class DPSkeleton
    {
        [System.Serializable]
        public class Joint
        {
            public nuitrack.JointType Type;
            public Vector3 realCoord;
            public Vector3 projCoord;

            public nuitrack.Vector3 Real
            {
                get
                {
                    return new nuitrack.Vector3(realCoord.x, realCoord.y, realCoord.z);
                }
            }

            public nuitrack.Vector3 Proj
            {
                get
                {
                    return new nuitrack.Vector3(projCoord.x, projCoord.y, projCoord.z);
                }
            }
        }

        [SerializeField] List<Joint> joints;

        public DPSkeleton(nuitrack.Skeleton skeleton)
        {
            joints = new List<Joint>();

            foreach (nuitrack.JointType jointType in Enum.GetValues(typeof(nuitrack.JointType)))
            {
                Joint ji = new Joint()
                {
                    Type = jointType,
                    realCoord = skeleton.GetJoint(jointType).Real.ToVector3(),
                    projCoord = skeleton.GetJoint(jointType).Proj.ToVector3()
                };

                joints.Add(ji);
            }
        }

        public Joint GetJoint(nuitrack.JointType jointType)
        {
            foreach (Joint jt in joints)
                if (jt.Type == jointType)
                    return jt;

            return null;
        }

    }

    [System.Serializable]
    public class DPUserFrame
    {
        public Vector3 floorPoint;
        public Vector3 floorNormal;

        public DPUserFrame(nuitrack.UserFrame userFrame)
        {
            floorPoint = userFrame.Floor.ToVector3();
            floorNormal = userFrame.FloorNormal.ToVector3();
        }

        public nuitrack.Vector3 Floor
        {
            get
            {
                return new nuitrack.Vector3(floorPoint.x, floorPoint.y, floorPoint.z);
            }
        }

        public nuitrack.Vector3 FloorNormal
        {
            get
            {
                return new nuitrack.Vector3(floorNormal.x, floorNormal.y, floorNormal.z);
            }
        }
    }

    [SerializeField] bool isActive = false;

    [SerializeField] DPFrame rgbFrame;
    [SerializeField] DPFrame depthFrame;
    [SerializeField] DPFrame segmentFrame;

    [SerializeField] DPSkeleton skeleton;

    [SerializeField] DPUserFrame userFrame;

    void Start()
    {
        if (isActive)
        {
            TPoseCalibration.Instance.onSuccess += Instance_onSuccess;

            //NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
            //NuitrackManager.onDepthUpdate += NuitrackManager_onDepthUpdate;
            //NuitrackManager.onUserTrackerUpdate += NuitrackManager_onUserTrackerUpdate;
            //NuitrackManager.onSkeletonTrackerUpdate += NuitrackManager_onSkeletonTrackerUpdate;
            //NuitrackManager.onUserTrackerUpdate += NuitrackManager_onUserTrackerUpdate1;
        }
    }

    private void NuitrackManager_onUserTrackerUpdate1(nuitrack.UserFrame frame)
    {
        userFrame = new DPUserFrame(frame);
    }

    private void NuitrackManager_onSkeletonTrackerUpdate(nuitrack.SkeletonData skeletonData)
    {
        skeleton = new DPSkeleton(skeletonData.Skeletons[0]);
    }

    private void Instance_onSuccess(Quaternion rotation)
    {
        NuitrackManager_onColorUpdate(NuitrackManager.ColorFrame);
        NuitrackManager_onDepthUpdate(NuitrackManager.DepthFrame);
        NuitrackManager_onUserTrackerUpdate(NuitrackManager.UserFrame);
        NuitrackManager_onSkeletonTrackerUpdate(NuitrackManager.SkeletonData);
        NuitrackManager_onUserTrackerUpdate1(NuitrackManager.UserFrame);

        Debug.Log("Shot");
    }

    private void NuitrackManager_onUserTrackerUpdate(nuitrack.UserFrame frame)
    {
        segmentFrame = new DPFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, segmentFrame.data, 0, frame.DataSize);
    }


    private void NuitrackManager_onDepthUpdate(nuitrack.DepthFrame frame)
    {
        depthFrame = new DPFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, depthFrame.data, 0, frame.DataSize);
    }

    void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        rgbFrame = new DPFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, rgbFrame.data, 0, frame.DataSize);
    }

    public DPFrame RGBFrame
    {
        get
        {
            return rgbFrame;
        }
    }

    public DPFrame DepthFrame
    {
        get
        {
            return depthFrame;
        }
    }


    public DPFrame SegmentFrame
    {
        get
        {
            return segmentFrame;
        }
    }

    public DPSkeleton CurrentSkeleton
    {
        get
        {
            return skeleton;
        }
    }

    public DPUserFrame UserFrame
    {
        get
        {
            return userFrame;
        }
    }

}
