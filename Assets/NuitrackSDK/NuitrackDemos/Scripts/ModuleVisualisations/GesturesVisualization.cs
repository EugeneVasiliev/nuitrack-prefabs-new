using UnityEngine;
using System;
using nuitrack;

public class GesturesVisualization : MonoBehaviour
{
    ExceptionsLogger exceptionsLogger;
    NuitrackModules nuitrackModules;
    GestureData gesturesData = null;

    private void OnEnable()
    {
        NuitrackManager.onNewGesture += OnNewGesture;
    }

    private void OnNewGesture(Gesture gesture)
    {
        ProcessGesturesData(NuitrackManager.GestureRecognizer.GetGestureData());
    }

    private void OnDisable()
    {
        NuitrackManager.onNewGesture -= OnNewGesture;
    }

    void Start()
    {
        exceptionsLogger = FindObjectOfType<ExceptionsLogger>();
        nuitrackModules = FindObjectOfType<NuitrackModules>();
    }

    void ProcessGesturesData(GestureData data)
    {
        if (data.NumGestures > 0)
        {
            for (int i = 0; i < data.Gestures.Length; i++)
            {
                string newEntry =
                    "User " + data.Gestures[i].UserID + ": " +
                    Enum.GetName(typeof(nuitrack.GestureType), (int)data.Gestures[i].Type);
                exceptionsLogger.AddEntry(newEntry);
            }
        }
    }
}
