using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    any,
    male,
    female
}

public enum AgeType
{
    any,
    kid,
    young,
    adult,
    senior
}

public enum EmotionType
{
    any,
    happy,
    surprise,
    neutral,
    angry
}

public class FaceManager : MonoBehaviour {
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject faceController;
    [SerializeField] SkeletonController skeletonController;
    List<FaceController> faceControllers = new List<FaceController>();
    Instances[] faces;

    FaceInfo faceInfo;    

    void Start()
    {
        for (int i = 0; i < skeletonController.skeletonCount; i++)
        {
            faceControllers.Add(Instantiate(faceController, canvas.transform).GetComponent<FaceController>());
        }
    }

    void Update ()
    {
        string json = nuitrack.Nuitrack.GetInstancesJson();
        faceInfo = JsonUtility.FromJson<FaceInfo>(json.Replace("\"\"", "[]"));

        faces = faceInfo.Instances;
        for (int i = 0; i < faceControllers.Count; i++)
        {
            if(faces != null && i < faces.Length)
            {
                int id = 0;
                Face currentFace = faces[i].face;
                //передаём "лицо" в FaceController
                faceControllers[i].SetFace(currentFace);
                faceControllers[i].gameObject.SetActive(true);

                //id лиц и id скелетов совпадают
                id = faces[i].id;

                nuitrack.Skeleton skeleton = null;
                if (NuitrackManager.SkeletonData != null)
                    skeleton = NuitrackManager.SkeletonData.GetSkeletonByID(id);

                if(skeleton != null)
                {
                    nuitrack.Joint head = skeleton.GetJoint(nuitrack.JointType.Head);
                    faceControllers[i].transform.position = new Vector2(head.Proj.X * Screen.width, Screen.height - head.Proj.Y * Screen.height);
                }
            }
            else
            {
                faceControllers[i].gameObject.SetActive(false);
            }
        }
    }
}
