using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

public class DataProvider : MonoBehaviour
{
    [SerializeField] bool isActive = false;

    [SerializeField] byte[] rgbData;
    [SerializeField] byte[] depthData;
    [SerializeField] ushort[] segmentData;

    [Header ("Frame size")]
    public int rgbCols = 640;
    public int rgbRows = 480;

    public int depthCols = 640;
    public int depthRows = 480;

    public int segmentCols = 640;
    public int segmentRows = 480;

    void Start()
    {
        if (isActive)
        {
            NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
            NuitrackManager.onDepthUpdate += NuitrackManager_onDepthUpdate;
            NuitrackManager.onUserTrackerUpdate += NuitrackManager_onUserTrackerUpdate;
        }
    }

    private void NuitrackManager_onUserTrackerUpdate(nuitrack.UserFrame frame)
    {
        int datasize = frame.DataSize;

        if (segmentData == null || segmentData.Length != datasize)
            segmentData = new ushort[datasize];

        for (int i = 0; i < datasize; i++)
            segmentData[i] = frame[i];

        segmentCols = frame.Cols;
        segmentRows = frame.Rows;
    }

    private void NuitrackManager_onDepthUpdate(nuitrack.DepthFrame frame)
    {
        int datasize = frame.DataSize;

        if (depthData == null || depthData.Length != datasize)
            depthData = new byte[datasize];

        depthData = new byte[datasize];
        Marshal.Copy(frame.Data, depthData, 0, depthData.Length);

        depthCols = frame.Cols;
        depthRows = frame.Rows;
    }

    void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        int datasize = frame.DataSize;
        
        if (rgbData == null || rgbData.Length != datasize)
            rgbData = new byte[datasize];

        Marshal.Copy(frame.Data, rgbData, 0, datasize);

        rgbCols = frame.Cols;
        rgbRows = frame.Rows;
    }

    public byte[] RGB
    {
        get
        {
            return rgbData;
        }
    }

    public byte[] Depth
    {
        get
        {
            return depthData;
        }
    }

    public ushort[] Segment
    {
        get
        {
            return segmentData;
        }
    }

}
