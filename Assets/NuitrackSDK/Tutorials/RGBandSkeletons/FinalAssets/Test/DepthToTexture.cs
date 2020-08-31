using UnityEngine;
using UnityEngine.UI;

public class DepthToTexture : MonoBehaviour
{
    [SerializeField] RawImage rawImage;

    [SerializeField] RawImage rawSourceImage;
    [SerializeField] ComputeShader segment2Texture;

    RenderTexture renderTexture;

    [SerializeField] Color[] defaultColors = new Color[]
    {
        Color.clear,
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow,
        Color.cyan,
        Color.grey
    };

    ComputeBuffer userColorsBuffer;
    ComputeBuffer sourceDataBuffer;

    UserFrame userFrame;

    void Start()
    {
        userFrame = new UserFrame();

        userColorsBuffer = new ComputeBuffer(defaultColors.Length, sizeof(float) * 4);
        userColorsBuffer.SetData(defaultColors);

        OldMethod();

        //ushort[] dataTest = new ushort[] { 1, 2, 3, 4 };
        //uint[] cData = new uint[dataTest.Length / 2];

        //System.Buffer.BlockCopy(dataTest, 0, cData, 0, dataTest.Length * sizeof(ushort));

        //string outStr = "";

        //for (int i = 0; i < cData.Length; i++)
        //    outStr += " | " + cData[i];

        //Debug.Log(outStr);

        //for (int i = 0; i < cData.Length; i++)
        //{
        //    uint first =  (cData[i] << 16) >> 16;
        //    uint second = (cData[i] >> 16);

        //    Debug.Log(first);
        //    Debug.Log(second);
        //}
    }

    void OnDestroy()
    {
        userColorsBuffer.Release();
        sourceDataBuffer.Release();
    }

    void OldMethod()
    {
        byte[] outSegment = new byte[userFrame.DataSize * 4];

        for (int i = 0; i < userFrame.DataSize; i++)
        {
            Color32 currentColor = defaultColors[userFrame[i]];

            int ptr = i * 4;
            outSegment[ptr] = currentColor.a;
            outSegment[ptr + 1] = currentColor.r;
            outSegment[ptr + 2] = currentColor.g;
            outSegment[ptr + 3] = currentColor.b;
        }

        Texture2D segmentTexture = new Texture2D(userFrame.Cols, userFrame.Rows, TextureFormat.ARGB32, false);

        segmentTexture.LoadRawTextureData(outSegment);
        segmentTexture.Apply();

        rawSourceImage.texture = segmentTexture;
    }

    // Update is called once per frame
    void Update()
    {
        ToTexture2D(userFrame);
        rawImage.texture = renderTexture;
    }

    void ToTexture2D(UserFrame frame)
    {
        if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
        {
            renderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            segment2Texture.SetInt("textureWidth", renderTexture.width);

            sourceDataBuffer = new ComputeBuffer(frame.Data.Length, sizeof(uint));
        }

        sourceDataBuffer.SetData(frame.Data);

        uint x, y, z;
        int kernelIndex = segment2Texture.FindKernel("Segment2Texture");

        segment2Texture.SetTexture(kernelIndex, "Result", renderTexture);

        segment2Texture.SetBuffer(kernelIndex, "UserIndexes", sourceDataBuffer);
        segment2Texture.SetBuffer(kernelIndex, "UserColors", userColorsBuffer);

        segment2Texture.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        segment2Texture.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }

    //void ToTexture2D(UserFrame frame)
    //{
    //    if (renderTexture == null || renderTexture.width != frame.Cols || renderTexture.height != frame.Rows)
    //    {
    //        renderTexture = new RenderTexture(frame.Cols, frame.Rows, 0, RenderTextureFormat.ARGB32);
    //        renderTexture.enableRandomWrite = true;
    //        renderTexture.Create();

    //        segment2Texture.SetInt("textureWidth", renderTexture.width);
    //    }

    //    ComputeBuffer sourceDataBuffer = new ComputeBuffer(frame.Data.Length, sizeof(uint));
    //    sourceDataBuffer.SetData(frame.Data);

    //    //string inData = "";

    //    //for (int i = 0; i < 10; i++)
    //    //    inData += " | " + frame.Data[i];

    //    //Debug.Log(inData);

    //    //ComputeBuffer outDataBuffer = new ComputeBuffer(frame.Data.Length, sizeof(uint));

    //    uint x, y, z;
    //    int kernelIndex = segment2Texture.FindKernel("Segment2Texture");

    //    segment2Texture.SetTexture(kernelIndex, "Result", renderTexture);

    //    segment2Texture.SetBuffer(kernelIndex, "UserIndexes", sourceDataBuffer);
    //    segment2Texture.SetBuffer(kernelIndex, "UserColors", userColorsBuffer);

    //    //segment2Texture.SetBuffer(kernelIndex, "UserIndexesOut", outDataBuffer);

    //    segment2Texture.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
    //    segment2Texture.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);

    //    sourceDataBuffer.Release();

    //    //uint[] outData = new uint[frame.Data.Length];
    //    //outDataBuffer.GetData(outData);

    //    //string outDataStr = "";

    //    //for (int i = 0; i < 10; i++)
    //    //    outDataStr += " | " + outData[i];

    //    //Debug.Log(outDataStr);
    //}

    class UserFrame
    {
        public ushort[] Data
        {
            get;
            private set;
        }

        public int Cols
        {
            get;
            private set;
        }

        public int Rows
        {
            get;
            private set;
        }

        public UserFrame()
        {
            Cols = 320;
            Rows = 240;

            Data = new ushort[Cols * Rows];

            for (int i = 0; i < Data.Length; i++)
            {
                //Data[i] = (ushort)(i < Data.Length / 2 ? 1 : 0);
                Data[i] = (ushort)Random.Range(0, 5);
            }
        }

        public ushort this[int i]
        {
            get
            {
                return Data[i];
            }
        }

        public int DataSize
        {
            get
            {
                return Data.Length;
            }
        }
    }
}


//Texture2D ToTexture2D(nuitrack.DepthFrame frame, float contrast = 0.9f)
//{
//    byte[] outDepth = new byte[(frame.DataSize / 2) * 3];
//    int de = 1 + 255 - (int)(contrast * 255);

//    for (int i = 0; i < frame.DataSize / 2; i++)
//    {
//        byte depth = (byte)(frame[i] / de);

//        Color32 currentColor = new Color32(depth, depth, depth, 255);

//        int ptr = i * 3;

//        outDepth[ptr] = currentColor.r;
//        outDepth[ptr + 1] = currentColor.g;
//        outDepth[ptr + 2] = currentColor.b;
//    }

//    Texture2D depthTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
//    depthTexture.LoadRawTextureData(outDepth);
//    depthTexture.Apply();

//    Resources.UnloadUnusedAssets();

//    return depthTexture;
//}
