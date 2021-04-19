using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;


[System.Serializable]
public class DataProviderFrame
{
    public byte[] data;
    public int Cols;
    public int Rows;

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

public class DataProvider : MonoBehaviour
{
    [SerializeField] bool isActive = false;

    [Header ("Do not open \"data\" array, please")] 
    [SerializeField] DataProviderFrame rgbFrame;
    [SerializeField] DataProviderFrame depthFrame;
    [SerializeField] DataProviderFrame segmentFrame;

    void Start()
    {
        if (isActive)
        {
            TPoseCalibration.Instance.onSuccess += Instance_onSuccess;

            //NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
            //NuitrackManager.onDepthUpdate += NuitrackManager_onDepthUpdate;
            //NuitrackManager.onUserTrackerUpdate += NuitrackManager_onUserTrackerUpdate;
        }
    }

    private void Instance_onSuccess(Quaternion rotation)
    {
        NuitrackManager_onColorUpdate(NuitrackManager.ColorFrame);
        NuitrackManager_onDepthUpdate(NuitrackManager.DepthFrame);
        NuitrackManager_onUserTrackerUpdate(NuitrackManager.UserFrame);

        Debug.Log("Shot");
    }

    private void NuitrackManager_onUserTrackerUpdate(nuitrack.UserFrame frame)
    {
        segmentFrame = new DataProviderFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, segmentFrame.data, 0, frame.DataSize);
    }


    private void NuitrackManager_onDepthUpdate(nuitrack.DepthFrame frame)
    {
        depthFrame = new DataProviderFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, depthFrame.data, 0, frame.DataSize);
    }

    void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        rgbFrame = new DataProviderFrame()
        {
            data = new byte[frame.DataSize],
            Cols = frame.Cols,
            Rows = frame.Rows
        };

        Marshal.Copy(frame.Data, rgbFrame.data, 0, frame.DataSize);
    }

    public DataProviderFrame RGBFrame
    {
        get
        {
            return rgbFrame;
        }
    }

    public DataProviderFrame DepthFrame
    {
        get
        {
            return depthFrame;
        }
    }


    public DataProviderFrame SegmentFrame
    {
        get
        {
            return segmentFrame;
        }
    }

}
