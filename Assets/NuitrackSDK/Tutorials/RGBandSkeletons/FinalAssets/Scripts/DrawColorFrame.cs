using UnityEngine;
using UnityEngine.UI;

public class DrawColorFrame : MonoBehaviour
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
}
