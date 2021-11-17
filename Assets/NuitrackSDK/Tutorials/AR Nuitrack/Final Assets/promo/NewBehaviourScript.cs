using System;
using System.Collections;
using System.Collections.Generic;
using nuitrack;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    GameObject ff;
    // Start is called before the first frame update
    void Start()
    {
        NuitrackManager.onNewGesture += dd;
        print("Start");
    }

    private void dd(Gesture gesture)
    {
        print(ff.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
