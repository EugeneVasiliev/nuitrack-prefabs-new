using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;

#if UNITY_ANDROID && UNITY_2018_1_OR_NEWER && !UNITY_EDITOR
using UnityEngine.Android;
#endif

[System.Serializable]
public class InitEvent : UnityEvent<NuitrackInitState>
{
}

enum WifiConnect
{
    none, VicoVR, TVico,
}

[HelpURL("https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/")]
public class NuitrackManager : MonoBehaviour
{
    bool _threadRunning;
    Thread _thread;

    public NuitrackInitState InitState { get { return NuitrackLoader.initState; } }
    [SerializeField]
    bool
    depthModuleOn = true,
    colorModuleOn = true,
    userTrackerModuleOn = true,
    skeletonTrackerModuleOn = true,
    gesturesRecognizerModuleOn = true,
    handsTrackerModuleOn = true;

    [Tooltip("Only skeleton. PC, Unity Editor, MacOS and IOS\n Please read this (Wireless case section): github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case")]
    [SerializeField] WifiConnect wifiConnect = WifiConnect.none;
    [SerializeField] bool runInBackground = false;
    [Tooltip("Asynchronous initialization, allows you to turn on the nuitrack more smoothly. In this case, you need to ensure that all components that use this script will start only after its initialization.")]
    [SerializeField] bool asyncInit = false;

    [Header("Config stats")]
    [Tooltip("Depth map doesn't accurately match an RGB image. Turn on this to align them")]
    public bool depth2ColorRegistration = false;
    [Tooltip("ONLY PC! Nuitrack AI is the new version of Nuitrack skeleton tracking middleware\n MORE: github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md")]
    public bool useNuitrackAi = false;
    [Tooltip("Track and get information about faces with Nuitrack (position, angle of rotation, box, emotions, age, gender).\n Tutotial: github.com/3DiVi/nuitrack-sdk/blob/master/doc/Unity_Face_Tracking.md")]
    public bool useFaceTracking = false;
    [Tooltip("Mirror sensor data")]
    public bool mirror = false;

    public static bool sensorConnected = false;
    public static nuitrack.DepthSensor DepthSensor { get; private set; }
    public static nuitrack.ColorSensor ColorSensor { get; private set; }
    public static nuitrack.UserTracker UserTracker { get; private set; }
    public static nuitrack.SkeletonTracker SkeletonTracker { get; private set; }
    public static nuitrack.GestureRecognizer GestureRecognizer { get; private set; }
    public static nuitrack.HandTracker HandTracker { get; private set; }
    public static nuitrack.DepthFrame DepthFrame { get; private set; }
    public static nuitrack.ColorFrame ColorFrame { get; private set; }
    public static nuitrack.UserFrame UserFrame { get; private set; }
    public static nuitrack.SkeletonData SkeletonData { get; private set; }
    public static nuitrack.HandTrackerData HandTrackerData { get; private set; }

    public static event nuitrack.DepthSensor.OnUpdate onDepthUpdate;
    public static event nuitrack.ColorSensor.OnUpdate onColorUpdate;
    public static event nuitrack.UserTracker.OnUpdate onUserTrackerUpdate;
    public static event nuitrack.SkeletonTracker.OnSkeletonUpdate onSkeletonTrackerUpdate;
    public static event nuitrack.HandTracker.OnUpdate onHandsTrackerUpdate;

    public delegate void OnNewGestureHandler(nuitrack.Gesture gesture);
    public static event OnNewGestureHandler onNewGesture;
    public static nuitrack.UserHands СurrentHands { get; private set; }

    static NuitrackManager instance;
    NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_MANAGER_NOT_INSTALLED;
    [SerializeField] InitEvent initEvent;

    bool prevSkel = false;
    bool prevHand = false;
    bool prevDepth = false;
    bool prevColor = false;
    bool prevGest = false;
    bool prevUser = false;

    bool pauseState = false;

    [HideInInspector] public bool nuitrackInitialized = false;

    [HideInInspector] public System.Exception initException;

#if UNITY_ANDROID && !UNITY_EDITOR
    static int GetAndroidAPILevel()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
#endif

