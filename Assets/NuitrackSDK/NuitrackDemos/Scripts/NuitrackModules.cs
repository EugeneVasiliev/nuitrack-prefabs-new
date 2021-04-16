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
    [SerializeField] GameObject indirectAvatar, directAvatar;

    Dropdown m_Dropdown;

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

        m_Dropdown = FindObjectOfType<Dropdown>();
        //Add listener for when the value of the Dropdown changes, to take action
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
    }

    GameObject root;
    GameObject skelVis;
    int skelVisId;

    void DropdownValueChanged(Dropdown change)
    {
        skelVisId = change.value;
        if (!root)
            root = GameObject.Find("Root_1");

        root.SetActive(change.value == 0);
        skelVis.SetActive(change.value == 0);
        indirectAvatar.SetActive(change.value == 1);
        directAvatar.SetActive(change.value == 2);
    }

    bool prevDepth = false;
    bool prevColor = false;
    bool prevUser = false;
    bool prevSkel = false;
    bool prevHand = false;
    bool prevGesture = false;

    bool currentDepth, currentColor, currentUser, currentSkeleton, currentHands, currentGestures;

    public void ChangeModules(bool depthOn, bool colorOn, bool userOn, bool skeletonOn, bool handsOn, bool gesturesOn)
    {
        try
        {
            InitTrackers(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
            //issuesProcessor = (GameObject)Instantiate(issuesProcessorPrefab);
        }
        catch (Exception ex)
        {
            exceptionsLogger.AddEntry(ex.ToString());
        }
    }

    private void InitTrackers(bool depthOn, bool colorOn, bool userOn, bool skeletonOn, bool handsOn, bool gesturesOn)
    {
        if(!NuitrackManager.Instance.nuitrackInitialized)
            exceptionsLogger.AddEntry(NuitrackManager.Instance.initException.ToString());

        if (prevDepth != depthOn)
        {
            prevDepth = depthOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevColor != colorOn)
        {
            prevColor = colorOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevUser != userOn)
        {
            prevUser = userOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (skeletonOn != prevSkel)
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
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevHand != handsOn)
        {
            prevHand = handsOn;
            NuitrackManager.Instance.ChangeModulsState(skeletonOn, handsOn, depthOn, colorOn, gesturesOn, userOn);
        }

        if (prevGesture != gesturesOn)
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
}
