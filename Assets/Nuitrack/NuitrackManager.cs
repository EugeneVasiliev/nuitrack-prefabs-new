using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NuitrackManager : MonoBehaviour 
{
  [SerializeField]bool 
  depthModuleOn = true,
  userTrackerModuleOn = true,
  skeletonTrackerModuleOn = true,
  gesturesRecognizerModuleOn = true,
  handsTrackerModuleOn = true;

  static nuitrack.DepthSensor depthSensor;
  static nuitrack.UserTracker userTracker;
  static nuitrack.SkeletonTracker skeletonTracker;
  static nuitrack.GestureRecognizer gestureRecognizer;
  static nuitrack.HandTracker handTracker;

  static nuitrack.DepthFrame depthFrame;
  public static nuitrack.DepthFrame DepthFrame {get {return depthFrame;}}
  static nuitrack.UserFrame userFrame;
  public static nuitrack.UserFrame UserFrame {get {return userFrame;}}
  static nuitrack.SkeletonData skeletonData;
  public static nuitrack.SkeletonData SkeletonData{get {return skeletonData;}}
  static nuitrack.HandTrackerData handTrackerData;
  public static nuitrack.HandTrackerData HandTrackerData {get {return handTrackerData;}}

  public static event nuitrack.DepthSensor.OnUpdate onDepthUpdate;
  public static event nuitrack.UserTracker.OnUpdate onUserTrackerUpdate;
  public static event nuitrack.SkeletonTracker.OnSkeletonUpdate onSkeletonTrackerUpdate;
  public static event nuitrack.HandTracker.OnUpdate onHandsTrackerUpdate;

  public delegate void OnNewGestureHandler(nuitrack.Gesture gesture);
  public static event OnNewGestureHandler onNewGesture;

  static NuitrackManager instance;

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
  
  void Awake()
  {
    NuitrackLoader.InitNuitrackLibraries();
  }
  
  void Start()
  {
    NuitrackInit();
  }
  
  void NuitrackInit()
  {
    CloseUserGen(); //just in case
    nuitrack.Nuitrack.Init();

    if (depthModuleOn)
    {
      depthSensor = nuitrack.DepthSensor.Create();
      depthSensor.OnUpdateEvent += HandleOnDepthSensorUpdateEvent;
    }

    if (userTrackerModuleOn)
    {
      userTracker = nuitrack.UserTracker.Create();
      userTracker.OnUpdateEvent += HandleOnUserTrackerUpdateEvent;
    }

    if (skeletonTrackerModuleOn)
    {
      skeletonTracker = nuitrack.SkeletonTracker.Create();
      skeletonTracker.OnSkeletonUpdateEvent += HandleOnSkeletonUpdateEvent;
    }

    if (gesturesRecognizerModuleOn)
    {
      gestureRecognizer = nuitrack.GestureRecognizer.Create();
      gestureRecognizer.OnNewGesturesEvent += OnNewGestures;
    }

    if (handsTrackerModuleOn)
    {
      handTracker = nuitrack.HandTracker.Create();
      handTracker.OnUpdateEvent += HandleOnHandsUpdateEvent;
    }

    nuitrack.Nuitrack.Run();
  }

  void HandleOnDepthSensorUpdateEvent (nuitrack.DepthFrame frame)
  {
    depthFrame = frame;
    if (onDepthUpdate != null) onDepthUpdate(depthFrame);
  }
  
  void HandleOnUserTrackerUpdateEvent (nuitrack.UserFrame frame)
  {
    userFrame = frame;
    if (onUserTrackerUpdate != null) onUserTrackerUpdate(userFrame);
  }

  void HandleOnSkeletonUpdateEvent (nuitrack.SkeletonData _skeletonData)
  {
    skeletonData = _skeletonData;
    if (onSkeletonTrackerUpdate != null) onSkeletonTrackerUpdate(skeletonData);
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
  
  void HandleOnHandsUpdateEvent (nuitrack.HandTrackerData _handTrackerData)
  {
    handTrackerData = _handTrackerData;
    if (onHandsTrackerUpdate != null) onHandsTrackerUpdate(handTrackerData);
  }
  
  void OnApplicationPause (bool pauseStatus)
  {
    if (pauseStatus)
    {
      CloseUserGen ();
    }
    else
    {
      NuitrackInit ();
    }
  }
  
  void Update()
  {
    nuitrack.Nuitrack.Update();
  }
  
  public void CloseUserGen()
  {
    if (depthSensor != null) depthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
    if (userTracker != null) userTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
    if (skeletonTracker != null) skeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
    if (gestureRecognizer != null) gestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
    if (handTracker != null) handTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;

    nuitrack.Nuitrack.Release();

    depthSensor = null;
    userTracker = null;
    skeletonTracker = null;
    gestureRecognizer = null;
    handTracker = null;

    depthFrame = null;
    userFrame = null;
    skeletonData = null;
    handTrackerData = null;
  }
  
  void OnDestroy()
  {
    CloseUserGen();
  }
}