    void ThreadedWork()
    {
        _threadRunning = true;

        while (_threadRunning)
        {
            NuitrackInit();
        }
    }

    public static NuitrackManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NuitrackManager>();
                if (instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "NuitrackManager";
                    instance = container.AddComponent<NuitrackManager>();
                }

                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private bool IsNuitrackLibrariesInitialized()
    {
        if (initState == NuitrackInitState.INIT_OK || wifiConnect != WifiConnect.none)
            return true;
        return false;
    }

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(AndroidStart());
#else
        FirstStart();
#endif
    }

    void FirstStart()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.targetFrameRate = 60;
        Application.runInBackground = runInBackground;
        //Debug.Log ("NuitrackStart");
        initState = NuitrackLoader.InitNuitrackLibraries();

        if (asyncInit)
        {
            StartCoroutine(InitEventStart());
            if (!_threadRunning)
            {
                _thread = new Thread(ThreadedWork);
                _thread.Start();
            }
        }
        else
        {
            if (initEvent != null)
            {
                initEvent.Invoke(initState);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsNuitrackLibrariesInitialized())
#endif
            NuitrackInit();
        }
    }

    IEnumerator AndroidStart()
    {
#if UNITY_ANDROID && UNITY_2018_1_OR_NEWER && !UNITY_EDITOR

        while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            yield return null;
        }

        while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            yield return null;
        }

        while (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
            yield return null;
        }

        if (GetAndroidAPILevel() > 26) // camera permissions required for Android newer than Oreo 8
        {
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                yield return null;
            }
        }

        yield return null;
