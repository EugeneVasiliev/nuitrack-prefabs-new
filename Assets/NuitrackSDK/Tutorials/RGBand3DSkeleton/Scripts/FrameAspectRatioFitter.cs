using UnityEngine;
using UnityEngine.UI;

namespace NuitrackSDK.Tutorials.RGBand3DSkeleton
{
    public class FrameAspectRatioFitter : MonoBehaviour
    {
        [SerializeField] new Camera camera;

        [SerializeField] RectTransform viewCanvas;
        [SerializeField] AspectRatioFitter aspectRatioFitter;

        bool waitEvent = true;

        void OnEnable()
        {
            waitEvent = true;
            NuitrackManager.onColorUpdate += NuitrackManager_onColorUpdate;
        }

        void OnDisable()
        {
            if (waitEvent)
                NuitrackManager.onColorUpdate -= NuitrackManager_onColorUpdate;
        }

        private void NuitrackManager_onColorUpdate(nuitrack.ColorFrame frame)
        {
            NuitrackManager.onColorUpdate -= NuitrackManager_onColorUpdate;

            waitEvent = false;

            float frameAspectRatio = (float)frame.Cols / frame.Rows;
            
            // Setting the aspect ratio RGB image is the same as that of nuitrack.ColorFrame.
            aspectRatioFitter.aspectRatio = frameAspectRatio;

            nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();

            float viewAspectRatio = (float)viewCanvas.rect.width / viewCanvas.rect.height;
            float targetAspectRatio = viewAspectRatio > frameAspectRatio ? (float)frame.Rows / frame.Cols : (float)viewCanvas.rect.height / viewCanvas.rect.width;

            // Setting the camera's vFOV equal to the depth sensor's vFOV. 
            // Nuitrack does not yet have a representation of vFOV, so we use the hFOV to vFOV conversion.
            float vFOV = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV * 0.5f) * targetAspectRatio);
            camera.fieldOfView = vFOV * Mathf.Rad2Deg;
        }
    }
}