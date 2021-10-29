using UnityEngine;

using NuitrackSDK.Frame;
using UnityEngine.Events;


namespace NuitrackSDK.Face
{
    [AddComponentMenu("NuitrackSDK/Face/Face Cropper")]
    public class FaceCropper : MonoBehaviour
    {
        [System.Serializable]
        public class TextureEvent : UnityEvent<Texture> { }

        [Header("User track settings")]
        [SerializeField, NuitrackSDKInspector] 
        bool useCurrentUser = true;

        [SerializeField, NuitrackSDKInspector, Range(1, 6)] 
        int userID = 0;

        [SerializeField, NuitrackSDKInspector, Range(0, 5)] 
        float loseTime = 0.25f;

        float t = 0;

        [Header("Visualisation")]
        [SerializeField, NuitrackSDKInspector, Range(0, 1)] 
        float margin = 0.1f;

        [SerializeField, NuitrackSDKInspector] 
        Texture2D noUserImage;

        [SerializeField, NuitrackSDKInspector] 
        bool smoothMove = true;

        [SerializeField, NuitrackSDKInspector, Range(0.1f, 24f)] 
        float smoothSpeed = 4f;

        [Header("Advanced")]
        [SerializeField, NuitrackSDKInspector] 
        bool useGPUCrop = true;

        Rect faceRect;
        Rect targerRect;

        [Header("Output")]
        [SerializeField, NuitrackSDKInspector] 
        TextureEvent onFrameUpdate;

        public UserData ControllerUser
        {
            get
            {
                if (useCurrentUser)
                    return NuitrackManager.Users.Current;
                else
                    return NuitrackManager.Users.GetUser(userID);
            }
        }

        void ResetFrame()
        {
            onFrameUpdate.Invoke(noUserImage);
            faceRect = default;
        }

        Texture2D croppedTexture;

        void OnDestroy()
        {
            if (croppedTexture != null)
                Destroy(croppedTexture);
        }

        void Update()
        {
            UserData userData = ControllerUser;

            if (userData == null || userData.Face == null)
            {
                if (t > loseTime)
                    ResetFrame();
                else if (t <= loseTime)
                    t += Time.deltaTime;

                return;
            }
            else
                t = 0;

            Texture2D frame = NuitrackManager.ColorFrame.ToTexture2D();

            if (frame == null)
            {
                ResetFrame();
                return;
            }

            targerRect = userData.Face.ScreenRect(frame.width, frame.height);

            if (faceRect.Equals(default))
                faceRect = targerRect;

            Vector2 deltaSize = new Vector2(targerRect.width * (1 + margin), targerRect.height * (1 + margin));

            targerRect.position -= deltaSize * 0.5f;
            targerRect.size += deltaSize;

            targerRect.xMin = Mathf.Clamp(targerRect.xMin, 0, frame.width);
            targerRect.xMax = Mathf.Clamp(targerRect.xMax, 0, frame.width);

            targerRect.yMin = Mathf.Clamp(targerRect.yMin, 0, frame.height);
            targerRect.yMax = Mathf.Clamp(targerRect.yMax, 0, frame.height);

            if (croppedTexture != null)
                Destroy(croppedTexture);

            croppedTexture = new Texture2D((int)faceRect.width, (int)faceRect.height, TextureFormat.RGBA32, false);

            if (useGPUCrop)
                Graphics.CopyTexture(frame, 0, 0, (int)faceRect.x, (int)faceRect.y, (int)faceRect.width, (int)faceRect.height, croppedTexture, 0, 0, 0, 0);
            else
            {
                Color[] pixels = frame.GetPixels((int)faceRect.x, (int)faceRect.y, (int)faceRect.width, (int)faceRect.height);
                croppedTexture.SetPixels(pixels);
                croppedTexture.Apply();
            }

            onFrameUpdate.Invoke(croppedTexture);

            if (smoothMove)
            {
                faceRect.position = Vector2.Lerp(faceRect.position, targerRect.position, Time.deltaTime * smoothSpeed);
                faceRect.size = Vector2.Lerp(faceRect.size, targerRect.size, Time.deltaTime * smoothSpeed);
            }
            else
                faceRect = targerRect;
        }
    }
}