#endif

        FirstStart();

        yield return null;
    }

    public void ChangeModulesState(bool skel, bool hand, bool depth, bool color, bool gest, bool user)
    {
        //Debug.Log("" + skel + hand + depth + gest + user);
        skeletonTrackerModuleOn = skel;
        handsTrackerModuleOn = hand;
        depthModuleOn = depth;
        colorModuleOn = color;
        gesturesRecognizerModuleOn = gest;
        userTrackerModuleOn = user;

        if (SkeletonTracker == null)
            return;
        if (prevSkel != skel)
        {
            SkeletonData = null;
            prevSkel = skel;
            if (skel)
            {
                SkeletonTracker.OnSkeletonUpdateEvent += HandleOnSkeletonUpdateEvent;
            }
            else
            {
                SkeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
            }
        }

        if (prevHand != hand)
        {
            HandTrackerData = null;
            prevHand = hand;
            if (hand)
                HandTracker.OnUpdateEvent += HandleOnHandsUpdateEvent;
            else
                HandTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;
        }
        if (prevGest != gest)
        {
            prevGest = gest;
            if (gest)
                GestureRecognizer.OnNewGesturesEvent += OnNewGestures;
            else
                GestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
        }
        if (prevDepth != depth)
        {
            DepthFrame = null;
            prevDepth = depth;
            if (depth)
                DepthSensor.OnUpdateEvent += HandleOnDepthSensorUpdateEvent;
            else
                DepthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
        }
        if (prevColor != color)
        {
            ColorFrame = null;
            prevColor = color;
            if (color)
                ColorSensor.OnUpdateEvent += HandleOnColorSensorUpdateEvent;
            else
                ColorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
        }
        if (prevUser != user)
        {
            UserFrame = null;
            prevUser = user;
            if (user)
                UserTracker.OnUpdateEvent += HandleOnUserTrackerUpdateEvent;
            else
                UserTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
        }
    }

    void NuitrackInit()
    {
        try
        {
            if (nuitrackInitialized)
                return;
            //Debug.Log("Application.runInBackground " + Application.runInBackground);
            //CloseUserGen(); //just in case
            if (wifiConnect == WifiConnect.VicoVR)
            {
                nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
                nuitrack.Nuitrack.SetConfigValue("Settings.IPAddress", "192.168.1.1");
            }
            else if (wifiConnect == WifiConnect.TVico)
            {
                Debug.Log("If something doesn't work, then read this (Wireless case section): github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#wireless-case");
                nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
                nuitrack.Nuitrack.SetConfigValue("Settings.IPAddress", "192.168.43.1");
            }
            else
            {
                nuitrack.Nuitrack.Init();

                if (depth2ColorRegistration)
                {
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                }

                if (useNuitrackAi)
                {
                    if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.isEditor)
                    {
                        nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                        nuitrack.Nuitrack.SetConfigValue("Skeletonization.Type", "CNN_HPE");
                    }
                    else
                    {
                        Debug.LogWarning("NuitrackAI doesn't support this platform: " + Application.platform + ". https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md");
                    }
                }

                if (useFaceTracking)
                {
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Depth2ColorRegistration", "true");
                    nuitrack.Nuitrack.SetConfigValue("Faces.ToUse", "true");
                }

                if (mirror)
                {
                    nuitrack.Nuitrack.SetConfigValue("DepthProvider.Mirror", "true");
                }

                Debug.Log(
                    "Nuitrack Config parameters:\n" +
                    "Skeletonization Type: " + nuitrack.Nuitrack.GetConfigValue("Skeletonization.Type") + "\n" +
                    "Faces using: " + nuitrack.Nuitrack.GetConfigValue("Faces.ToUse"));
            }

            Debug.Log("Init OK");

            DepthSensor = nuitrack.DepthSensor.Create();

            ColorSensor = nuitrack.ColorSensor.Create();

            UserTracker = nuitrack.UserTracker.Create();

            SkeletonTracker = nuitrack.SkeletonTracker.Create();

            GestureRecognizer = nuitrack.GestureRecognizer.Create();

            HandTracker = nuitrack.HandTracker.Create();

            nuitrack.Nuitrack.Run();
            Debug.Log("Run OK");

            ChangeModulesState(
                skeletonTrackerModuleOn,
                handsTrackerModuleOn,
                depthModuleOn,
                colorModuleOn,
                gesturesRecognizerModuleOn,
                userTrackerModuleOn
            );

            nuitrackInitialized = true;
            _threadRunning = false;
        }
        catch (System.Exception ex)
        {
            initException = ex;
            NuitrackErrorSolver.CheckError(ex);
        }
    }

    void HandleOnDepthSensorUpdateEvent(nuitrack.DepthFrame frame)
    {
        if (DepthFrame != null)
            DepthFrame.Dispose();
        DepthFrame = (nuitrack.DepthFrame)frame.Clone();
        //Debug.Log("Depth Update");
        onDepthUpdate?.Invoke(DepthFrame);
    }

    void HandleOnColorSensorUpdateEvent(nuitrack.ColorFrame frame)
    {
        if (ColorFrame != null)
            ColorFrame.Dispose();
        ColorFrame = (nuitrack.ColorFrame)frame.Clone();
        //Debug.Log("Color Update");
        onColorUpdate?.Invoke(ColorFrame);
    }

    void HandleOnUserTrackerUpdateEvent(nuitrack.UserFrame frame)
    {
        if (UserFrame != null)
            UserFrame.Dispose();
        UserFrame = (nuitrack.UserFrame)frame.Clone();
        onUserTrackerUpdate?.Invoke(UserFrame);
    }

    void HandleOnSkeletonUpdateEvent(nuitrack.SkeletonData _skeletonData)
    {
        if (SkeletonData != null)
            SkeletonData.Dispose();
        SkeletonData = (nuitrack.SkeletonData)_skeletonData.Clone();
        //Debug.Log("Skeleton Update ");
        sensorConnected = true;
        onSkeletonTrackerUpdate?.Invoke(SkeletonData);
    }

    private void OnNewGestures(nuitrack.GestureData gestures)
    {
        if (gestures.NumGestures > 0)
        {
            if (onNewGesture != null)
            {
                for (int i = 0; i < gestures.Gestures.Length; i++)
                {
                    onNewGesture(gestures.Gestures[i]);
                }
            }
        }
    }

    void HandleOnHandsUpdateEvent(nuitrack.HandTrackerData _handTrackerData)
    {
        if (HandTrackerData != null)
            HandTrackerData.Dispose();
        HandTrackerData = (nuitrack.HandTrackerData)_handTrackerData.Clone();
        onHandsTrackerUpdate?.Invoke(HandTrackerData);

        //Debug.Log ("Grabbed hands");
        if (HandTrackerData == null) return;
        if (CurrentUserTracker.CurrentUser != 0)
        {
            СurrentHands = HandTrackerData.GetUserHandsByID(CurrentUserTracker.CurrentUser);
        }
        else
        {
            СurrentHands = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        //Debug.Log("pauseStatus " + pauseStatus);
        if (pauseStatus)
        {
            StopNuitrack();
            pauseState = true;
        }
        else
        {
            StartCoroutine(RestartNuitrack());
        }
    }

    IEnumerator RestartNuitrack()
    {
        yield return null;

        while (pauseState)
        {
            StartNuitrack();
            pauseState = false;
            yield return null;
        }
        yield return null;
    }

    public void StartNuitrack()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsNuitrackLibrariesInitialized())
            return;
