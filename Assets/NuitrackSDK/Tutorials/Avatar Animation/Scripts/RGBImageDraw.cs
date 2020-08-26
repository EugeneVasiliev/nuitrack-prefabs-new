using UnityEngine;
using UnityEngine.UI;

public class RGBImageDraw : MonoBehaviour
{
    [SerializeField] Image background;

    Rect rect;
    Texture2D texture;
    Sprite sprite;

    void Start()
    {
        NuitrackManager.onColorUpdate += DrawColor;
    }

    private void OnDestroy()
    {
        NuitrackManager.onColorUpdate -= DrawColor;
    }

    void DrawColor(nuitrack.ColorFrame frame)
    {
        rect = new Rect(0, 0, frame.Cols, frame.Rows);
        NuitrackUtils.ToTexture2D(frame, ref texture);

        sprite = Sprite.Create(texture, rect, Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        background.sprite = sprite;
        background.preserveAspect = true;
    }

}