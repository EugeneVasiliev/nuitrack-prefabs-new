using UnityEngine;
using UnityEngine.UI;

public class Frame2Depth : MonoBehaviour
{
    [SerializeField] RawImage background;
    [SerializeField] ComputeShader BGR2RGBShader;

    RenderTexture renderTexture;

    void Start()
    {
        NuitrackManager.onColorUpdate += DrawColor;
    }

    void OnDestroy()
    {
        NuitrackManager.onColorUpdate -= DrawColor;
    }

    void DrawColor(nuitrack.ColorFrame frame)
    {
        NuitrackUtils.ToRenderTexture(frame, ref renderTexture, BGR2RGBShader);
        background.texture = renderTexture;
    }

    void ToRenderTexture(nuitrack.DepthFrame frame, ref RenderTexture renderTexture, ComputeShader frame2DepthShader, float contrast = 0.9f)
    {
        int de = 1 + 255 - (int)(contrast * 255);

        byte[] outDepth = new byte[(frame.DataSize / 2) * 3];
        for (int i = 0; i < frame.DataSize / 2; i++)
        {
            byte depth = (byte)(frame[i] / de);

            Color32 currentColor = new Color32(depth, depth, depth, 255);

            int ptr = i * 3;

            outDepth[ptr] = currentColor.r;
            outDepth[ptr + 1] = currentColor.g;
            outDepth[ptr + 2] = currentColor.b;
        }

        Texture2D depthTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
        depthTexture.LoadRawTextureData(outDepth);
        depthTexture.Apply();
    }
}