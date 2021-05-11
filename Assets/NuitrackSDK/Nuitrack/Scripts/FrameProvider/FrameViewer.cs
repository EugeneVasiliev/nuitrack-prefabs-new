using UnityEngine;

using UnityEngine.Events;

[System.Serializable]
public class TextureEvent : UnityEvent<Texture> { }


public class FrameViewer : MonoBehaviour
{
    public enum FrameMode
    {
        Color = 0,
        Depth = 1,
        Segment = 2
    }

    public enum TextureMode
    {
        RenderTexture = 0,
        Texture2D
    }

    [SerializeField] FrameMode frameMode;
    [SerializeField] TextureMode textureMode;

    [SerializeField] TextureEvent onFrameUpdate;

    private void Update()
    {
        Texture texture = null;

        if (textureMode == TextureMode.Texture2D)
            texture = GetFrameConverter().GetTexture2D();
        else
            texture = GetFrameConverter().GetRenderTexture();

        onFrameUpdate.Invoke(texture);
    }

    FrameToTexture GetFrameConverter()
    {
        switch (frameMode)
        {
            case FrameMode.Color:
                return FrameProvider.ColorFrame;
            case FrameMode.Depth:
                return FrameProvider.DepthFrame;
            case FrameMode.Segment:
                return FrameProvider.UserFrame;
            default:
                return null;
        }
    }
}