using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFullscreen : MonoBehaviour
{
    Camera camera;

    [SerializeField] GameObject[] hideObjects;

    bool fullscreen;
    Rect defaultRect;

    private void Start()
    {
        camera = GetComponent<Camera>();
        defaultRect = camera.rect;
    }

    public void SwitchFullscreen()
    {
        fullscreen = !fullscreen;

        if (fullscreen)
        {
            camera.rect = new Rect(0, 0, 1, 1);
        }
        else
        {
            camera.rect = defaultRect;
        }

        for (int i = 0; i < hideObjects.Length; i++)
        {
            hideObjects[i].SetActive(!fullscreen);
        }
    }
}
