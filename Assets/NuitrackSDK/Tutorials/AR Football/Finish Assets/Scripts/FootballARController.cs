#if ENABLE_AR_TUTORIAL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FootballARController : MonoBehaviour {
    public Camera mainCamera;

    // A model to place when a raycast from a user touch hits a plane.
    Environment environment;

    // A gameobject parenting UI for displaying the "searching for planes" snackbar.
    public GameObject searchingForPlaneUI;

    // The rotation in degrees need to apply to model when model is placed.
    private const float modelRotation = 180.0f; // rotate so that the environment is facing the camera

    // A list to hold all planes AR is tracking in the current frame. 
    TrackableCollection<ARPlane> allPlanes;

    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;

    [SerializeField] Transform aRDevice; // must be the parent of the camera

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start ()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    void Update () {
        allPlanes = planeManager.trackables;

        bool showSearchingUI = true;

        showSearchingUI = allPlanes.count == 0;

        // Hide or show the inscription "Searching for surfaces ..."
        searchingForPlaneUI.SetActive(showSearchingUI);

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        environment = FindObjectOfType<Environment>();

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitRay;

        if (Physics.Raycast(ray, out hitRay, 100))
        {
            if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon) && environment)
            {
                // If the beam hits the correct (not the opposite) part of the surface, then we immediately check if there is a gate along the way. If the surface is "empty", then we put the gate on it. If on the way there are gates, then "kick the ball"
                if (hitRay.transform.name.Contains("ARPlane"))
                {
                    environment.transform.position = hits[0].pose.position;
                    environment.transform.rotation = hits[0].pose.rotation;
                    environment.transform.Rotate(0, modelRotation, 0, Space.Self);
                }
                else
                {
                    // If there are no surfaces along the path of the beam, but there is a gate, then "kick the ball"
                    KickBall(hitRay.point);
                }
            }
            else
            {
                KickBall(hitRay.point);
            }
        }
    }
    // If you can kick the ball, then kick it and return true, if not, then return false
    void KickBall(Vector3 targetPos)
    {
        //Sending a "kick" message to the server
        mainCamera.transform.parent = environment.transform; //We temporarily make the camera a child object for our "environment". This is necessary to get its local coordinates relative to the game object "Environment" (GameObject environment)
        environment.aim.position = targetPos;
        FindObjectOfType<PlayerController>().Kick(mainCamera.transform.localPosition, environment.aim.transform.localPosition);

        mainCamera.transform.parent = aRDevice.transform; //return the camera back
    }
}
#endif