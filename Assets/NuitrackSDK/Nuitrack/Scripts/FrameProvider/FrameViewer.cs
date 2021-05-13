using UnityEngine;

using UnityEngine.Events;
using FrameProviderModules;

public class FrameViewer : MonoBehaviour
{
    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }

    public enum FrameMode
    {
        Color = 0,
        Depth = 1,
        Segment = 2
    }

    public enum TextureMode
    {
        RenderTexture = 0,
        Texture2D = 1,
        Texture = 2
    }

    [SerializeField] FrameMode frameMode;
    [SerializeField] TextureMode textureMode;

    [SerializeField] TextureEvent onFrameUpdate;

    private void Update()
    {
        Texture texture = GetTexture();

        onFrameUpdate.Invoke(texture);
    }

    Texture GetTexture()
    {
        FrameToTexture converter = GetFrameConverter();

        switch (textureMode)
        {
            case TextureMode.RenderTexture:
                return converter.GetRenderTexture();
            case TextureMode.Texture2D:
                return converter.GetTexture2D();
            case TextureMode.Texture:
                return converter.GetTexture();
            default:
                return null;
        }
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