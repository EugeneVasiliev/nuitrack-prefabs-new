using UnityEngine;

public class CustomFrameViewer : MonoBehaviour
{
    public enum Mode
    {
        Cut,
        ReverseCut,
        Mul,
        Mix
    }

    [SerializeField] Texture mainTexture;
    [SerializeField] Texture altTexture;

    [SerializeField] Mode mode = Mode.Cut;
    [SerializeField] FrameViewer.TextureEvent onFrameUpdate;

    RenderTexture renderTexture;

    void Update()
    {
        switch(mode)
        {
            case Mode.Cut:
                FrameProvider.FrameUtils.Cut(mainTexture, altTexture, ref renderTexture);
                break;
            case Mode.ReverseCut:
                FrameProvider.FrameUtils.ReverseCut(mainTexture, altTexture, ref renderTexture);
                break;
            case Mode.Mul:
                FrameProvider.FrameUtils.Mul(mainTexture, altTexture, ref renderTexture);
                break;
            case Mode.Mix:
                FrameProvider.FrameUtils.MixMask(mainTexture, altTexture, ref renderTexture);
                break;
        }

        onFrameUpdate.Invoke(renderTexture);
    }

    public void MainTexture(Texture texture)
    {
        mainTexture = texture;
    }

    public void AltTexture(Texture texture)
    {
        altTexture = texture;
    }
}
