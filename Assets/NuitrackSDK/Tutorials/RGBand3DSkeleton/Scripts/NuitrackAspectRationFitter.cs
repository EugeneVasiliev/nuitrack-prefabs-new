using UnityEngine;
using UnityEngine.UI;

namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/Nuitrack Aspect Ration Fitter")]
    public class NuitrackAspectRationFitter : AspectRatioFitter
    {
        public enum FrameMode
        {
            Color = 0,
            Depth = 1,
            Segment = 2
        }

        [SerializeField] FrameMode frameMode = FrameMode.Color;

        protected override void OnEnable()
        {
            base.OnEnable();

            switch (frameMode)
            {
                case FrameMode.Color:
                    NuitrackManager.onColorUpdate += NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Depth:
                    NuitrackManager.onDepthUpdate += NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Segment:
                    NuitrackManager.onUserTrackerUpdate += NuitrackManager_onFrameUpdate;
                    break;
            }
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            switch (frameMode)
            {
                case FrameMode.Color:
                    NuitrackManager.onColorUpdate -= NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Depth:
                    NuitrackManager.onDepthUpdate -= NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Segment:
                    NuitrackManager.onUserTrackerUpdate -= NuitrackManager_onFrameUpdate;
                    break;
            }
        }


        void NuitrackManager_onFrameUpdate<T>(nuitrack.Frame<T> frame) where T : struct
        {
            float frameAspectRatio = (float)frame.Cols / frame.Rows;
            aspectRatio = frameAspectRatio;
        }
    }
}