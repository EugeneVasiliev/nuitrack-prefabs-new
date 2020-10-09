using System.Collections;
using System.Collections.Generic;
//using GoogleARCore;
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
    private const float modelRotation = 180.0f; // поворачиваем, чтобы environment был повернут лицевой стороной к камере

    // A list to hold all planes ARCore is tracking in the current frame. 
    // This object is used across the application to avoid per-frame allocations.

    [SerializeField] Transform aRCoreDevice; // должен быть родителем камеры 

    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;
    Vector2 touchPosition;
    TrackableCollection<ARPlane> allPlanes;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start ()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    void Update () {
        // Hide snackbar when currently tracking at least one plane.

        allPlanes = planeManager.trackables;

        bool showSearchingUI = true;

        showSearchingUI = allPlanes.count == 0;
        
        //for (int i = 0; i < allPlanes.count; i++)
        //{
        //    //Если хотя бы одна поверхность трекается, то поиск прекращается
        //    if (allPlanes[i].TrackingState == TrackingState.Tracking)
        //    {
        //        showSearchingUI = false;
        //        break;
        //    }
        //}

        // Убираем или показываем надпись "Searching for surfaces..."
        searchingForPlaneUI.SetActive(showSearchingUI);

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Raycast against the location the player touched to search for planes.
        //TrackableHit hit;
        //TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
        //TrackableHitFlags.FeaturePointWithSurfaceNormal;

        environment = FindObjectOfType<Environment>();

        //// Пускаем луч к поверхности, которую нашёл ArCore
        //if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        //{
        //    var hitPose = hits[0].pose;

        //    if (spawnedObject)
        //        spawnedObject.transform.position = hitPose.position;
        //    else
        //        spawnedObject = Instantiate(gameObjecToInsanstiate, hitPose.position, hitPose.rotation);
        //}

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hits[0].hitType == TrackableType.Planes) &&
                 Vector3.Dot(mainCamera.transform.position - hits[0].pose.position,
                       hits[0].pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                // Если луч ударился в правильную (не обратную) часть поверхности, то сразу проверяем, нет ли по пути ворот.  Если поверхность "пустая", то ставим ворота на неё. Если по пути встречаются ворота, то "пинаем мяч"
                if (KickBall() == false && environment)
                {
                    environment.transform.position = hits[0].pose.position;
                    environment.transform.rotation = hits[0].pose.rotation;
                    environment.transform.Rotate(0, modelRotation, 0, Space.Self);
                }
            }
        }
        else
        {
            // Если по пути луча нет ArCore поверхностей, но есть ворота, то "пинаем мяч"
            KickBall();
        }
    }
    // Если можно пнуть мяч, то пинаем и возвращаем true, если нет, то возвращаем false
    bool KickBall()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitRay;

        //Пускаем стандартный луч Unity
        if (Physics.Raycast(ray, out hitRay, 100) && environment)
        {
            //Отправляем сообщение о "пинке" на сервер
            mainCamera.transform.parent = environment.transform; //Временно делаем камеру дочерним объектом для нашего "окружения". Это нужно чтобы получить её локальные координаты относительно игрового объекта "Окружение" (GameObject environment)
            environment.aim.position = hitRay.point;
            FindObjectOfType<PlayerController>().Kick(mainCamera.transform.localPosition, environment.aim.transform.localPosition);

            mainCamera.transform.parent = aRCoreDevice.transform; //возвращаем камеру обратно
            return true;
        }
        return false;
    }
}