#endif
        if (asyncInit)
        {
            if (!_threadRunning)
            {
                _thread = new Thread(ThreadedWork);
                _thread.Start();
            }
        }
        else
        {
            NuitrackInit();
        }
    }

    public void StopNuitrack()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsNuitrackLibrariesInitialized())
            return;
#endif
        prevSkel = false;
        prevHand = false;
        prevGest = false;
        prevDepth = false;
        prevColor = false;
        prevUser = false;

        CloseUserGen();
    }

    IEnumerator InitEventStart()
    {
        while (!nuitrackInitialized)
        {
            yield return new WaitForEndOfFrame();
        }

        if (initEvent != null)
        {
            initEvent.Invoke(initState);
        }
    }

    bool licenseIsOver = false;
    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (IsNuitrackLibrariesInitialized())
#endif
        if (!pauseState || (asyncInit && _threadRunning))
        {
            try
            {
                nuitrack.Nuitrack.Update();
            }
            catch (System.Exception ex)
            {
                if (ex.ToString().Contains("LicenseNotAcquiredException") && licenseIsOver == false)
                {
                    licenseIsOver = true;
                    if (Time.time > 5.0f)
                        Debug.LogError("Nuitrack Trial time is over. Restart app. For unlimited time of use, you can switch to another license https://nuitrack.com/#pricing");
                    else
                        Debug.LogError("Activate Nuitrack license https://nuitrack.com/#pricing");
                }
                else
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }
    }

    public void DepthModuleClose()
    {
        //Debug.Log ("changeModuls: start");
        //if (!depthModuleOn)
        //    return;
        depthModuleOn = false;
        userTrackerModuleOn = false;
        colorModuleOn = false;
        ChangeModulesState(
            skeletonTrackerModuleOn,
            handsTrackerModuleOn,
            depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
            userTrackerModuleOn
        );
        //Debug.Log ("changeModuls: end");
    }

    public void DepthModuleStart()
    {
        //if (depthModuleOn)
        //    return;
        depthModuleOn = true;
        userTrackerModuleOn = true;
        colorModuleOn = true;
        Debug.Log("DepthModuleStart");
        ChangeModulesState(
            skeletonTrackerModuleOn,
            handsTrackerModuleOn,
            depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
            userTrackerModuleOn
        );
    }

    public void EnableNuitrackAI(bool use)
    {
        StopNuitrack();
        useNuitrackAi = use;
        StartNuitrack();
    }

    public void CloseUserGen()
    {
        try
        {
            if (DepthSensor != null) DepthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
            if (ColorSensor != null) ColorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
            if (UserTracker != null) UserTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
            if (SkeletonTracker != null) SkeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
            if (GestureRecognizer != null) GestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
            if (HandTracker != null) HandTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;

            DepthFrame = null;
            ColorFrame = null;
            UserFrame = null;
            SkeletonData = null;
            HandTrackerData = null;

            DepthSensor = null;
            ColorSensor = null;
            UserTracker = null;
            SkeletonTracker = null;
            GestureRecognizer = null;
            HandTracker = null;

            nuitrack.Nuitrack.Release();
            Debug.Log("CloseUserGen");
            nuitrackInitialized = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    void OnDestroy()
    {
        CloseUserGen();
    }

    void OnDisable()
    {
        StopThread();
    }

    void StopThread()
    {
        if (_threadRunning)
        {
            _threadRunning = false;
            _thread.Join();
        }
    }
}
