using UnityEngine;

using UnityEngine.UI;

public class RGBand3DSkeleton : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] AspectRatioFitter aspectRatioFitter;

    bool complate = false;

    void Awake()
    {
        NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
    }

    private void OnDestroy()
    {
        if(!complate)
            NuitrackManager.onColorUpdate -= NuitrackManager_onColorUpdate;
    }

    private void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
    {
        NuitrackManager.onColorUpdate -= NuitrackManager_onColorUpdate;

        // Setting the aspect ratio RGB image is the same as that of nuitrack.ColorFrame.
        aspectRatioFitter.aspectRatio = (float)frame.Cols / frame.Rows;

        nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

        // Setting the camera's vFOV equal to the depth sensor's vFOV. 
        // Nuitrack does not yet have a representation of vFOV, so we use the hFOV to vFOV conversion.
        float vFOV = mode.HFOV * ((float)frame.Rows / frame.Cols);
        camera.fieldOfView = vFOV * Mathf.Rad2Deg;

        complate = true;
    }
}
