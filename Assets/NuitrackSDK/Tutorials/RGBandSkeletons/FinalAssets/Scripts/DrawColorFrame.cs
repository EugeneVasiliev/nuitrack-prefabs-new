using UnityEngine;
using UnityEngine.UI;

public class DrawColorFrame : MonoBehaviour
{
    [SerializeField] RawImage background;
    Texture2D texture2D;

    void Start()
    {
        NuitrackManager.onColorUpdate += DrawColor;
    }

    void DrawColor(nuitrack.ColorFrame frame)
    {
        NuitrackUtils.ToTexture2D(frame, ref texture2D);
        background.texture = texture2D;
    }
}
