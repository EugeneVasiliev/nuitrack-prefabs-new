using UnityEngine;

using NuitrackSDK.Frame;
using UnityEngine.Events;


namespace NuitrackSDK.Face
{
    [AddComponentMenu("NuitrackSDK/Face/Face Cropper/Face Cropper")]
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

        /// <summary>
        /// If True, the current user tracker is used, otherwise the user specified by ID is used <see cref="UserID"/>
        /// </summary>
        public bool UseCurrentUserTracker
        {
            get
            {
                return useCurrentUser;
            }
            set
            {
                useCurrentUser = value;
            }
        }

        /// <summary>
        /// ID of the current user
        /// For the case when current user tracker <see cref="UseCurrentUserTracker"/> of is used, the ID of the active user will be returned
        /// If current user tracker is used and a new ID is set, tracking of the current user will stop
        /// </summary>
        public int UserID
        {
            get
            {
                if (UseCurrentUserTracker)
                    return ControllerUser != null ? ControllerUser.ID : 0;
                else
                    return userID;
            }
            set
            {
                if (value >= Users.MinID && value <= Users.MaxID)
                {
                    userID = value;

                    if (useCurrentUser)
                        Debug.Log(string.Format("CurrentUserTracker mode was disabled for {0}", gameObject.name));

                    useCurrentUser = false;
                }
                else
                    throw new System.Exception(string.Format("The User ID must be within the bounds of [{0}, {1}]", Users.MinID, Users.MaxID));
            }
        }

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

        /// <summary>
        /// Cropped face texture (may be null)
        /// </summary>
        public Texture2D CroppedFaceTexture
        {
            get; private set;
        }

        void ResetFrame()
        {
            onFrameUpdate.Invoke(noUserImage);
            faceRect = default;
        }  

        void OnDestroy()
        {
            if (CroppedFaceTexture != null)
                Destroy(CroppedFaceTexture);
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

            Vector2 deltaSize = targerRect.size * margin;

            targerRect.position -= deltaSize * 0.5f;
            targerRect.size += deltaSize;

            targerRect.xMin = Mathf.Clamp(targerRect.xMin, 0, frame.width);
            targerRect.xMax = Mathf.Clamp(targerRect.xMax, 0, frame.width);

            targerRect.yMin = Mathf.Clamp(targerRect.yMin, 0, frame.height);
            targerRect.yMax = Mathf.Clamp(targerRect.yMax, 0, frame.height);

            if (CroppedFaceTexture != null)
                Destroy(CroppedFaceTexture);

            CroppedFaceTexture = new Texture2D((int)faceRect.width, (int)faceRect.height, TextureFormat.RGBA32, false);

            if (useGPUCrop)
                Graphics.CopyTexture(frame, 0, 0, (int)faceRect.x, (int)faceRect.y, (int)faceRect.width, (int)faceRect.height, CroppedFaceTexture, 0, 0, 0, 0);
            else
            {
                Color[] pixels = frame.GetPixels((int)faceRect.x, (int)faceRect.y, (int)faceRect.width, (int)faceRect.height);
                CroppedFaceTexture.SetPixels(pixels);
                CroppedFaceTexture.Apply();
            }

            onFrameUpdate.Invoke(CroppedFaceTexture);

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