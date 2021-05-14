using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_ANDROID && UNITY_2018_1_OR_NEWER && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class NuitrackModules : MonoBehaviour
{
    [SerializeField] GameObject depthUserVisualizationPrefab;
    [SerializeField] GameObject depthUserMeshVisualizationPrefab;
    [SerializeField] GameObject skeletonsVisualizationPrefab;
    [SerializeField] GameObject gesturesVisualizationPrefab;
    [SerializeField] GameObject handTrackerVisualizationPrefab;
    [SerializeField] GameObject issuesProcessorPrefab;

    ExceptionsLogger exceptionsLogger;

    [SerializeField] TextMesh perfomanceInfoText;

    [SerializeField] GameObject standardCamera, threeViewCamera;
    [SerializeField] GameObject indirectAvatar, directAvatar, indirectAvatarMan, directAvatarMan;

    [SerializeField] GameObject sensorFrame;

    [SerializeField] Dropdown dropdownModelSwitcher;

    int sensorFrameId = 0;

    public void SwitchCamera()
    {
        standardCamera.SetActive(!standardCamera.activeSelf);
        threeViewCamera.SetActive(!threeViewCamera.activeSelf);
    }

    void Awake()
    {
        exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
        NuitrackInitState state = NuitrackLoader.initState;
        if (state != NuitrackInitState.INIT_OK)
        {
            exceptionsLogger.AddEntry("Nuitrack native libraries initialization error: " + Enum.GetName(typeof(NuitrackInitState), state));
        }
    }

    GameObject root;
    GameObject skelVis;
    int skelVisId;

    public void SwitchModelByIndex(int id)
    {
        skelVisId = id;
        if (!root)
            root = GameObject.Find("Root_1");

        if(root)
            root.SetActive(skelVisId == 0);
        skelVis.SetActive(skelVisId == 0);
        indirectAvatar.SetActive(skelVisId == 1);
        directAvatar.SetActive(skelVisId == 2);
        indirectAvatarMan.SetActive(skelVisId == 3);
        directAvatarMan.SetActive(skelVisId == 4);
    }

    bool prevDepth = false;
    bool prevColor = false;
    bool prevUser = false;
    bool prevSkel = false;
    bool prevHand = false;
    bool prevGesture = false;

    bool currentDepth, currentColor, currentUser, currentSkeleton, currentHands, currentGestures;

    public void ChangeModules(bool depthOn, bool colorOn, bool userOn, bool skeletonOn, bool handsOn, bool gesturesOn, bool force = false)
    {
        try
        {
            InitTrackers(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn, force);
            //issuesProcessor = (GameObject)Instantiate(issuesProcessorPrefab);
        }
        catch (Exception ex)
        {
            exceptionsLogger.AddEntry(ex.ToString());
        }
    }

    private void InitTrackers(bool depthOn, bool colorOn, bool userOn, bool skeletonOn, bool handsOn, bool gesturesOn, bool force)
    {
        if(!NuitrackManager.Instance.nuitrackInitialized)
            exceptionsLogger.AddEntry(NuitrackManager.Instance.initException.ToString());

        if (prevDepth != depthOn || force)
        {
            prevDepth = depthOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevColor != colorOn || force)
        {
            prevColor = colorOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevUser != userOn || force)
        {
            prevUser = userOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (skeletonOn != prevSkel || force)
        {
            prevSkel = skeletonOn;
            if (skelVisId == 0)
            {
                if (root)
                    root.SetActive(true);
                skelVis.SetActive(true);
            }
            if (skelVisId == 1)
                indirectAvatar.SetActive(skeletonOn);
            if (skelVisId == 2)
                directAvatar.SetActive(skeletonOn);
            if (skelVisId == 3)
                indirectAvatarMan.SetActive(skeletonOn);
            if (skelVisId == 4)
                directAvatarMan.SetActive(skeletonOn);
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevHand != handsOn || force)
        {
            prevHand = handsOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevGesture != gesturesOn || force)
        {
            prevGesture = gesturesOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }
    }

    public void InitModules()
    {
        if (!NuitrackManager.Instance.nuitrackInitialized)
            return;

        try
        {
            Instantiate(issuesProcessorPrefab);
            Instantiate(depthUserVisualizationPrefab);
            Instantiate(depthUserMeshVisualizationPrefab);
            skelVis = Instantiate(skeletonsVisualizationPrefab);
            Instantiate(handTrackerVisualizationPrefab);
            Instantiate(gesturesVisualizationPrefab);
        }
        catch (Exception ex)
        {
            exceptionsLogger.AddEntry(ex.ToString());
        }
    }

    void Update()
    {
        try
        {
            string processingTimesInfo = "";
            if ((NuitrackManager.UserTracker != null) && (NuitrackManager.UserTracker.GetProcessingTime() > 1f)) processingTimesInfo += "User FPS: " + (1000f / NuitrackManager.UserTracker.GetProcessingTime()).ToString("0") + "\n";
            if ((NuitrackManager.SkeletonTracker != null) && (NuitrackManager.SkeletonTracker.GetProcessingTime() > 1f)) processingTimesInfo += "Skeleton FPS: " + (1000f / NuitrackManager.SkeletonTracker.GetProcessingTime()).ToString("0") + "\n";
            if ((NuitrackManager.HandTracker != null) && (NuitrackManager.HandTracker.GetProcessingTime() > 1f)) processingTimesInfo += "Hand FPS: " + (1000f / NuitrackManager.HandTracker.GetProcessingTime()).ToString("0") + "\n";

            perfomanceInfoText.text = processingTimesInfo;

            nuitrack.Nuitrack.Update();
        }
        catch (Exception ex)
        {
            exceptionsLogger.AddEntry(ex.ToString());
        }
    }

    public void SwitchSensorFrame()
    {
        sensorFrameId++;
        if (sensorFrameId > 1)
            sensorFrameId = 0;

        if(sensorFrameId == 1)
        {
            sensorFrame.SetActive(true);
        }
        else if (sensorFrameId == 0)
        {
            sensorFrame.SetActive(false);
        }
    }

    public void SwitchNuitrackAi()
    {
        NuitrackManager.Instance.EnableNuitrackAI(!NuitrackManager.Instance.useNuitrackAi);
        ChangeModules(prevDepth, prevColor, prevUser, prevSkel, prevHand, prevGesture, true);
    }
}
