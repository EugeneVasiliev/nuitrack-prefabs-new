using UnityEngine;
using System;

public class GesturesVisualization : MonoBehaviour
{
    ExceptionsLogger exceptionsLogger;
    NuitrackModules nuitrackModules;
    nuitrack.GestureData gesturesData = null;

    void Start()
    {
        exceptionsLogger = FindObjectOfType<ExceptionsLogger>();
        nuitrackModules = FindObjectOfType<NuitrackModules>();
    }

    void Update()
    {
        if (NuitrackManager.GestureRecognizer != null)
        {
            if (gesturesData != NuitrackManager.GestureRecognizer.GetGestureData())
            {
                gesturesData = NuitrackManager.GestureRecognizer.GetGestureData();
                ProcessGesturesData(gesturesData);
            }
        }
    }

    void ProcessGesturesData(nuitrack.GestureData data)
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
